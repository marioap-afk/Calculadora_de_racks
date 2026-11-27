using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.StructuralSections;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.Domain.Systems.Cantilever;
using RackCad.Domain.Systems.Shared;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T5 ct (I-64, fifth batch; prototype «cantilever», accepted by the Owner on 2026-11-26): a Cantilever line edited in the window, with its
    /// numbered door, as Dinámico, Selectivo and Cabecera. «Editar aquí» asks AutoCAD to read the line (RACKCANTILEVERABRIR, no undo marker), the
    /// editor opens on the answer to THAT request, and «Actualizar» hands COPIES to RACKCANTILEVERAPLICAR, which redraws its views in place
    /// with the drawing half of RACKEDITAR (one «U»). «Insertar vista…» is the reusable panel (RACKCANTILEVERINSERTAR), and «Cantilever en esta
    /// ventana» of «Nuevo» is a draft («Borrador N») inserted with «Insertar en el dibujo…» (RACKCANTILEVERNUEVO, the name of I-60). The one
    /// request in flight, «Dejar de esperar», the request the command already took, a draft of another drawing, the prune of a closed drawing
    /// and the veto of the close (D-08) are the rules of the window; the BOM of the draft is the one of the classic editor (BM-02), and «Más» ›
    /// «Guardar en biblioteca…» saves what the editor shows (UT-04).
    /// </summary>
    public sealed partial class RackCadWindowViewModel
    {
        private IRackCadCantileverDoorPort _cantileverDoor;
        private StructuralSectionCatalog _cantileverCatalogue;
        private string _cantileverCatalogueError;

        // The Cantilever lines opened in the window (of the drawing, and the drafts of «Nuevo»), with the key and the life of the other drafts.
        private readonly Dictionary<(SessionId Session, string RackId), CantileverDraftEntry> _cantileverDrafts = new(DraftKeyComparer.Instance);

        /// <summary>
        /// How the window loads the catalogue of structural sections of Cantilever (fail closed, as the classic editor: an invalid catalogue
        /// means no trustworthy dimension, so the window does not offer the editor and says why). Tests give theirs.
        /// </summary>
        internal static Func<StructuralSectionCatalog> CantileverCatalogueSource { get; set; } =
            () => new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private sealed class CantileverDraftEntry
        {
            public CantileverDraftEntry(SessionId session, CantileverEditorViewModel editor, string listedName)
            {
                Session = session;
                Editor = editor;
                ListedName = string.IsNullOrWhiteSpace(listedName) ? null : listedName.Trim();
            }

            public SessionId Session { get; }

            public CantileverEditorViewModel Editor { get; }

            public string ListedName { get; }
        }

        // One line of the constructor: a Cantilever line is edited here only through a door that reads and writes it.
        private void WireCantileverDoor(IRackCadRackDoorPort door)
        {
            _cantileverDoor = door as IRackCadCantileverDoorPort;
            if (_cantileverDoor != null) _cantileverDoor.CantileverRead += OnCantileverRead;
        }

        // The catalogue, loaded once; null with the reason when it cannot be used.
        private StructuralSectionCatalog CantileverCatalogue()
        {
            if (_cantileverCatalogue != null || _cantileverCatalogueError != null) return _cantileverCatalogue;
            try
            {
                _cantileverCatalogue = CantileverCatalogueSource();
                if (_cantileverCatalogue == null) _cantileverCatalogueError = "no hay catálogo de secciones estructurales.";
            }
            catch (Exception ex)
            {
                _cantileverCatalogueError = ex.Message;
            }

            return _cantileverCatalogue;
        }

        private string CatalogueRefusal => "No se pudo cargar el catálogo de secciones estructurales, así que el editor de Cantilever no se abre: "
                                           + (_cantileverCatalogueError ?? "sin detalle.");

        // ------------------------------------------------------------------ a line of the drawing

        /// <summary>Whether the active rack is a Cantilever line, by the Kind of the list of the drawing.</summary>
        public bool IsCantileverRack =>
            string.Equals(RackList.KindOf(Panel.Browser.ActiveRackId), RackEmbedDocument.KindCantilever, StringComparison.OrdinalIgnoreCase);

        /// <summary>The editor of the active Cantilever line in the window, when it was opened here; null otherwise.</summary>
        public CantileverEditorViewModel ActiveCantileverEditor =>
            Panel.PresentedSession.HasValue && Panel.Browser.ActiveRackId != null
            && _cantileverDrafts.TryGetValue((Panel.PresentedSession.Value, Panel.Browser.ActiveRackId), out var entry)
            && !entry.Editor.IsNewRack
                ? entry.Editor
                : null;

        public bool HasCantileverEditor => ActiveCantileverEditor != null;

        /// <summary>Every Cantilever editor that the window keeps open, of any drawing.</summary>
        public IReadOnlyList<CantileverEditorViewModel> CantileverEditors => _cantileverDrafts.Values.Select(e => e.Editor).ToList();

        /// <summary>«Editar aquí» of a Cantilever line (dolor 19): a line with no editor open yet, and a door that reads and writes it.</summary>
        public bool ShowCantileverOffer => IsCantileverRack && ActiveCantileverEditor == null && _cantileverDoor != null;

        /// <summary>«Editar aquí» of a Cantilever line: asks AutoCAD to read it (RACKCANTILEVERABRIR); the one request of the door is shared.</summary>
        public void EditCantileverHere()
        {
            var rack = Panel.Browser.ActiveRackId;
            var session = Panel.PresentedSession;
            if (_cantileverDoor == null || rack == null || !session.HasValue) return;
            if (CantileverCatalogue() == null)
            {
                Say(CatalogueRefusal, failed: true);
                return;
            }

            if (_inFlight != null)
            {
                Say(DoorBusyText, failed: true);
                return;
            }

            // The request waits BEFORE it is handed over: AutoCAD may run the command, and answer, during the call (M-22).
            var id = ++_lastRequestId;
            _inFlight = new DoorRequest(id, false, session.Value, rack, null, null, IsCantilever: true);
            Say("Leyendo la línea en AutoCAD…", failed: false);
            RaiseWaiting();

            RackCadClassicResult result;
            try
            {
                result = _cantileverDoor.RequestCantileverDesign(new RackDoorRequest(id, session.Value, rack))
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

        // The answer of AutoCAD to «Editar aquí» of a Cantilever line: only the answer to the Cantilever read that waits.
        private void OnCantileverRead(CantileverDesignRead read)
        {
            Contained(() =>
            {
                var request = _inFlight;
                if (read == null || request == null || request.IsWrite || !request.IsCantilever || read.RequestId != request.Id) return;
                _inFlight = null;
                RaiseWaiting();

                if (!read.Opens)
                {
                    Say("No se pudo abrir la línea: " + (read.Reason ?? "sin detalle."), failed: true);
                    return;
                }

                if (!string.Equals(read.RackId, request.RackId, StringComparison.OrdinalIgnoreCase))
                {
                    Say("AutoCAD leyó otro rack que el pedido: no se abrió nada.", failed: true);
                    return;
                }

                var design = read.Project()?.CantileverLineDesign;
                var catalogue = CantileverCatalogue();
                if (design == null || catalogue == null)
                {
                    Say(catalogue == null ? CatalogueRefusal : "La línea no se puede abrir en este editor; usa RACKEDITAR.", failed: true);
                    return;
                }

                var key = (request.Session, request.RackId);
                if (_cantileverDrafts.TryGetValue(key, out var existing) && (existing.Editor.IsDirty || existing.Editor.IsWriting))
                {
                    Say("Esta línea ya tiene un borrador con cambios en la ventana: no se reemplazó.", failed: false);
                    RaiseDynamic();
                    return;
                }

                if (existing != null) ForgetCantilever(key, existing);

                // Correction 4 of the review: the rule of RACKEDITAR (LoadExisting(…, embed.Name, …)): the editor shows and writes the name of
                // the envelope; the inner name of the design is not touched (a design with no name is not given one).
                var draft = new CantileverDraftEditor(catalogue);
                draft.Load(design, read.Name);
                var editor = new CantileverEditorViewModel(draft, request.RackId, UpdateCantilever);
                editor.LoadProperties(read.Properties); // the tab «Propiedades» (VA-07)
                WireCantilever(editor);
                _cantileverDrafts[key] = new CantileverDraftEntry(request.Session, editor, RackList.ListedNameOf(request.RackId));
                _showPanel = false;
                Say(string.Empty, failed: false);
                RaiseDynamic();
            });
        }

        // ------------------------------------------------------------------ a new line of «Nuevo» (a draft, SD-06)

        /// <summary>The new line that «Nuevo» shows: the editor of the active draft of the drawing when it is a Cantilever; null otherwise.</summary>
        public CantileverEditorViewModel NewCantileverEditor => ActiveDraftEditor as CantileverEditorViewModel;

        public bool HasNewCantileverEditor => NewCantileverEditor != null;

        public bool ShowNewCantileverEditor => IsNuevoPage && HasNewCantileverEditor;

        /// <summary>A new line can be made here: the window has the door of Cantilever and a drawing.</summary>
        public bool CanStartNewCantilever => _cantileverDoor != null && Panel.PresentedSession.HasValue;

        /// <summary>
        /// «Cantilever en esta ventana» of «Nuevo»: a new line with the defaults of the classic editor (no section and no cut: it does not
        /// resolve until «Lo esencial» is filled, CT-05), as a «Borrador N»; «Insertar en el dibujo…» places it with the name of I-60.
        /// </summary>
        public void StartNewCantilever() => OpenAsNewDraft(RackCadDraftSeed.Defaults(RackSystemKind.Cantilever));

        // The editor of a new draft of Cantilever (the defaults, a design of the library or a copy), kept as a new line of the window.
        private CantileverEditorViewModel CreateCantileverDraft(RackCadDraftSeed seed, SessionId session, string rackId, string label, out string failure)
        {
            failure = null;
            var catalogue = CantileverCatalogue();
            if (catalogue == null)
            {
                failure = CatalogueRefusal;
                return null;
            }

            var draft = new CantileverDraftEditor(catalogue);
            if (seed.Project != null)
            {
                var design = seed.Project.CantileverLineDesign;
                if (design == null)
                {
                    failure = "El diseño no tiene una línea Cantilever que abrir.";
                    return null;
                }

                draft.Load(design, seed.Name); // the name of the entry, as LoadDesignForNew(…, entry.Name, …); the inner one is not touched
            }

            var editor = new CantileverEditorViewModel(draft, rackId, UpdateCantilever, isNewRack: true);
            if (seed.Edit != null && !editor.TakeDesignAsEdit(seed.Edit.CantileverLineDesign))
            {
                failure = "No se pudo copiar el diseño.";
                return null;
            }

            WireCantilever(editor);
            _cantileverDrafts[(session, rackId)] = new CantileverDraftEntry(session, editor, null);
            if (seed.Project != null)
                editor.Say(label + ": Cantilever nuevo en esta ventana" + (seed.Origin == null ? ", con el diseño abierto" : ", " + seed.Origin) + ". Todavía no está en el dibujo.", error: false);
            return editor;
        }

        // ------------------------------------------------------------------ the wiring and the writes

        private void WireCantilever(CantileverEditorViewModel editor)
        {
            editor.InsertViewsAction = InsertCantileverViews;
            editor.InsertNewAction = InsertNewCantilever;
            editor.SaveToLibraryAction = StartSaveToLibrary;
            editor.ExistingViewKinds = () => RackList.ViewKindsOf(editor.RackId);
            editor.OtherRequestText = OtherRequestText;
            editor.OtherDrawingText = () => OtherDrawingText(_cantileverDrafts.Values.FirstOrDefault(e => ReferenceEquals(e.Editor, editor))?.Session);
            editor.PropertyChanged += OnCantileverEditorChanged;
            editor.CloseRequested += CloseCantileverEditor;
        }

        private void OnCantileverEditorChanged(object sender, PropertyChangedEventArgs e)
        {
            RefreshDraftOf(sender, e.PropertyName); // the item of a draft follows its name and its state
            if (e.PropertyName != nameof(CantileverEditorViewModel.IsDirty) && e.PropertyName != nameof(CantileverEditorViewModel.IsWriting)) return;
            RackList.RefreshStates();
            OnPropertyChanged(nameof(HeaderText));
        }

        private void UpdateCantilever(CantileverEditorViewModel editor) => WriteCantilever(editor, null);

        private void InsertCantileverViews(CantileverEditorViewModel editor, IReadOnlyList<RackViewAddress> views)
        {
            if (views == null || views.Count == 0 || editor.IsNewRack) return;
            WriteCantilever(editor, views);
        }

        private void InsertNewCantilever(CantileverEditorViewModel editor, IReadOnlyList<RackViewAddress> views)
        {
            if (views == null || views.Count == 0 || !editor.IsNewRack) return;
            WriteCantilever(editor, views);
        }

        // «Actualizar», «Insertar vista…» and «Insertar en el dibujo…»: COPIES of the draft go to AutoCAD; the editor is locked BEFORE the
        // request is handed over (M-22), until the answer arrives or the window stops waiting.
        private void WriteCantilever(CantileverEditorViewModel editor, IReadOnlyList<RackViewAddress> insertViews)
        {
            if (_cantileverDoor == null)
            {
                editor.Say("Aquí no se puede escribir el dibujo.", error: true);
                return;
            }

            var entry = _cantileverDrafts.Values.FirstOrDefault(e => ReferenceEquals(e.Editor, editor));
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
                editor.Say("No hay una línea resuelta que escribir.", error: true);
                return;
            }

            var id = ++_lastRequestId;
            _inFlight = new DoorRequest(id, true, entry.Session, editor.RackId, snapshot.DesignJson, snapshot.Name, IsCantilever: true);
            editor.BeginWriting();
            RaiseWaiting();

            RackCadClassicResult result;
            try
            {
                result = _cantileverDoor.RequestCantileverUpdate(new CantileverDoorWrite(id, entry.Session, editor.RackId, snapshot.Name, snapshot.Design,
                             snapshot.Line, snapshot.DesignJson, insertViews == null ? null : new List<RackViewAddress>(insertViews), editor.IsNewRack,
                             insertViews == null || editor.IsNewRack ? editor.PropertiesWrite() : null)) // T6 pr: and a new line's (VA-S9)
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

        // The answer to a write of a Cantilever line: written = «Limpio» on what was sent; a new line that AutoCAD placed is a line of the drawing.
        private void CantileverWritten(DoorRequest request, RackDesignWritten written)
        {
            if (!_cantileverDrafts.TryGetValue((request.Session, request.RackId), out var entry)) return;
            var editor = entry.Editor;
            editor.WriteHandedOver();
            editor.EndWriting();
            if (written.Written) editor.MarkWritten(written.Message, request.SentJson);
            else
            {
                editor.Say(written.Message, error: true);
                EchoHiddenDraft(editor, written.Message, failed: true);
            }

            editor.PropertiesWritten(written);
            if (written.Written && editor.IsNewRack)
            {
                editor.MarkInserted(written.Name);
                Say(written.Message, failed: false);
                RaiseDynamic();
            }
        }

        // «Dejar de esperar» of a write of a Cantilever line: the editor can be edited again, with its draft still not written.
        private void StopCantileverWrite(DoorRequest request, string text)
        {
            if (!_cantileverDrafts.TryGetValue((request.Session, request.RackId), out var entry)) return;
            entry.Editor.EndWriting();
            entry.Editor.Say(text, error: false);
            EchoHiddenDraft(entry.Editor, text, failed: false);
        }

        private void CantileverWriteTaken(DoorRequest request)
        {
            if (_cantileverDrafts.TryGetValue((request.Session, request.RackId), out var entry)) entry.Editor.WriteTaken();
        }

        private void CloseCantileverEditor(CantileverEditorViewModel editor)
        {
            Contained(() =>
            {
                foreach (var pair in _cantileverDrafts.Where(p => ReferenceEquals(p.Value.Editor, editor)).ToList())
                {
                    if (pair.Value.Editor.IsWriting) return;
                    ForgetCantilever(pair.Key, pair.Value);
                }

                RaiseDynamic();
                OnPropertyChanged(nameof(HeaderText));
            });
        }

        private void ForgetCantilever((SessionId Session, string RackId) key, CantileverDraftEntry entry)
        {
            entry.Editor.PropertyChanged -= OnCantileverEditorChanged;
            entry.Editor.CloseRequested -= CloseCantileverEditor;
            _cantileverDrafts.Remove(key);
            ForgetUtilitiesOf(entry.Editor);
        }

        // The name of a Cantilever line in the notices: a draft of «Nuevo» by its own name, then the name of the line, of the list, its id.
        private string CantileverNameOf(CantileverDraftEntry entry) =>
            DraftNoticeName(entry.Editor) is string draft ? draft
            : !string.IsNullOrWhiteSpace(entry.Editor.Name) ? entry.Editor.Name.Trim()
            : entry.ListedName ?? entry.Editor.RackId;

        // D-08: the lines of the session with changes not written (or being written).
        private IEnumerable<string> UnwrittenCantileverNames(SessionId session)
            => _cantileverDrafts.Where(p => p.Key.Session == session && (p.Value.Editor.IsDirty || p.Value.Editor.IsWriting)).Select(p => CantileverNameOf(p.Value));

        // The lines of a drawing that was closed are forgotten, and work that was lost is said.
        private void PruneCantileverDrafts(IWorkspaceBrowserSummaryPort port)
        {
            var dead = _cantileverDrafts.Where(p => !IsAlive(port, p.Key.Session)).ToList();
            if (dead.Count == 0) return;
            foreach (var pair in dead)
            {
                // T5 integration (cm's prune): the name goes to LostOnPrune, said once with what the other prunes lost.
                if (pair.Value.Editor.IsDirty || pair.Value.Editor.IsWriting) LostOnPrune(CantileverNameOf(pair.Value));
                if (_inFlight != null && _inFlight.Session == pair.Key.Session) AbandonInFlight();
                ForgetCantilever(pair.Key, pair.Value);
            }

            RaiseWaiting();
            RaiseDynamic();
        }

        // The state of a line in the list while its draft is open in the window (the marks of the other drafts).
        private bool CantileverDraftState(SessionId session, string rackId, out bool writing, out bool dirty)
        {
            writing = dirty = false;
            if (!_cantileverDrafts.TryGetValue((session, rackId), out var entry)) return false;
            writing = entry.Editor.IsWriting;
            dirty = entry.Editor.IsDirty;
            return true;
        }

        private void RaiseCantilever()
        {
            OnPropertyChanged(nameof(IsCantileverRack));
            OnPropertyChanged(nameof(ActiveCantileverEditor));
            OnPropertyChanged(nameof(HasCantileverEditor));
            OnPropertyChanged(nameof(CantileverEditors));
            OnPropertyChanged(nameof(ShowCantileverOffer));
        }

        private void RaiseNewCantilever()
        {
            OnPropertyChanged(nameof(NewCantileverEditor));
            OnPropertyChanged(nameof(HasNewCantileverEditor));
            OnPropertyChanged(nameof(ShowNewCantileverEditor));
            OnPropertyChanged(nameof(CanStartNewCantilever));
        }

        private void RefreshCantileverInsertPanels()
        {
            foreach (var entry in _cantileverDrafts.Values) entry.Editor.RefreshInsertState();
        }

        // ------------------------------------------------------------------ the drafts of «Nuevo» (the per-system cases of the Drafts file)

        private bool IsLiveCantilever(SessionId session, string rackId, CantileverEditorViewModel editor) =>
            editor.IsNewRack && _cantileverDrafts.TryGetValue((session, rackId), out var entry) && ReferenceEquals(entry.Editor, editor);

        // For «Duplicar»: the last valid design as a project (a copy), and the base of its «Modificado», which the copy keeps.
        private static (RackProject Base, RackProject Copied) CopyOfCantilever(CantileverEditorViewModel editor)
        {
            // Correction 5 of the review: the copy has an identity of its own (DuplicateWithNewIdentity), the base and the copy the same one,
            // so the copy is «Modificado» exactly when its original was.
            var snapshot = editor.Draft.SnapshotForWrite();
            var copied = (snapshot != null ? snapshot.Design : editor.Draft.Design).DuplicateWithNewIdentity();
            copied.Name = null;
            var baseDesign = editor.Draft.BaselineDesign;
            baseDesign.Id = copied.Id;
            baseDesign.Name = null; // the copy has no name (ND-07), and neither has its base
            return (RackProject.ForCantilever(baseDesign), RackProject.ForCantilever(copied));
        }

        // ------------------------------------------------------------------ the BOM of the draft (BM-02, SD-02) and the library (UT-04)

        private BillOfMaterials CantileverDraftBom(SessionId session, string rackId)
            => _cantileverDrafts.TryGetValue((session, rackId), out var entry) && !entry.Editor.IsNewRack && entry.Editor.IsDirty ? entry.Editor.Draft.BomCopy() : null;

        private bool HasUnwrittenCantilever(SessionId session, string rackId)
            => _cantileverDrafts.TryGetValue((session, rackId), out var entry) && !entry.Editor.IsNewRack && entry.Editor.IsDirty;

        private IEnumerable<(string RackId, string Name)> UnwrittenCantileverDrafts(SessionId session)
            => _cantileverDrafts.Where(p => p.Key.Session == session && !p.Value.Editor.IsNewRack && p.Value.Editor.IsDirty)
                .Select(p => (p.Key.RackId, CantileverNameOf(p.Value)));

        private bool IsCantileverEditorAlive(object editor) => _cantileverDrafts.Values.Any(e => ReferenceEquals(e.Editor, editor));

        // «Más» › «Guardar en biblioteca…» of a Cantilever editor: the page of the library with the line of the save.
        private void StartSaveToLibrary(CantileverEditorViewModel editor)
        {
            Contained(() => BeginLibrarySave(new RackCadLibrarySaveSource(editor, editor.Name, "Cantilever",
                () => ChangesNote(editor.IsNewRack, editor.IsDirty), name => BuildCantileverProject(editor, name), editor.SaveRefusal)));
        }

        // What is saved for a Cantilever line: the design of the draft (the last resolved one), with the name typed, as «Guardar en
        // biblioteca» of the classic editor saves it (RackProject.ForCantilever, with the metadata of its source, I-11).
        private (RackProject Project, string Error) BuildCantileverProject(CantileverEditorViewModel editor, string name)
        {
            var snapshot = editor.Draft.SnapshotForWrite();
            if (snapshot == null) return (null, "no hay una línea resuelta que guardar.");
            snapshot.Design.Name = name;
            _librarySources.TryGetValue(editor, out var source);
            return (RackProject.ForCantilever(snapshot.Design).WithSourceMetadataFrom(source), null);
        }
    }
}
