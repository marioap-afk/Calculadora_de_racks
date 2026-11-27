using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.Domain.Systems.Shared;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T5 cm (I-64, quinta tanda; prototype «camas», every question accepted): the camas de rodamiento in the window, as Dinámico, Selectivo
    /// and the cabecera of their own. «Editar aquí» of a cama of the list asks AutoCAD to read it (RACKCAMAABRIR, no undo marker), the editor
    /// opens on the answer to THAT request, and «Actualizar» hands a COPY to RACKCAMAAPLICAR, which redraws its block in place (one «U»). «Cama
    /// en esta ventana» of «Nuevo» is a draft («Borrador N», SD-06) that «Insertar en el dibujo…» places with RACKCAMANUEVO (the jig and the
    /// automatic name of I-60). The one request in flight, «Dejar de esperar», the request the command took, a draft of another drawing, the
    /// prune of a closed drawing and the veto of the close (D-08) are the rules of the window. A cama has one view, so it has no «Insertar
    /// vista…» (question 5; DG-07: the panel has no views for it). The library opens a cama as a draft and saves the one of the editor; the
    /// page of the BOM shows the BOM of the draft of an editor with changes.
    /// </summary>
    public sealed partial class RackCadWindowViewModel
    {
        /// <summary>The name of a new cama with no name in the notices, as «un Dinámico nuevo».</summary>
        internal const string NewCamaName = "una cama nueva";

        private IRackCadCamaDoorPort _camaDoor;

        // The camas opened in the window and the new ones of «Nuevo», with the key and the life of the other drafts.
        private readonly Dictionary<(SessionId Session, string RackId), CamaDraftEntry> _camaDrafts = new(DraftKeyComparer.Instance);

        private sealed class CamaDraftEntry
        {
            public CamaDraftEntry(SessionId session, CamaEditorViewModel editor, string listedName = null)
            {
                Session = session;
                Editor = editor;
                ListedName = string.IsNullOrWhiteSpace(listedName) ? null : listedName.Trim();
            }

            public SessionId Session { get; }

            public CamaEditorViewModel Editor { get; }

            public string ListedName { get; }
        }

        // One line of the constructor: a cama is edited here only through a door that reads and writes it.
        private void WireCamaDoor(IRackCadRackDoorPort door)
        {
            _camaDoor = door as IRackCadCamaDoorPort;
            if (_camaDoor != null) _camaDoor.CamaRead += OnCamaRead;
        }

        // ------------------------------------------------------------------ a cama of the drawing

        /// <summary>Whether the active rack is a cama de rodamiento, by the Kind of the list of the drawing.</summary>
        public bool IsCamaRack =>
            string.Equals(RackList.KindOf(Panel.Browser.ActiveRackId), RackEmbedDocument.KindCama, StringComparison.OrdinalIgnoreCase);

        /// <summary>The editor of the active cama in the window, when it was opened here; null otherwise.</summary>
        public CamaEditorViewModel ActiveCamaEditor =>
            Panel.PresentedSession.HasValue && Panel.Browser.ActiveRackId != null
            && _camaDrafts.TryGetValue((Panel.PresentedSession.Value, Panel.Browser.ActiveRackId), out var entry)
            && !entry.Editor.IsNewRack
                ? entry.Editor
                : null;

        public bool HasCamaEditor => ActiveCamaEditor != null;

        /// <summary>Every cama editor that the window keeps open, of any drawing (the new ones of «Nuevo» too).</summary>
        public IReadOnlyList<CamaEditorViewModel> CamaEditors => _camaDrafts.Values.Select(e => e.Editor).ToList();

        /// <summary>«Editar aquí» of a cama: a cama with no editor open yet, and a door that reads and writes a cama.</summary>
        public bool ShowCamaOffer => IsCamaRack && ActiveCamaEditor == null && _camaDoor != null;

        /// <summary>«Editar aquí» of a cama: asks AutoCAD to read it (RACKCAMAABRIR); the one request of the door is shared.</summary>
        public void EditCamaHere()
        {
            var rack = Panel.Browser.ActiveRackId;
            var session = Panel.PresentedSession;
            if (_camaDoor == null || rack == null || !session.HasValue) return;
            if (_inFlight != null)
            {
                Say(DoorBusyText, failed: true);
                return;
            }

            // The request waits BEFORE it is handed over: AutoCAD may run the command, and answer, during the call (M-22).
            var id = ++_lastRequestId;
            _inFlight = new DoorRequest(id, false, session.Value, rack, null, null, IsCama: true);
            Say("Leyendo la cama en AutoCAD…", failed: false);
            RaiseWaiting();

            RackCadClassicResult result;
            try
            {
                result = _camaDoor.RequestCamaDesign(new RackDoorRequest(id, session.Value, rack))
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

        // The answer of AutoCAD to «Editar aquí» of a cama: only the answer to the cama read that waits.
        private void OnCamaRead(CamaDesignRead read)
        {
            Contained(() =>
            {
                var request = _inFlight;
                if (read == null || request == null || request.IsWrite || !request.IsCama || read.RequestId != request.Id) return;
                _inFlight = null;
                RaiseWaiting();

                if (!read.Opens)
                {
                    Say("No se pudo abrir la cama: " + (read.Reason ?? "sin detalle."), failed: true);
                    return;
                }

                if (!string.Equals(read.RackId, request.RackId, StringComparison.OrdinalIgnoreCase))
                {
                    Say("AutoCAD leyó otro rack que el pedido: no se abrió nada.", failed: true);
                    return;
                }

                var document = read.Document();
                var bed = document?.ToDomain();
                if (bed == null)
                {
                    Say("La cama no se puede abrir en este editor; usa RACKEDITAR.", failed: true);
                    return;
                }

                var key = (request.Session, request.RackId);
                if (_camaDrafts.TryGetValue(key, out var existing) && (existing.Editor.IsDirty || existing.Editor.IsWriting))
                {
                    Say("Esta cama ya tiene un borrador con cambios en la ventana: no se reemplazó.", failed: false);
                    RaiseDynamic();
                    return;
                }

                if (existing != null) ForgetCama(key, existing);
                var editor = new CamaEditorViewModel(new CamaDraftEditor(_catalog(), bed, read.Name, document), request.RackId, UpdateCama);
                editor.LoadProperties(read.Properties); // VA-07
                WireCama(editor);
                _camaDrafts[key] = new CamaDraftEntry(request.Session, editor, RackList.ListedNameOf(request.RackId));
                _showPanel = false;
                Say(string.Empty, failed: false);
                RaiseDynamic();
            });
        }

        // What every cama editor of the window gets: its writes, «Guardar en biblioteca…» and the close.
        private void WireCama(CamaEditorViewModel editor)
        {
            editor.InsertNewAction = InsertNewCama;
            editor.SaveToLibraryAction = StartSaveToLibrary;
            editor.PropertyChanged += OnCamaEditorChanged;
            editor.CloseRequested += CloseCamaEditor;
        }

        private void OnCamaEditorChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshDraftOf(sender, e.PropertyName); // the item of a draft of «Nuevo» follows its name and its state
            if (e.PropertyName != nameof(CamaEditorViewModel.IsDirty) && e.PropertyName != nameof(CamaEditorViewModel.IsWriting)) return;
            RackList.RefreshStates();
            OnPropertyChanged(nameof(HeaderText));
        }

        // «Actualizar» of a cama of the drawing and «Insertar en el dibujo…» of a new one: the same write, a COPY of the bed to AutoCAD; the
        // editor is locked until the answer arrives or the window stops waiting.
        private void UpdateCama(CamaEditorViewModel editor) => WriteCama(editor);

        private void InsertNewCama(CamaEditorViewModel editor)
        {
            if (editor.IsNewRack) WriteCama(editor);
        }

        private void WriteCama(CamaEditorViewModel editor)
        {
            if (_camaDoor == null)
            {
                editor.Say("Aquí no se puede escribir el dibujo.", error: true);
                return;
            }

            var entry = _camaDrafts.Values.FirstOrDefault(e => ReferenceEquals(e.Editor, editor));
            if (entry == null) return;

            // The door writes in the drawing that is active in AutoCAD: a draft of another drawing waits for its drawing.
            if (!Panel.PresentedSession.HasValue || entry.Session != Panel.PresentedSession.Value)
            {
                editor.Say("Este borrador es de otro dibujo: vuelve a ese dibujo para escribirlo.", error: true);
                return;
            }

            if (_inFlight != null)
            {
                editor.Say(DoorBusyText, error: true);
                return;
            }

            var bed = editor.Draft.Snapshot();
            var name = (editor.Name ?? string.Empty).Trim();
            var sent = new FlowBedConfigurationStore().Serialize(bed);
            var source = editor.IsNewRack ? SourceDesignJsonOf(editor) : null;

            // The request waits, and the editor is locked, BEFORE it is handed over: AutoCAD may run the command during the call (M-22).
            var id = ++_lastRequestId;
            _inFlight = new DoorRequest(id, true, entry.Session, editor.RackId, sent, name, IsCama: true);
            editor.BeginWriting();
            RaiseWaiting();

            RackCadClassicResult result;
            try
            {
                result = _camaDoor.RequestCamaWrite(new CamaDoorWrite(id, entry.Session, editor.RackId, name, bed, editor.IsNewRack,
                             editor.IsNewRack ? null : editor.PropertiesWrite(), source))
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

        // The document of the design of the library a new cama was opened from (its unknown fields reach the drawing, I-11), or null.
        private string SourceDesignJsonOf(CamaEditorViewModel editor)
        {
            var document = _librarySources.TryGetValue(editor, out var project) ? project?.SourceFlowBedDocument : null;
            return document == null ? null : new FlowBedConfigurationStore().SerializeDocument(document);
        }

        // The answer to the write of a cama: written = «Limpio» on what was sent; else it stays «Modificado» with the reason. A new cama that
        // AutoCAD placed is a cama of the drawing now: «Nuevo» goes back to its home and the window says how to see it in the list (D-10).
        private void CamaWritten(DoorRequest request, RackDesignWritten written)
        {
            if (!_camaDrafts.TryGetValue((request.Session, request.RackId), out var entry)) return;
            var editor = entry.Editor;
            editor.EndWriting();
            if (written.Written) editor.MarkWritten(written.Message, request.SentJson, request.SentName);
            else editor.Say(written.Message, error: true);
            if (!written.Written) EchoHiddenDraft(editor, written.Message, failed: true); // a draft not on screen says it in the window too
            editor.PropertiesWritten(written);

            if (written.Written && editor.IsNewRack)
            {
                editor.MarkInserted(written.Name);
                Say(written.Message, failed: false);
                RaiseDynamic();
            }
        }

        // «Dejar de esperar» of a write of a cama: the editor can be edited again, with its draft still not written.
        private void StopCamaWrite(DoorRequest request, string text)
        {
            if (!_camaDrafts.TryGetValue((request.Session, request.RackId), out var entry)) return;
            entry.Editor.EndWriting();
            entry.Editor.Say(text, error: false);
            EchoHiddenDraft(entry.Editor, text, failed: false);
        }

        private void CamaWriteTaken(DoorRequest request)
        {
            if (_camaDrafts.TryGetValue((request.Session, request.RackId), out var entry)) entry.Editor.WriteTaken();
        }

        // «Cerrar el editor» (confirmed by the editor when it had changes), and «Descartar el borrador» of a new cama.
        private void CloseCamaEditor(CamaEditorViewModel editor)
        {
            Contained(() =>
            {
                foreach (var pair in _camaDrafts.Where(p => ReferenceEquals(p.Value.Editor, editor)).ToList())
                {
                    if (pair.Value.Editor.IsWriting) return;
                    ForgetCama(pair.Key, pair.Value);
                }

                RaiseDynamic();
                OnPropertyChanged(nameof(HeaderText));
            });
        }

        private void ForgetCama((SessionId Session, string RackId) key, CamaDraftEntry entry)
        {
            entry.Editor.PropertyChanged -= OnCamaEditorChanged;
            entry.Editor.CloseRequested -= CloseCamaEditor;
            _camaDrafts.Remove(key);
            ForgetUtilitiesOf(entry.Editor);
        }

        // The name of a cama in the notices: a draft of «Nuevo» by its own name («Borrador N (Cama nueva)»), then the one of its editor, the one
        // of the list when it opened, and its id last.
        private string CamaNameOf(CamaDraftEntry entry) =>
            DraftNoticeName(entry.Editor) is string draft ? draft
            : !string.IsNullOrWhiteSpace(entry.Editor.Name) ? entry.Editor.Name.Trim()
            : entry.Editor.IsNewRack ? NewCamaName
            : entry.ListedName ?? entry.Editor.RackId;

        // D-08: the camas of the session with changes not written (or being written).
        private IEnumerable<string> UnwrittenCamaNames(SessionId session)
            => _camaDrafts.Where(p => p.Key.Session == session && (p.Value.Editor.IsDirty || p.Value.Editor.IsWriting)).Select(p => CamaNameOf(p.Value));

        // The camas of a drawing that was closed are forgotten; work that was lost goes to the one line of the prune (point 1 of the review:
        // a Say here was overwritten by the prunes that run after it).
        private void PruneCamaDrafts(IWorkspaceBrowserSummaryPort port)
        {
            var dead = _camaDrafts.Where(p => !IsAlive(port, p.Key.Session)).ToList();
            if (dead.Count == 0) return;
            foreach (var pair in dead)
            {
                if (pair.Value.Editor.IsDirty || pair.Value.Editor.IsWriting) LostOnPrune(CamaNameOf(pair.Value));
                if (_inFlight != null && _inFlight.Session == pair.Key.Session) AbandonInFlight();
                ForgetCama(pair.Key, pair.Value);
            }

            RaiseWaiting();
            RaiseDynamic();
        }

        // The state of a cama in the list while its draft is open in the window (the marks of the other drafts).
        private bool CamaDraftState(SessionId session, string rackId, out bool writing, out bool dirty)
        {
            writing = dirty = false;
            if (!_camaDrafts.TryGetValue((session, rackId), out var entry)) return false;
            writing = entry.Editor.IsWriting;
            dirty = entry.Editor.IsDirty;
            return true;
        }

        private void RaiseCama()
        {
            OnPropertyChanged(nameof(IsCamaRack));
            OnPropertyChanged(nameof(ActiveCamaEditor));
            OnPropertyChanged(nameof(HasCamaEditor));
            OnPropertyChanged(nameof(CamaEditors));
            OnPropertyChanged(nameof(ShowCamaOffer));
        }

        // ------------------------------------------------------------------ a new cama of «Nuevo» (a draft, SD-06)

        /// <summary>The new cama that «Nuevo» shows: the editor of the active draft of the drawing when it is a cama; null otherwise.</summary>
        public CamaEditorViewModel NewCamaEditor => ActiveDraftEditor as CamaEditorViewModel;

        public bool HasNewCamaEditor => NewCamaEditor != null;

        public bool ShowNewCamaEditor => IsNuevoPage && HasNewCamaEditor;

        /// <summary>A new cama can be made here: the window has the door of the cama and a drawing.</summary>
        public bool CanStartNewCama => _camaDoor != null && Panel.PresentedSession.HasValue;

        /// <summary>«Cama en esta ventana» of «Nuevo»: a new draft with the defaults of the classic editor, through the one entry point.</summary>
        public void StartNewCama() => OpenAsNewDraft(RackCadDraftSeed.Defaults(RackSystemKind.Cama));

        private void RaiseNewCama()
        {
            OnPropertyChanged(nameof(NewCamaEditor));
            OnPropertyChanged(nameof(HasNewCamaEditor));
            OnPropertyChanged(nameof(ShowNewCamaEditor));
            OnPropertyChanged(nameof(CanStartNewCama));
            RaiseCama(); // a reading of the list can tell the kind of the active rack: its offer follows
        }

        // The editor of a new draft of a cama (CreateDraftEditor of the drafts): the defaults of the classic editor, a design of the library, or
        // a copy of another draft (Duplicar). Null, with the reason, when the design cannot be opened.
        private CamaEditorViewModel CreateCamaDraft(RackCadDraftSeed seed, SessionId session, string rackId, string label, out string failure)
        {
            failure = null;
            CamaDraftEditor draft;
            if (seed.Project == null)
            {
                draft = CamaDraftEditor.CreateNew(_catalog());
            }
            else
            {
                var bed = CamaDraftRules.Copy(seed.Project.FlowBed);
                if (bed == null || !RackDesignValidation.IsUsableFlowBed(bed))
                {
                    failure = "El diseño no tiene una cama que abrir.";
                    return null;
                }

                draft = new CamaDraftEditor(_catalog(), bed, seed.Name, seed.Project.SourceFlowBedDocument);
            }

            var editor = new CamaEditorViewModel(draft, rackId, UpdateCama, isNewRack: true);
            if (seed.Edit != null && !editor.TakeDesignAsEdit(CamaDraftRules.Copy(seed.Edit.FlowBed)))
            {
                failure = "No se pudo copiar el diseño.";
                return null;
            }

            WireCama(editor);
            _camaDrafts[(session, rackId)] = new CamaDraftEntry(session, editor);
            if (seed.Project != null) editor.Say(OpenedText(label, seed, null, null), error: false);
            return editor;
        }

        // A draft of a cama is live while its editor is in the dictionary of the camas, as a new cama.
        private bool IsLiveCama(DraftEntry entry, CamaEditorViewModel cama)
            => cama.IsNewRack && _camaDrafts.TryGetValue((entry.Session, entry.RackId), out var live) && ReferenceEquals(live.Editor, cama);

        // For «Duplicar»: the bed of the draft (a copy) and the base of its «Modificado», which the copy keeps.
        private static (RackProject Base, RackProject Copied) CopyOfCama(CamaEditorViewModel cama)
        {
            var copied = cama.Draft.Problems().Count > 0 ? null : cama.Draft.Snapshot();
            var baseline = cama.BaselineBed;
            return (baseline == null ? null : RackProject.ForCama(baseline), copied == null ? null : RackProject.ForCama(copied));
        }

        // ------------------------------------------------------------------ «Guardar en biblioteca…» (UT-04) and the BOM (BM-02)

        // «Más» › «Guardar en biblioteca…» of a cama editor: the page of the library with the line of the save.
        private void StartSaveToLibrary(CamaEditorViewModel editor)
        {
            Contained(() => BeginLibrarySave(new RackCadLibrarySaveSource(editor, editor.Name, "Cama",
                () => ChangesNote(editor.IsNewRack, editor.IsDirty), name => BuildCamaProject(editor), editor.SaveRefusal)));
        }

        // What is saved for a cama: what the editor shows (the draft included), as «Guardar en biblioteca» of the classic editor saves it,
        // keeping the unknown fields of the design it was opened from (I-11): a project of the library, or the document of the drawing.
        private (RackProject Project, string Error) BuildCamaProject(CamaEditorViewModel editor)
        {
            if (editor.Draft.Problems().Count > 0) return (null, "la cama tiene valores marcados que corregir.");
            var project = RackProject.ForCama(editor.Draft.Snapshot());
            return (_librarySources.TryGetValue(editor, out var source) ? project.WithSourceMetadataFrom(source) : project.WithSourceFlowBed(editor.Draft.Source), null);
        }

        private bool IsCamaEditorAlive(object editor) => _camaDrafts.Values.Any(e => ReferenceEquals(e.Editor, editor));

        // The BOM of the draft of a cama of the drawing with changes: the one of its handler, on a copy; none while a value is marked
        // (CM-S5, point 4 of the review: a value refused in its field is marked, and the BOM does not count with the last valid one).
        private BillOfMaterials CamaDraftBomOf(SessionId session, string rackId)
            => _camaDrafts.TryGetValue((session, rackId), out var cama) && !cama.Editor.IsNewRack && cama.Editor.IsDirty ? cama.Editor.DraftBom() : null;

        // Whether a cama of the drawing has changes not written in the window; null when the rack is not a cama of the window.
        private bool? HasUnwrittenCama(SessionId session, string rackId)
            => _camaDrafts.TryGetValue((session, rackId), out var cama) ? !cama.Editor.IsNewRack && cama.Editor.IsDirty : null;

        private IEnumerable<(string RackId, string Name)> UnwrittenCamaDraftsOf(SessionId session)
            => _camaDrafts
                .Where(p => p.Key.Session == session && !p.Value.Editor.IsNewRack && p.Value.Editor.IsDirty)
                .Select(p => (p.Key.RackId, !string.IsNullOrWhiteSpace(p.Value.Editor.Name) ? p.Value.Editor.Name.Trim() : RackList.NameOf(p.Key.RackId)));
    }
}
