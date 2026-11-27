using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T5 pb (I-64, fifth batch; the prototype «pushback»): a Push Back of the list edited in the window, with its numbered door, as Dinámico,
    /// Selectivo and the cabecera: «Editar aquí» asks AutoCAD to read it (RACKPUSHBACKABRIR, no undo marker), the editor opens on the answer
    /// to THAT request, and «Actualizar» hands COPIES of the draft to RACKPUSHBACKAPLICAR, which redraws its views in place (one «U»).
    /// «Insertar vista…» is RACKPUSHBACKINSERTAR, and «Push Back en esta ventana» of «Nuevo» is a draft («Borrador N») inserted with
    /// RACKPUSHBACKNUEVO. The one request in flight, «Dejar de esperar», the request the command already took, a draft of another drawing,
    /// the prune of a closed drawing and the veto of the close (D-08) are the rules of the window.
    /// </summary>
    public sealed partial class RackCadWindowViewModel
    {
        private IRackCadPushBackDoorPort _pushBackDoor;

        // The Push Back racks opened in the window, with the key and the life of the other drafts.
        private readonly Dictionary<(SessionId Session, string RackId), PushBackDraftEntry> _pushBackDrafts = new(DraftKeyComparer.Instance);

        private sealed class PushBackDraftEntry
        {
            public PushBackDraftEntry(SessionId session, PushBackEditorViewModel editor, string listedName = null)
            {
                Session = session;
                Editor = editor;
                ListedName = string.IsNullOrWhiteSpace(listedName) ? null : listedName.Trim();
            }

            public SessionId Session { get; }

            public PushBackEditorViewModel Editor { get; }

            public string ListedName { get; }
        }

        // One line of the constructor: a Push Back is edited here only through a door that reads and writes it.
        private void WirePushBackDoor(IRackCadRackDoorPort door)
        {
            _pushBackDoor = door as IRackCadPushBackDoorPort;
            if (_pushBackDoor != null) _pushBackDoor.PushBackRead += OnPushBackRead;
        }

        /// <summary>Whether the active rack is a Push Back, by the Kind of the list of the drawing.</summary>
        public bool IsPushBackRack =>
            string.Equals(RackList.KindOf(Panel.Browser.ActiveRackId), RackEmbedDocument.KindPushBack, StringComparison.OrdinalIgnoreCase);

        /// <summary>The editor of the active Push Back in the window, when it was opened here; null otherwise.</summary>
        public PushBackEditorViewModel ActivePushBackEditor =>
            Panel.PresentedSession.HasValue && Panel.Browser.ActiveRackId != null
            && _pushBackDrafts.TryGetValue((Panel.PresentedSession.Value, Panel.Browser.ActiveRackId), out var entry)
            && !entry.Editor.IsNewRack
                ? entry.Editor
                : null;

        public bool HasPushBackEditor => ActivePushBackEditor != null;

        /// <summary>Every Push Back editor that the window keeps open, of any drawing.</summary>
        public IReadOnlyList<PushBackEditorViewModel> PushBackEditors => _pushBackDrafts.Values.Select(e => e.Editor).ToList();

        /// <summary>«Editar aquí» of a Push Back: a Push Back with no editor open yet, and a door that reads and writes a Push Back.</summary>
        public bool ShowPushBackOffer => IsPushBackRack && ActivePushBackEditor == null && _pushBackDoor != null;

        /// <summary>The new Push Back that «Nuevo» shows: the editor of the active draft when it is a Push Back; null otherwise.</summary>
        public PushBackEditorViewModel NewPushBackEditor => ActiveDraftEditor as PushBackEditorViewModel;

        public bool ShowNewPushBackEditor => IsNuevoPage && NewPushBackEditor != null;

        /// <summary>A new Push Back can be made here: the window has the door of Push Back and a drawing.</summary>
        public bool CanStartNewPushBack => _pushBackDoor != null && Panel.PresentedSession.HasValue;

        /// <summary>«Push Back en esta ventana» of «Nuevo»: a new draft («Borrador N») with the defaults of the classic editor.</summary>
        public void StartNewPushBack() => OpenAsNewDraft(RackCadDraftSeed.Defaults(RackCad.Domain.Systems.Shared.RackSystemKind.PushBack));

        /// <summary>«Editar aquí» of a Push Back: asks AutoCAD to read it (RACKPUSHBACKABRIR); the one request of the door is shared.</summary>
        public void EditPushBackHere()
        {
            var rack = Panel.Browser.ActiveRackId;
            var session = Panel.PresentedSession;
            if (_pushBackDoor == null || rack == null || !session.HasValue) return;
            if (_inFlight != null)
            {
                Say(DoorBusyText, failed: true);
                return;
            }

            // The request waits BEFORE it is handed over: AutoCAD may run the command, and answer, during the call (M-22).
            var id = ++_lastRequestId;
            _inFlight = new DoorRequest(id, false, session.Value, rack, null, null, IsPushBack: true);
            Say("Leyendo el rack en AutoCAD…", failed: false);
            RaiseWaiting();

            RackCadClassicResult result;
            try
            {
                result = _pushBackDoor.RequestPushBackDesign(new RackDoorRequest(id, session.Value, rack))
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

        // The answer of AutoCAD to «Editar aquí» of a Push Back: only the answer to the Push Back read that waits.
        private void OnPushBackRead(RackDesignRead read)
        {
            Contained(() =>
            {
                var request = _inFlight;
                if (read == null || request == null || request.IsWrite || !request.IsPushBack || read.RequestId != request.Id) return;
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
                if (project?.PushBackDesign == null)
                {
                    Say("El rack no es un Push Back que este editor pueda abrir; usa RACKEDITAR.", failed: true);
                    return;
                }

                var key = (request.Session, request.RackId);
                if (_pushBackDrafts.TryGetValue(key, out var existing) && (existing.Editor.IsDirty || existing.Editor.IsWriting))
                {
                    Say("Este rack ya tiene un borrador con cambios en la ventana: no se reemplazó.", failed: false);
                    RaiseDynamic();
                    return;
                }

                if (existing != null) ForgetPushBack(key, existing);

                // A design that cannot be recomposed still opens the editor, with the reason (the classic editor shows it too).
                var draft = new PushBackDraftEditor(_catalog());
                draft.LoadFrom(project.PushBackDesign, read.Name);
                var editor = new PushBackEditorViewModel(draft, request.RackId, UpdatePushBack, readDesignJson: read.DesignJson);
                editor.LoadProperties(read.Properties); // VA-07
                WirePushBack(editor);
                _pushBackDrafts[key] = new PushBackDraftEntry(request.Session, editor, RackList.ListedNameOf(request.RackId));
                _showPanel = false;
                Say(string.Empty, failed: false);
                RaiseDynamic();
            });
        }

        // What every Push Back editor of the window gets: the panel «Insertar vista…», the writes, «Más» and the close.
        private void WirePushBack(PushBackEditorViewModel editor)
        {
            editor.ExistingViewKinds = () => RackList.ViewKindsOf(editor.RackId);
            editor.OtherRequestText = OtherRequestText;
            editor.OtherDrawingText = () => OtherDrawingText(_pushBackDrafts.Values.FirstOrDefault(e => ReferenceEquals(e.Editor, editor))?.Session);
            editor.InsertViewsAction = InsertPushBackViews;
            editor.InsertNewAction = InsertNewPushBack;
            editor.SaveToLibraryAction = StartSaveToLibrary;
            editor.PropertyChanged += OnPushBackEditorChanged;
            editor.CloseRequested += ClosePushBackEditor;
        }

        private void OnPushBackEditorChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshDraftOf(sender, e.PropertyName);
            if (e.PropertyName != nameof(PushBackEditorViewModel.IsDirty) && e.PropertyName != nameof(PushBackEditorViewModel.IsWriting)) return;
            RackList.RefreshStates();
            OnPropertyChanged(nameof(HeaderText));
        }

        private void UpdatePushBack(PushBackEditorViewModel editor) => WritePushBack(editor, null);

        private void InsertPushBackViews(PushBackEditorViewModel editor, IReadOnlyList<RackViewAddress> views)
        {
            if (views == null || views.Count == 0) return;
            WritePushBack(editor, views);
        }

        private void InsertNewPushBack(PushBackEditorViewModel editor, IReadOnlyList<RackViewAddress> views)
        {
            if (views == null || views.Count == 0 || !editor.IsNewRack) return;
            WritePushBack(editor, views);
        }

        // «Actualizar», «Insertar vista…» and «Insertar en el dibujo…» of a Push Back: COPIES of the draft go to AutoCAD. The editor is locked
        // until the answer arrives or the window stops waiting, so what is written is what the user saw.
        private void WritePushBack(PushBackEditorViewModel editor, IReadOnlyList<RackViewAddress> insertViews)
        {
            if (_pushBackDoor == null)
            {
                editor.Say("Aquí no se puede escribir el dibujo.", error: true);
                return;
            }

            var entry = _pushBackDrafts.Values.FirstOrDefault(e => ReferenceEquals(e.Editor, editor));
            if (entry == null) return;

            if (!Panel.PresentedSession.HasValue || entry.Session != Panel.PresentedSession.Value)
            {
                editor.Say(OtherDrawingWriteText, error: true);
                return;
            }

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

            var id = ++_lastRequestId;
            _inFlight = new DoorRequest(id, true, entry.Session, editor.RackId, snapshot.DesignJson, snapshot.Name, IsPushBack: true);
            editor.BeginWriting();
            RaiseWaiting();

            RackCadClassicResult result;
            try
            {
                result = _pushBackDoor.RequestPushBackUpdate(new PushBackDoorWrite(id, entry.Session, editor.RackId, snapshot.Name, snapshot.Design, snapshot.System,
                             insertViews == null ? null : new List<RackViewAddress>(insertViews), NewRack: editor.IsNewRack,
                             Properties: insertViews == null || editor.IsNewRack ? editor.PropertiesWrite() : null)) // T6 pr: and a new rack's (VA-S9)
                         ?? RackCadClassicResult.Without(RackCadClassicStatus.Failed, "sin respuesta.");
            }
            catch (Exception ex)
            {
                result = RackCadClassicResult.Without(RackCadClassicStatus.Failed, ex.Message);
            }

            if (result.IsStarted)
            {
                editor.WriteHandedOver();
                return;
            }

            if (_inFlight != null && _inFlight.Id == id)
            {
                _inFlight = null;
                editor.EndWriting();
            }

            RaiseWaiting();
            editor.Say(RefusalText(result), error: true);
        }

        // The answer to a write of a Push Back (one line of OnDesignWritten): what was SENT is the drawing now when AutoCAD wrote it.
        private void PushBackWritten(DoorRequest request, RackDesignWritten written)
        {
            if (!_pushBackDrafts.TryGetValue((request.Session, request.RackId), out var entry)) return;
            var editor = entry.Editor;
            editor.WriteHandedOver();
            editor.EndWriting();
            if (written.Written) editor.MarkWritten(written.Message, request.SentJson, request.SentName);
            else editor.Say(written.Message, error: true);
            if (!written.Written) EchoHiddenDraft(editor, written.Message, failed: true);
            editor.PropertiesWritten(written);

            // A new Push Back that AutoCAD placed is a rack of the drawing now: «Nuevo» shows its systems again and says where it is.
            if (written.Written && editor.IsNewRack)
            {
                editor.MarkInserted(written.Name);
                Say(written.Message, failed: false);
                RaiseDynamic();
            }
        }

        // «Dejar de esperar» of a write of a Push Back (one line of StopWaiting): the editor can be edited again, not written.
        private void StopPushBackWrite(DoorRequest request, string text)
        {
            if (request.IsWrite && _pushBackDrafts.TryGetValue((request.Session, request.RackId), out var entry))
            {
                entry.Editor.EndWriting();
                entry.Editor.Say(text, error: false);
                EchoHiddenDraft(entry.Editor, text, failed: false);
            }
            else if (!request.IsWrite)
            {
                Say("Se dejó de esperar la lectura: si la orden llega a correr, no abrirá nada.", failed: false);
            }
        }

        // The command of the write took its request (one line of OnRequestStarted).
        private void PushBackWriteTaken(DoorRequest request)
        {
            if (request.IsPushBack && _pushBackDrafts.TryGetValue((request.Session, request.RackId), out var entry)) entry.Editor.WriteTaken();
        }

        private void ClosePushBackEditor(PushBackEditorViewModel editor)
        {
            Contained(() =>
            {
                foreach (var pair in _pushBackDrafts.Where(p => ReferenceEquals(p.Value.Editor, editor)).ToList())
                {
                    if (pair.Value.Editor.IsWriting) return;
                    ForgetPushBack(pair.Key, pair.Value);
                }

                RaiseDynamic();
                OnPropertyChanged(nameof(HeaderText));
            });
        }

        private void ForgetPushBack((SessionId Session, string RackId) key, PushBackDraftEntry entry)
        {
            entry.Editor.PropertyChanged -= OnPushBackEditorChanged;
            entry.Editor.CloseRequested -= ClosePushBackEditor;
            _pushBackDrafts.Remove(key);
            ForgetUtilitiesOf(entry.Editor);
        }

        // The name of a Push Back in the notices: a draft of «Nuevo» by its own name, then its name, the one of the list, its id.
        private string PushBackNameOf(PushBackDraftEntry entry) =>
            DraftNoticeName(entry.Editor) is string draft ? draft
            : !string.IsNullOrWhiteSpace(entry.Editor.Name) ? entry.Editor.Name.Trim()
            : entry.Editor.IsNewRack ? "un Push Back nuevo"
            : entry.ListedName ?? entry.Editor.RackId;

        // D-08: the Push Back racks of the session with changes not written (or being written).
        private IEnumerable<string> UnwrittenPushBackNames(SessionId session)
            => _pushBackDrafts.Where(p => p.Key.Session == session && (p.Value.Editor.IsDirty || p.Value.Editor.IsWriting)).Select(p => PushBackNameOf(p.Value));

        // The Push Back racks of a drawing that was closed are forgotten, and work that was lost is said.
        private void PrunePushBackDrafts(IWorkspaceBrowserSummaryPort port)
        {
            var dead = _pushBackDrafts.Where(p => !IsAlive(port, p.Key.Session)).ToList();
            if (dead.Count == 0) return;
            foreach (var pair in dead)
            {
                // T5 integration (cm's prune): the name goes to LostOnPrune, said once with what the other prunes lost.
                if (pair.Value.Editor.IsDirty || pair.Value.Editor.IsWriting) LostOnPrune(PushBackNameOf(pair.Value));
                if (_inFlight != null && _inFlight.Session == pair.Key.Session) AbandonInFlight();
                ForgetPushBack(pair.Key, pair.Value);
            }

            RaiseWaiting();
            RaisePushBack();
        }

        // The state of a Push Back in the list while its draft is open in the window (the marks of the other drafts).
        private bool PushBackDraftState(SessionId session, string rackId, out bool writing, out bool dirty)
        {
            writing = dirty = false;
            if (!_pushBackDrafts.TryGetValue((session, rackId), out var entry)) return false;
            writing = entry.Editor.IsWriting;
            dirty = entry.Editor.IsDirty;
            return true;
        }

        // The open panels «Insertar vista…» of the Push Back editors follow the door (one line of RefreshInsertPanels).
        private void RefreshPushBackInsertPanels()
        {
            foreach (var entry in _pushBackDrafts.Values) entry.Editor.RefreshInsertState();
        }

        // «Más» › «Guardar en biblioteca…» of a Push Back editor: the page of the library with the line of the save.
        private void StartSaveToLibrary(PushBackEditorViewModel editor)
        {
            Contained(() => BeginLibrarySave(new RackCadLibrarySaveSource(editor, editor.Name, "Push Back",
                () => ChangesNote(editor.IsNewRack, editor.IsDirty), name => BuildPushBackProject(editor), editor.SaveRefusal)));
        }

        // What is saved for a Push Back: the design of the draft, as «Guardar en biblioteca» of the classic editor saves it (its document keeps
        // no name: the library names it by its file).
        private (RackProject Project, string Error) BuildPushBackProject(PushBackEditorViewModel editor)
        {
            var snapshot = editor.Draft.SnapshotForWrite();
            if (snapshot == null) return (null, "no hay un borrador válido que guardar.");
            _librarySources.TryGetValue(editor, out var source);
            return (RackProject.ForPushBack(snapshot.Design).WithSourceMetadataFrom(source), null);
        }

        // The BOM of «BOM y lista de racks» for a Push Back with changes not written: the BOM of its handler over a copy (BM-02).
        private BillOfMaterials PushBackDraftBom(SessionId session, string rackId)
        {
            if (!_pushBackDrafts.TryGetValue((session, rackId), out var entry) || entry.Editor.IsNewRack || !entry.Editor.IsDirty) return null;

            // The gate of the output (I-42, as OutputBlockedReason of the BOM of the drawing): a blocked draft, or one that cannot be
            // recomposed, has no BOM to count (review of 24adfee7, point 2).
            if (entry.Editor.Draft.IsOutputBlocked || !string.IsNullOrWhiteSpace(entry.Editor.Draft.LastError)) return null;
            var snapshot = entry.Editor.Draft.SnapshotForWrite();
            return snapshot?.System == null ? null : PushBackBomBuilder.Build(snapshot.System, entry.Editor.Draft.Catalog);
        }

        private bool HasUnwrittenPushBack(SessionId session, string rackId)
            => _pushBackDrafts.TryGetValue((session, rackId), out var entry) && !entry.Editor.IsNewRack && entry.Editor.IsDirty;

        private IEnumerable<(string RackId, string Name)> UnwrittenPushBackDrafts(SessionId session)
            => _pushBackDrafts.Where(p => p.Key.Session == session && !p.Value.Editor.IsNewRack && p.Value.Editor.IsDirty)
                .Select(p => (p.Key.RackId, !string.IsNullOrWhiteSpace(p.Value.Editor.Name) ? p.Value.Editor.Name.Trim() : RackList.NameOf(p.Key.RackId)));

        // The new draft of «Nuevo» of a Push Back (a case of CreateDraftEditor): the defaults of the classic editor, or a design.
        private PushBackEditorViewModel CreatePushBackDraft(RackCadDraftSeed seed, SessionId session, string rackId, string label, out string failure)
        {
            failure = null;
            PushBackDraftEditor draft;
            if (seed.Project == null)
            {
                draft = PushBackDraftEditor.CreateNew(_catalog());
            }
            else
            {
                var design = CopyOfPushBackDesign(seed.Project.PushBackDesign);
                if (design == null)
                {
                    failure = "El diseño no tiene un Push Back que abrir.";
                    return null;
                }

                // PB-S10 also here (review of 24adfee7, point 6): a design that keeps the dormant half of a side B (a library one, or the copy
                // of «Duplicar») is outside the guarantee of the editable state, as the door refuses it.
                if (IsOutsideTheGuarantee(design) || IsOutsideTheGuarantee(seed.Edit?.PushBackDesign))
                {
                    failure = DormantTailText;
                    return null;
                }

                draft = new PushBackDraftEditor(_catalog());
                if (!draft.LoadFrom(design, seed.Name))
                {
                    failure = "No se pudo abrir el diseño como borrador: " + (string.IsNullOrWhiteSpace(draft.LastError) ? "sin detalle." : draft.LastError);
                    return null;
                }
            }

            var editor = new PushBackEditorViewModel(draft, rackId, UpdatePushBack, isNewRack: true);
            if (seed.Edit != null && !editor.TakeDesignAsEdit(CopyOfPushBackDesign(seed.Edit.PushBackDesign)))
            {
                failure = "No se pudo copiar el diseño.";
                return null;
            }

            WirePushBack(editor);
            _pushBackDrafts[(session, rackId)] = new PushBackDraftEntry(session, editor);
            if (seed.Project != null) editor.Say(OpenedText(label, seed, null, null), error: false);
            return editor;
        }

        // What the door says of a design with the dormant half of a side B (PushBackDoorRules.AnswerForOpen), for «Nuevo» from a design.
        private const string DormantTailText = "Este Push Back guarda la mitad dormida de un lado B que la ventana todavía no edita." + PushBackDoorRules.UseClassicText;

        private static bool IsOutsideTheGuarantee(RackCad.Domain.Systems.PushBack.PushBackDesign design)
            => design != null && PushBackEditableState.SupportOf(design) == PushBackEditableSupport.DormantCompositeTail;

        private static RackCad.Domain.Systems.PushBack.PushBackDesign CopyOfPushBackDesign(RackCad.Domain.Systems.PushBack.PushBackDesign design)
        {
            if (design == null) return null;
            var store = new RackProjectStore();
            return store.Deserialize(store.Serialize(RackProject.ForPushBack(design)))?.PushBackDesign;
        }

        // A draft of «Nuevo» of a Push Back is live while its editor is a new rack in the dictionary.
        private bool IsLivePushBack(object editor, SessionId session, string rackId)
            => editor is PushBackEditorViewModel pushBack && pushBack.IsNewRack
               && _pushBackDrafts.TryGetValue((session, rackId), out var entry) && ReferenceEquals(entry.Editor, pushBack);

        // For «Duplicar»: the last valid design of a draft as a project, and the base of its «Modificado».
        private static (RackProject Base, RackProject Copied) CopyOfPushBack(PushBackEditorViewModel editor)
        {
            var copy = editor.Draft.SnapshotForWrite();
            var baseline = editor.BaselineDesignJson == null ? null : new RackProjectStore().Deserialize(editor.BaselineDesignJson);
            return (baseline?.PushBackDesign == null ? null : baseline, copy == null ? null : RackProject.ForPushBack(copy.Design));
        }

        private void RaisePushBack()
        {
            OnPropertyChanged(nameof(IsPushBackRack));
            OnPropertyChanged(nameof(ActivePushBackEditor));
            OnPropertyChanged(nameof(HasPushBackEditor));
            OnPropertyChanged(nameof(PushBackEditors));
            OnPropertyChanged(nameof(ShowPushBackOffer));
            OnPropertyChanged(nameof(NewPushBackEditor));
            OnPropertyChanged(nameof(ShowNewPushBackEditor));
            OnPropertyChanged(nameof(CanStartNewPushBack));
        }
    }
}
