using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.Domain.Systems.Shared;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// The drafts of «Nuevo» (I-64 A-14, line D, cuarta tanda, part «new»; SD-06 of the Owner: «En lugar de nuevo, podríamos manejar
    /// borradores, tipo, borrador 1, borrador 2 etc, para poder tener varios nuevos, mejora la propuesta»). Until this part «Nuevo» held
    /// ONE new rack of the drawing and refused the second (P3-04). Now every «… en esta ventana», every design opened as a new rack and
    /// every «Duplicar» is a draft of its own: «Borrador 1», «Borrador 2»…, numbered by drawing, of any system that has an editor in the
    /// window (Selectivo and Dinámico today).
    ///
    /// A draft IS a new rack of the window as P3 and the step 4 of Dinámico made it: its editor lives in the dictionaries of the editors
    /// with <c>IsNewRack</c>, so the door, the one request in flight, «Insertar en el dibujo…» (RACKSELECTIVONUEVO, RACKDINAMICONUEVO, the
    /// automatic name of I-60), «Dejar de esperar», «Nuevo sigue al dibujo» (a draft of another drawing is not written in the active one)
    /// and the prune of a closed drawing do not change. This file only adds what a list of drafts needs: their numbers, which one «Nuevo»
    /// shows, the items of the side column and of the list of «Nuevo» with their state, «Duplicar», «Descartar el borrador», and their names
    /// in the veto of the close (D-08), which now names every draft with work by its own name.
    ///
    /// <para><b>The one entry point</b> to open a design as a new draft is <see cref="OpenAsNewDraft"/>. «Selectivo en esta ventana» and
    /// «Dinámico en esta ventana» call it with <see cref="RackCadDraftSeed.Defaults"/>; «Duplicar» with a copy; Utilidades («Abrir de la
    /// biblioteca») calls it with <see cref="RackCadDraftSeed.FromProject"/> over the project the library loaded. To add a system (Cabecera,
    /// «Nuevo › Cabecera en esta ventana»), give its case to the six per-system methods of this file: <see cref="DraftDoorOf"/> (it has an
    /// editor, and the door that writes it), <see cref="CreateDraftEditor"/> (its editor, kept in its own dictionary with its
    /// <c>IsNewRack</c>, as the two here), <see cref="IsLive"/>, <see cref="StateOf"/>, <see cref="CopyOf"/> and
    /// <see cref="ForgetDraftEditor"/>; everything else (numbers, column, list, limit, notices) follows.</para>
    /// </summary>
    public sealed partial class RackCadWindowViewModel
    {
        /// <summary>What the list of «Nuevo» says over its drafts.</summary>
        internal static readonly string DraftsIntro =
            "Cada «… en esta ventana» empieza un borrador nuevo. Se abren, se insertan y se descartan por separado; al insertarlo, si no le "
            + "pusiste nombre, toma el automático. Hasta " + RackCadDraftNaming.MaxPerDrawing + " por dibujo.";

        // One draft of the window: the drawing it belongs to, its editor (one of the dictionaries of the editors) and its item.
        private sealed class DraftEntry
        {
            public DraftEntry(SessionId session, object editor, RackCadDraftItem item)
            {
                Session = session;
                Editor = editor;
                Item = item;
            }

            public SessionId Session { get; }

            public object Editor { get; }

            public RackCadDraftItem Item { get; }

            public string RackId => Item.RackId;
        }

        private readonly List<DraftEntry> _draftEntries = new List<DraftEntry>();

        // The draft whose editor «Nuevo» shows, per drawing: coming back to a drawing shows the draft it had open.
        private readonly Dictionary<SessionId, DraftEntry> _activeDraftOf = new Dictionary<SessionId, DraftEntry>();

        private IReadOnlyList<RackCadDraftItem> _drafts = Array.Empty<RackCadDraftItem>();

        // ------------------------------------------------------------------ what the window shows

        /// <summary>The drafts of the drawing of the window, by number: under «Nuevo» in the side column and in the list of «Nuevo».</summary>
        public IReadOnlyList<RackCadDraftItem> Drafts => _drafts;

        public bool HasDrafts => _drafts.Count > 0;

        /// <summary>The draft whose editor «Nuevo» shows; null on the home of «Nuevo» (its systems and the list of drafts).</summary>
        public RackCadDraftItem ActiveDraft => ActiveEntry?.Item;

        public bool HasActiveDraft => ActiveEntry != null;

        /// <summary>The bar of the drafts over the page «Nuevo»: the list on its home, or the strip of the draft that is open.</summary>
        public bool ShowDraftsBar => IsNuevoPage && HasDrafts;

        public bool ShowDraftList => IsNuevoPage && HasDrafts && !HasActiveDraft;

        public bool ShowActiveDraftStrip => IsNuevoPage && HasActiveDraft;

        public string DraftsIntroText => DraftsIntro;

        // The editor that «Nuevo» shows: the one of the active draft of the drawing of the window (a Selectivo or a Dinámico).
        private object ActiveDraftEditor => ActiveEntry?.Editor;

        private DraftEntry ActiveEntry =>
            Panel.PresentedSession.HasValue && _activeDraftOf.TryGetValue(Panel.PresentedSession.Value, out var entry) && IsLive(entry) ? entry : null;

        // ------------------------------------------------------------------ the entry point

        /// <summary>
        /// THE entry point to open a design as a new draft of «Nuevo» (T4 new): a new rack of the window, in the drawing of the window,
        /// not inserted yet. <see cref="RackCadDraftSeed.Defaults"/> gives the defaults of the classic editor of the system;
        /// <see cref="RackCadDraftSeed.FromProject"/> opens a design the library loaded (its name, or none: then it takes the automatic one
        /// of I-60 at insertion). The draft gets the next «Borrador N» of its drawing, and «Nuevo» shows its editor; «Insertar en el
        /// dibujo…» places it. Nothing is read from or written to the drawing.
        ///
        /// It refuses, says why in the message of the window and returns the reason, when: there is no seed; the system has no editor in
        /// the window yet (<see cref="RackCadDraftNaming.NoEditorText"/>); the window has no door that writes that system; there is no
        /// drawing; the drawing already holds <see cref="RackCadDraftNaming.MaxPerDrawing"/> drafts; or the design cannot be opened.
        /// </summary>
        public RackCadDraftOpening OpenAsNewDraft(RackCadDraftSeed seed)
        {
            RackCadDraftOpening opening = null;
            Contained(() => opening = OpenDraftCore(seed));
            return opening ?? RackCadDraftOpening.Refused(_actionMessage);
        }

        private RackCadDraftOpening OpenDraftCore(RackCadDraftSeed seed)
        {
            if (seed == null) return RefuseDraft("No hay un diseño que abrir como borrador.");
            if (!DraftDoorOf(seed.Kind, out var hasDoor)) return RefuseDraft(RackCadDraftNaming.NoEditorText(seed.Kind));
            if (!hasDoor) return RefuseDraft("Aquí no se puede crear un rack en el dibujo.");
            if (!Panel.PresentedSession.HasValue) return RefuseDraft("No hay un dibujo en la ventana.");

            var session = Panel.PresentedSession.Value;
            SyncDrafts();
            var used = _draftEntries.Where(e => e.Session == session).Select(e => e.Item.Number).ToList();
            if (used.Count >= RackCadDraftNaming.MaxPerDrawing)
            {
                ShowPage(RackCadWindowPage.Nuevo); // the list, to insert or discard one
                return RefuseDraft(RackCadDraftNaming.LimitText);
            }

            var number = RackCadDraftNaming.NextNumber(used);
            var rackId = Guid.NewGuid().ToString();
            var editor = CreateDraftEditor(seed, session, rackId, RackCadDraftNaming.Label(number), out var failure);
            if (editor == null) return RefuseDraft(failure);

            var entry = new DraftEntry(session, editor, new RackCadDraftItem(number, seed.Kind, rackId));
            _draftEntries.Add(entry);
            RefreshEntry(entry);
            ShowPage(RackCadWindowPage.Nuevo);
            Say(string.Empty, failed: false);
            SetActiveDraft(entry);
            RaiseDynamic();
            return new RackCadDraftOpening(true, null, entry.Item);
        }

        private RackCadDraftOpening RefuseDraft(string reason)
        {
            Say(reason, failed: true);
            RaiseDynamic();
            return RackCadDraftOpening.Refused(reason);
        }

        // ------------------------------------------------------------------ the per-system cases (a new system adds its own)

        // Whether the system has an editor in the window (true), and whether the window has the door that writes it. Cabecera adds its case.
        private bool DraftDoorOf(RackSystemKind kind, out bool hasDoor)
        {
            switch (kind)
            {
                case RackSystemKind.SelectiveRack:
                    hasDoor = _selectiveDoor != null;
                    return true;
                case RackSystemKind.PalletFlow:
                    hasDoor = _door != null;
                    return true;
                default:
                    hasDoor = false;
                    return false;
            }
        }

        // The editor of a new draft, kept in the dictionary of its system as a new rack of the window (as P3 and the step 4 of Dinámico
        // made them), wired as every editor of its system. Null, with the reason, when the design cannot be opened.
        private object CreateDraftEditor(RackCadDraftSeed seed, SessionId session, string rackId, string label, out string failure)
        {
            switch (seed.Kind)
            {
                case RackSystemKind.SelectiveRack: return CreateSelectiveDraft(seed, session, rackId, label, out failure);
                case RackSystemKind.PalletFlow: return CreateDynamicDraft(seed, session, rackId, label, out failure);
                default:
                    failure = RackCadDraftNaming.NoEditorText(seed.Kind);
                    return null;
            }
        }

        private SelectiveEditorViewModel CreateSelectiveDraft(RackCadDraftSeed seed, SessionId session, string rackId, string label, out string failure)
        {
            failure = null;
            SelectiveDraftEditor draft;
            string linkedNote = null;
            if (seed.Project == null)
            {
                draft = SelectiveDraftEditor.CreateNew(_catalog());
            }
            else
            {
                var document = seed.Project.SelectiveRack;
                var design = document?.ToDomain();
                if (design == null)
                {
                    failure = "El diseño no tiene un Selectivo que abrir.";
                    return null;
                }

                draft = new SelectiveDraftEditor(_catalog());
                if (!draft.LoadFrom(design, null, seed.Name, null))
                {
                    failure = "No se pudo abrir el diseño como borrador: " + (string.IsNullOrWhiteSpace(draft.LastError) ? "sin detalle." : draft.LastError);
                    return null;
                }

                // A new rack of the window carries literal holguras (decision 12; RACKSELECTIVONUEVO refuses a linked one, A2-01). The
                // classic editor opened from the library loads the same literals (the overload without the register).
                if (HasLinkedHolgura(document)) linkedNote = LinkedHolgurasText;
            }

            var editor = SelectiveEditorViewModel.ForNewRack(draft, rackId, UpdateSelective, DimensionStylesOf(session));
            if (seed.Edit != null && !editor.TakeDesignAsEdit(seed.Edit.SelectiveRack?.ToDomain()))
            {
                failure = "No se pudo copiar el diseño: " + (string.IsNullOrWhiteSpace(draft.LastError) ? "sin detalle." : draft.LastError);
                return null;
            }

            WireSelective(editor);
            _selectiveDrafts[(session, rackId)] = new SelectiveDraftEntry(session, editor);
            if (seed.Project != null) editor.Say(OpenedText(label, seed, draft.LoadNotice, linkedNote), error: false);
            return editor;
        }

        private DynamicEditorViewModel CreateDynamicDraft(RackCadDraftSeed seed, SessionId session, string rackId, string label, out string failure)
        {
            failure = null;
            DynamicDraftEditor draft;
            if (seed.Project == null)
            {
                draft = DynamicDraftEditor.CreateNew(_catalog());
            }
            else
            {
                var design = CopyOfDynamicDesign(seed.Project.DynamicDesign);
                if (design == null)
                {
                    failure = "El diseño no tiene un Dinámico que abrir.";
                    return null;
                }

                draft = new DynamicDraftEditor(_catalog());
                draft.LoadFrom(design, seed.Name);
            }

            var editor = new DynamicEditorViewModel(draft, rackId, UpdateDynamic, isNewRack: true);
            if (seed.Edit != null && !editor.TakeDesignAsEdit(CopyOfDynamicDesign(seed.Edit.DynamicDesign)))
            {
                failure = "No se pudo copiar el diseño.";
                return null;
            }

            Wire(editor);
            _dynamicDrafts[(session, rackId)] = new DynamicDraftEntry(session, editor);
            if (seed.Project != null) editor.Say(OpenedText(label, seed, draft.LoadNotice, null), error: false);
            return editor;
        }

        /// <summary>What a Selectivo of the library with holguras bound to variables says when it opens as a draft (point 4 of the review).</summary>
        internal const string LinkedHolgurasText =
            "Las holguras vinculadas a variables se abren con el valor literal guardado en el diseño, no con el de la variable: un rack nuevo de la ventana no lleva vínculos.";

        // Whether a holgura of the design carries a binding entry (presence, as the document answers it): the window never names the map
        // of the links itself (ProjectVariablesConformanceTests: nobody outside Application touches it).
        private static bool HasLinkedHolgura(SelectivePalletDesignDocument document)
            => document.HasBindingEntry(ProjectPropertyIds.SelectiveVerticalClearance) || document.HasBindingEntry(ProjectPropertyIds.SelectivePalletTolerance);

        // What the editor of a draft opened from a design says first: what it is, where it comes from and what the load had to change.
        private static string OpenedText(string label, RackCadDraftSeed seed, string loadNotice, string linkedNote)
        {
            var text = label + ": " + RackCadDraftNaming.SystemLabel(seed.Kind) + " nuevo en esta ventana"
                       + (seed.Origin == null ? ", con el diseño abierto" : ", " + seed.Origin) + ". Todavía no está en el dibujo.";
            if (!string.IsNullOrWhiteSpace(loadNotice)) text += " " + loadNotice.Trim();
            if (!string.IsNullOrWhiteSpace(linkedNote)) text += " " + linkedNote;
            return text;
        }

        // The editor of Dinámico keeps the design it loads as its own (DynamicDraftEditor.LoadFrom): a copy through the store, so nothing
        // the draft does reaches the project of the caller.
        private static RackCad.Domain.Systems.Dynamic.DynamicRackDesign CopyOfDynamicDesign(RackCad.Domain.Systems.Dynamic.DynamicRackDesign design)
        {
            if (design == null) return null;
            var store = new RackProjectStore();
            return store.Deserialize(store.Serialize(RackProject.ForDynamic(design)))?.DynamicDesign;
        }

        // ------------------------------------------------------------------ the gestures of a draft

        /// <summary>Shows the editor of a draft in «Nuevo» (a click on it in the side column, or «Abrir» in the list of «Nuevo»).</summary>
        public void OpenDraft(RackCadDraftItem item)
        {
            Contained(() =>
            {
                var entry = LiveEntryOf(item);
                if (entry == null)
                {
                    Say(NotHereText(item, "abrirlo"), failed: true);
                    RaiseDynamic();
                    return;
                }

                ShowPage(RackCadWindowPage.Nuevo);
                SetActiveDraft(entry);
            });
        }

        /// <summary>
        /// «Duplicar»: a new draft of the same system with a copy of the last valid design of this one, with no name (at insertion it takes
        /// the automatic one of I-60, so two racks never share a name by accident) and its own identity. The copy keeps the BASE of
        /// «Modificado» of its original (point 1 of the review): a copy of a draft with changes is «Modificado» too, so its work vetoes
        /// the close, is named by the prune and asks before «Descartar el borrador» or «Cerrar el editor», whatever happens to its original;
        /// its «Descartar» goes back to that base (the defaults, or the design of the library). Written tramos not applied (S-21) are not
        /// part of the design and are not copied. Contained, as every action of the window.
        /// </summary>
        public RackCadDraftOpening DuplicateDraft(RackCadDraftItem item)
        {
            RackCadDraftOpening opening = null;
            Contained(() => opening = DuplicateCore(item));
            return opening ?? RackCadDraftOpening.Refused(_actionMessage);
        }

        private RackCadDraftOpening DuplicateCore(RackCadDraftItem item)
        {
            var entry = LiveEntryOf(item);
            if (entry == null) return RefuseDraft(NotHereText(item, "duplicarlo"));
            if (StateOf(entry).Writing) return RefuseDraft("AutoCAD está escribiendo " + item.Label + ": espera a que termine para duplicarlo.");

            var copy = CopyOf(entry);
            if (copy.Copied == null) return RefuseDraft("No hay un diseño válido de " + item.Label + " que copiar.");
            return OpenAsNewDraft(RackCadDraftSeed.CopyOf(copy.Base, copy.Copied, "copia de " + item.Title));
        }

        /// <summary>
        /// «Descartar el borrador»: forgets the draft and its editor (it is not in the drawing, so nothing is written). A draft with changes
        /// asks first, inline in the list (<see cref="RackCadDraftItem.IsConfirmingDiscard"/>); one that AutoCAD writes cannot be discarded.
        /// </summary>
        public void DiscardDraft(RackCadDraftItem item)
        {
            Contained(() =>
            {
                var entry = LiveEntryOf(item);
                if (entry == null)
                {
                    if (item != null) Say(NotHereText(item, "descartarlo"), failed: true);
                    return;
                }

                var state = StateOf(entry);
                if (state.Writing)
                {
                    Say("AutoCAD está escribiendo " + item.Label + ": espera a que termine, o termina o cancela la orden en AutoCAD.", failed: true);
                    return;
                }

                if (state.Dirty)
                {
                    item.SetConfirmingDiscard(true);
                    return;
                }

                ForgetDraft(entry);
            });
        }

        /// <summary>«Descartar el borrador» of the inline question of a draft with changes.</summary>
        public void ConfirmDiscardDraft(RackCadDraftItem item)
        {
            Contained(() =>
            {
                var entry = LiveEntryOf(item);
                if (entry == null || !item.IsConfirmingDiscard || StateOf(entry).Writing) return;
                item.SetConfirmingDiscard(false);
                ForgetDraft(entry);
            });
        }

        /// <summary>«Cancelar» of the inline question: the draft stays as it is.</summary>
        public void CancelDiscardDraft(RackCadDraftItem item) => item?.SetConfirmingDiscard(false);

        private void ForgetDraft(DraftEntry entry)
        {
            ForgetDraftEditor(entry);
            Say("Se descartó " + entry.Item.Title + " (" + entry.Item.SystemLabel + " nuevo): no estaba en el dibujo.", failed: false);
            RaiseDynamic();
        }

        // ------------------------------------------------------------------ the per-system cases, continued

        // A draft is live while its editor is in the dictionary of its system, as a new rack: once inserted, closed or pruned it is not.
        private bool IsLive(DraftEntry entry)
        {
            switch (entry?.Editor)
            {
                case SelectiveEditorViewModel selective:
                    return selective.IsNewRack && _selectiveDrafts.TryGetValue((entry.Session, entry.RackId), out var s) && ReferenceEquals(s.Editor, selective);
                case DynamicEditorViewModel dynamic:
                    return dynamic.IsNewRack && _dynamicDrafts.TryGetValue((entry.Session, entry.RackId), out var d) && ReferenceEquals(d.Editor, dynamic);
                default:
                    return false;
            }
        }

        private static (string Name, bool Dirty, bool Writing) StateOf(DraftEntry entry)
        {
            switch (entry.Editor)
            {
                case SelectiveEditorViewModel selective: return (selective.Name, selective.IsDirty, selective.IsWriting);
                case DynamicEditorViewModel dynamic: return (dynamic.Name, dynamic.IsDirty, dynamic.IsWriting);
                default: return (null, false, false);
            }
        }

        // For «Duplicar»: the last valid design of a draft as a project (copies: SnapshotForWrite), and the base of its «Modificado» (what
        // its «Descartar» goes back to), which the copy keeps (point 1 of the review). Copied is null when the draft has no valid design;
        // Base is null when it has no base, and the copy then opens over itself.
        private static (RackProject Base, RackProject Copied) CopyOf(DraftEntry entry)
        {
            switch (entry.Editor)
            {
                case SelectiveEditorViewModel selective:
                    var selectiveCopy = selective.Draft.SnapshotForWrite();
                    var selectiveBase = selective.BaselineDesignJson == null ? null : new SelectivePalletDesignStore().Deserialize(selective.BaselineDesignJson);
                    return (selectiveBase == null ? null : RackProject.ForSelectiveRack(selectiveBase),
                        selectiveCopy == null ? null : RackProject.ForSelectiveRack(SelectivePalletDesignDocument.From(selectiveCopy.Design, null, null)));
                case DynamicEditorViewModel dynamic:
                    var dynamicCopy = dynamic.Draft.SnapshotForWrite();
                    var dynamicBase = dynamic.BaselineDesignJson == null ? null : new RackProjectStore().Deserialize(dynamic.BaselineDesignJson);
                    return (dynamicBase?.DynamicDesign == null ? null : dynamicBase, dynamicCopy == null ? null : RackProject.ForDynamic(dynamicCopy.Design));
                default:
                    return (null, null);
            }
        }

        // Forgets the editor as «Cerrar el editor» does (the same methods), so the window forgets it everywhere.
        private void ForgetDraftEditor(DraftEntry entry)
        {
            switch (entry.Editor)
            {
                case SelectiveEditorViewModel selective:
                    CloseSelectiveEditor(selective);
                    break;
                case DynamicEditorViewModel dynamic:
                    CloseDynamicEditor(dynamic);
                    break;
            }
        }

        // ------------------------------------------------------------------ internals

        private DraftEntry LiveEntryOf(RackCadDraftItem item)
        {
            if (item == null || !Panel.PresentedSession.HasValue) return null;
            var entry = _draftEntries.FirstOrDefault(e => ReferenceEquals(e.Item, item));
            return entry != null && entry.Session == Panel.PresentedSession.Value && IsLive(entry) ? entry : null;
        }

        // Why a gesture on an item does nothing (point 5 of the review): its draft is of another drawing, or it is gone.
        private string NotHereText(RackCadDraftItem item, string what)
        {
            var entry = _draftEntries.FirstOrDefault(e => ReferenceEquals(e.Item, item));
            return entry != null && IsLive(entry) && (!Panel.PresentedSession.HasValue || entry.Session != Panel.PresentedSession.Value)
                ? "Ese borrador es de otro dibujo: vuelve a ese dibujo para " + what + "."
                : "Ese borrador ya no está en la ventana.";
        }

        // Point 2 of the review: what AutoCAD (or «Dejar de esperar») answers to a draft that is not on screen (another draft open, another
        // page, another drawing) is said in its editor and ALSO in the message of the window, with its label: «Borrador 1: …».
        private void EchoHiddenDraft(object editor, string message, bool failed)
        {
            if (string.IsNullOrEmpty(message)) return;
            var entry = _draftEntries.FirstOrDefault(e => ReferenceEquals(e.Editor, editor));
            if (entry == null || !IsLive(entry)) return;
            if (IsNuevoPage && ReferenceEquals(ActiveEntry, entry)) return;
            Say(entry.Item.Label + ": " + message, failed);
        }

        private void SetActiveDraft(DraftEntry entry)
        {
            if (!Panel.PresentedSession.HasValue) return;
            var session = Panel.PresentedSession.Value;
            if (entry == null) _activeDraftOf.Remove(session);
            else _activeDraftOf[session] = entry;
            RaiseNewDynamic();
        }

        // «Nuevo» in the side column goes to its home (its systems and the list of drafts), as the approved prototype of Largueros does.
        private void ShowDraftsHome()
        {
            if (Panel.PresentedSession.HasValue && _activeDraftOf.Remove(Panel.PresentedSession.Value)) RaiseNewDynamic();
        }

        // Drafts whose editor is no longer a new rack of the window (inserted, closed, or of a drawing that was closed) leave the list.
        private void SyncDrafts()
        {
            _draftEntries.RemoveAll(entry => !IsLive(entry));
            foreach (var session in _activeDraftOf.Where(pair => !IsLive(pair.Value)).Select(pair => pair.Key).ToList()) _activeDraftOf.Remove(session);
        }

        private static void RefreshEntry(DraftEntry entry)
        {
            var state = StateOf(entry);
            entry.Item.Refresh(state.Name, state.Dirty, state.Writing);
        }

        // An editor of the window changed: when it is a draft, its item follows its name and its state.
        private void RefreshDraftOf(object editor, string propertyName)
        {
            if (!string.IsNullOrEmpty(propertyName) && propertyName != "Name" && propertyName != "IsDirty" && propertyName != "IsWriting") return;
            var entry = _draftEntries.FirstOrDefault(e => ReferenceEquals(e.Editor, editor));
            if (entry != null) RefreshEntry(entry);
        }

        // The name of a draft in the veto of the close and in the notice of a closed drawing; null when the editor is not a draft.
        private string DraftNoticeName(object editor)
        {
            var entry = _draftEntries.FirstOrDefault(e => ReferenceEquals(e.Editor, editor));
            if (entry == null || !IsLive(entry)) return null;
            return RackCadDraftNaming.NoticeName(entry.Item.Number, entry.Item.SystemLabel, StateOf(entry).Name);
        }

        // The header counts the drafts of the drawing too (an observation of the smoke of the entrega 2: with a new rack «Modificado» it
        // said «todo escrito»).
        private string WithDrafts(string racks)
        {
            var count = _drafts.Count;
            if (count == 0) return racks;
            var drafts = count == 1 ? "1 borrador sin insertar" : count + " borradores sin insertar";
            return string.IsNullOrEmpty(racks) ? drafts : racks + " · " + drafts;
        }

        // The drafts changed (or the drawing of the window, or the page): the list, the items and what the page shows follow.
        private void RaiseDrafts()
        {
            SyncDrafts();
            var active = ActiveEntry;
            foreach (var entry in _draftEntries)
            {
                RefreshEntry(entry);
                entry.Item.SetActive(IsNuevoPage && ReferenceEquals(entry, active)); // only while «Nuevo» is shown (point 7 of the review)
            }

            var list = Panel.PresentedSession.HasValue
                ? _draftEntries.Where(e => e.Session == Panel.PresentedSession.Value).OrderBy(e => e.Item.Number).Select(e => e.Item).ToList()
                : new List<RackCadDraftItem>();
            if (!list.SequenceEqual(_drafts))
            {
                _drafts = list;
                OnPropertyChanged(nameof(Drafts));
                OnPropertyChanged(nameof(HasDrafts));
                OnPropertyChanged(nameof(HeaderText));
            }

            OnPropertyChanged(nameof(ActiveDraft));
            OnPropertyChanged(nameof(HasActiveDraft));
            OnPropertyChanged(nameof(ShowDraftsBar));
            OnPropertyChanged(nameof(ShowDraftList));
            OnPropertyChanged(nameof(ShowActiveDraftStrip));
        }
    }
}
