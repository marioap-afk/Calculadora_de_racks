using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems.Shared;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// What a new draft of «Nuevo» starts from (T4 new, SD-06 of the Owner: «En lugar de nuevo, podríamos manejar borradores, tipo,
    /// borrador 1, borrador 2 etc, para poder tener varios nuevos»): the system, and optionally a design to open as that draft (a design of
    /// the library, a copy of another draft). It is the argument of <see cref="RackCadWindowViewModel.OpenAsNewDraft"/>, the one entry
    /// point to open a design as a new draft of the window.
    /// </summary>
    public sealed record RackCadDraftSeed
    {
        private RackCadDraftSeed(RackSystemKind kind, RackProject project, string name, string origin, RackProject edit = null)
        {
            Kind = kind;
            Project = project;
            Name = string.IsNullOrWhiteSpace(name) ? null : name;
            Origin = string.IsNullOrWhiteSpace(origin) ? null : origin.Trim();
            Edit = edit;
        }

        /// <summary>The system of the draft (the canonical kind of the library: SelectiveRack, PalletFlow, …).</summary>
        public RackSystemKind Kind { get; }

        /// <summary>The design to open, as the library loads it; null = the defaults of the classic editor of that system.</summary>
        public RackProject Project { get; }

        /// <summary>The name of the rack the draft starts with; null = none, and at insertion it takes the automatic one of I-60.</summary>
        public string Name { get; }

        /// <summary>Where the design comes from, for the line of the editor when it opens (e.g. «del diseño «Nave A» de la biblioteca»).</summary>
        public string Origin { get; }

        /// <summary>
        /// «Duplicar» only: the design the draft takes as an EDIT over <see cref="Project"/>, which is then the base of «Modificado» and of
        /// «Descartar» of the copy (the base of its original). Null for every other seed.
        /// </summary>
        internal RackProject Edit { get; }

        /// <summary>A new draft with the defaults of the classic editor of the system: «Selectivo en esta ventana», «Dinámico en esta ventana».</summary>
        public static RackCadDraftSeed Defaults(RackSystemKind kind) => new RackCadDraftSeed(kind, null, null, null);

        /// <summary>
        /// Any loaded project as a new draft. Its name is <paramref name="name"/> when given; otherwise the one its own document keeps (a
        /// Selectivo keeps its name; a Dinámico keeps none, its document stores no name), and empty = the automatic one of I-60 at
        /// insertion. <paramref name="origin"/> is said when the draft opens. For a design of the design library use
        /// <see cref="FromLibrary"/>, which names it as the classic editor opened from the library does.
        /// </summary>
        public static RackCadDraftSeed FromProject(RackProject project, string origin = null, string name = null)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));
            return new RackCadDraftSeed(project.Kind, project, name ?? NameOf(project), origin);
        }

        /// <summary>
        /// A design of the design library as a new draft, with the name the classic editor opened from the library gives it
        /// (<c>EditorModules.OpenFromLibrary</c>): a Selectivo keeps the name of its document (<c>RackSelectiveWindow.LoadForNew</c>;
        /// empty = the automatic one of I-60); the other systems take the name of the entry of the library, <paramref name="entryName"/>
        /// = <c>RackDesignLibraryEntry.Name</c> (<c>LoadDesignForNew(…, entry.Name, …)</c>; for a Dinámico it is the name of the file,
        /// because its document keeps none). Utilidades («Abrir de la biblioteca») calls
        /// <c>OpenAsNewDraft(RackCadDraftSeed.FromLibrary(project, entry.Name, "del diseño «" + entry.Name + "» de la biblioteca"))</c>.
        /// </summary>
        public static RackCadDraftSeed FromLibrary(RackProject project, string entryName, string origin = null)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));
            var name = project.Kind == RackSystemKind.SelectiveRack ? NameOf(project) : entryName?.Trim();
            return new RackCadDraftSeed(project.Kind, project, name, origin);
        }

        // The copy of a draft («Duplicar», point 1 of the review): it opens over the BASE of its original and takes the copied design as an
        // edit, so it is «Modificado» exactly when its original was; no name (two racks would get the same name), and where it comes from.
        internal static RackCadDraftSeed CopyOf(RackProject baseProject, RackProject copied, string origin)
            => new RackCadDraftSeed(copied.Kind, baseProject ?? copied, null, origin, baseProject == null ? null : copied);

        private static string NameOf(RackProject project)
        {
            switch (project.Kind)
            {
                case RackSystemKind.SelectiveRack: return project.SelectiveRack?.Name;
                case RackSystemKind.PalletFlow: return project.DynamicSystem?.Name;
                case RackSystemKind.Cantilever: return project.CantileverLineDesign?.Name; // T5 ct
                default: return null;
            }
        }
    }

    /// <summary>What <see cref="RackCadWindowViewModel.OpenAsNewDraft"/> did: the draft that opened, or why none did.</summary>
    public sealed record RackCadDraftOpening(bool Opened, string Reason, RackCadDraftItem Draft)
    {
        public static RackCadDraftOpening Refused(string reason) => new RackCadDraftOpening(false, reason, null);
    }

    /// <summary>
    /// The pure rules of the drafts of «Nuevo» (T4 new): their numbers, their names in the window and in the notices, the limit per
    /// drawing and the systems that have an editor in the window. Each one is in ONE place, to be changed when the Owner answers.
    /// </summary>
    public static class RackCadDraftNaming
    {
        /// <summary>How many drafts a drawing holds at a time (supuesto ND-04): enough to compare variants, few enough to see them all.</summary>
        public const int MaxPerDrawing = 8;

        /// <summary>The refusal when the drawing already holds <see cref="MaxPerDrawing"/> drafts.</summary>
        public static readonly string LimitText = "Ya hay " + MaxPerDrawing.ToString(CultureInfo.InvariantCulture)
            + " borradores sin insertar en este dibujo: inserta o descarta alguno antes de empezar otro.";

        /// <summary>
        /// The number of a new draft of a drawing: the lowest one that no draft of that drawing uses now (supuesto ND-02), so a drawing
        /// with «Borrador 1» and «Borrador 3» gets «Borrador 2», and one with none starts again at «Borrador 1».
        /// </summary>
        public static int NextNumber(IEnumerable<int> used)
        {
            var taken = new HashSet<int>(used ?? Enumerable.Empty<int>());
            var number = 1;
            while (taken.Contains(number)) number++;
            return number;
        }

        /// <summary>«Borrador N»: the label of a draft in the window, which does not change while it lives (its rack name may).</summary>
        public static string Label(int number) => "Borrador " + number.ToString(CultureInfo.InvariantCulture);

        /// <summary>What the window calls the draft: the name of its rack when it has one, its label otherwise.</summary>
        public static string Title(int number, string name) => string.IsNullOrWhiteSpace(name) ? Label(number) : name.Trim();

        /// <summary>
        /// The name of a draft in the veto of the close of its drawing and in the notice of a drawing that closed (D-08): «Borrador 2
        /// (Selectivo nuevo)», or «Nave A (Borrador 2, Selectivo nuevo)» when its rack has a name, so two drafts never read the same, not
        /// even two with the same name (point 6 of the review), as two «un Selectivo nuevo» did.
        /// </summary>
        public static string NoticeName(int number, string systemLabel, string name)
            => string.IsNullOrWhiteSpace(name)
                ? Label(number) + " (" + systemLabel + " nuevo)"
                : name.Trim() + " (" + Label(number) + ", " + systemLabel + " nuevo)";

        /// <summary>The system as the window says it, for the kinds of the library.</summary>
        public static string SystemLabel(RackSystemKind kind)
        {
            switch (kind)
            {
                case RackSystemKind.SelectiveRack: return "Selectivo";
                case RackSystemKind.PalletFlow: return "Dinámico";
                case RackSystemKind.Selective: return "Cabecera";
                case RackSystemKind.PushBack: return "Push Back";
                case RackSystemKind.Cantilever: return "Cantilever";
                case RackSystemKind.Cama: return "Cama";
                case RackSystemKind.Larguero: return "Larguero";
                default: return kind.ToString();
            }
        }

        /// <summary>The refusal for a system that has no editor in the window yet.</summary>
        public static string NoEditorText(RackSystemKind kind)
            => SystemLabel(kind) + " todavía no tiene editor en la ventana: se abre con su editor clásico.";
    }

    /// <summary>
    /// One draft of «Nuevo» as the window shows it (T4 new): under «Nuevo» in the side column and in the list of the drafts of the
    /// drawing, with its state. It lives while its draft lives and refreshes in place (D-06 of Dinámico), so a click never loses the
    /// button under the mouse.
    /// </summary>
    public sealed class RackCadDraftItem : ObservableObject
    {
        /// <summary>The state of a draft that is not in the drawing and has nothing changed since it opened.</summary>
        public const string NewKind = "Nuevo";

        public const string ModifiedKind = "Modificado";

        public const string WritingKind = "Escribiendo";

        private string _title;
        private string _subtitle;
        private string _statusText;
        private string _statusKind = NewKind;
        private string _tag;
        private bool _isActive;
        private bool _isConfirmingDiscard;
        private bool _isWriting;
        private bool _isDirty;

        internal RackCadDraftItem(int number, RackSystemKind kind, string rackId)
        {
            Number = number;
            Kind = kind;
            RackId = rackId;
            Label = RackCadDraftNaming.Label(number);
            SystemLabel = RackCadDraftNaming.SystemLabel(kind);
            Refresh(null, dirty: false, writing: false);
        }

        /// <summary>The number of the draft in its drawing.</summary>
        public int Number { get; }

        public RackSystemKind Kind { get; }

        /// <summary>The identity the window gave the rack of the draft (it keeps it once inserted).</summary>
        public string RackId { get; }

        /// <summary>«Borrador N».</summary>
        public string Label { get; }

        /// <summary>«Selectivo», «Dinámico».</summary>
        public string SystemLabel { get; }

        /// <summary>The name of the rack when it has one, «Borrador N» otherwise.</summary>
        public string Title => _title;

        /// <summary>What goes under the title: the system, and the label when the rack has a name.</summary>
        public string Subtitle => _subtitle;

        /// <summary>The line of the side column: the title and the system («Borrador 1 · Selectivo»).</summary>
        public string ColumnText => _title + " · " + SystemLabel;

        /// <summary>The second line of the list of «Nuevo»: the system (and the label, when the rack has a name) and the state.</summary>
        public string DetailText => _subtitle + " · " + _statusText;

        /// <summary>«Sin insertar», «Sin insertar, con cambios», «Escribiendo en el dibujo».</summary>
        public string StatusText => _statusText;

        /// <summary><see cref="NewKind"/>, <see cref="ModifiedKind"/> or <see cref="WritingKind"/>: the dot of the column.</summary>
        public string StatusKind => _statusKind;

        /// <summary>The short tag of the column: «Sin insertar», «Modificado», «Escribiendo».</summary>
        public string Tag => _tag;

        /// <summary>The draft whose editor «Nuevo» shows.</summary>
        public bool IsActive => _isActive;

        public bool IsDirty => _isDirty;

        public bool IsWriting => _isWriting;

        /// <summary>«Descartar el borrador» of a draft with changes asks first, inline, in the list of «Nuevo».</summary>
        public bool IsConfirmingDiscard => _isConfirmingDiscard;

        /// <summary>The draft can be discarded or copied while AutoCAD does not write it.</summary>
        public bool CanDiscard => !_isWriting;

        public bool CanDuplicate => !_isWriting;

        /// <summary>The question of «Descartar el borrador» when it has changes.</summary>
        public string DiscardQuestion => "¿Descartar " + Label + "? Tiene cambios y no está en el dibujo: se pierden.";

        /// <summary>The name of the item of the side column for UI Automation: unique (it has the number), with the system and the state.</summary>
        public string AccessibleName => Label + (string.Equals(_title, Label, StringComparison.Ordinal) ? string.Empty : " (" + _title + ")")
                                        + ", " + SystemLabel + ", " + _statusText;

        public string OpenAccessibleName => "Abrir " + Label;

        public string DuplicateAccessibleName => "Duplicar " + Label;

        public string DiscardAccessibleName => "Descartar " + Label;

        public string ConfirmDiscardAccessibleName => "Confirmar: descartar " + Label;

        public string CancelDiscardAccessibleName => "Cancelar el descarte de " + Label;

        // The state of its editor, refreshed in place.
        internal void Refresh(string name, bool dirty, bool writing)
        {
            var title = RackCadDraftNaming.Title(Number, name);
            var named = !string.Equals(title, Label, StringComparison.Ordinal);
            var subtitle = SystemLabel + (named ? " · " + Label : string.Empty);
            var kind = writing ? WritingKind : dirty ? ModifiedKind : NewKind;
            var status = writing ? "Escribiendo en el dibujo" : dirty ? "Sin insertar, con cambios" : "Sin insertar";
            var tag = writing ? "Escribiendo" : dirty ? "Modificado" : "Sin insertar";

            var changed = !string.Equals(title, _title, StringComparison.Ordinal) || !string.Equals(status, _statusText, StringComparison.Ordinal);
            SetProperty(ref _title, title, nameof(Title));
            SetProperty(ref _subtitle, subtitle, nameof(Subtitle));
            SetProperty(ref _statusText, status, nameof(StatusText));
            SetProperty(ref _statusKind, kind, nameof(StatusKind));
            SetProperty(ref _tag, tag, nameof(Tag));
            SetProperty(ref _isDirty, dirty, nameof(IsDirty));
            if (SetProperty(ref _isWriting, writing, nameof(IsWriting)))
            {
                OnPropertyChanged(nameof(CanDiscard));
                OnPropertyChanged(nameof(CanDuplicate));
            }

            if (changed)
            {
                OnPropertyChanged(nameof(ColumnText));
                OnPropertyChanged(nameof(DetailText));
                OnPropertyChanged(nameof(AccessibleName));
            }

            // A draft without changes has nothing to confirm: a pending question of an older state goes away.
            if (!dirty || writing) SetConfirmingDiscard(false);
        }

        internal void SetActive(bool active) => SetProperty(ref _isActive, active, nameof(IsActive));

        internal void SetConfirmingDiscard(bool confirming) => SetProperty(ref _isConfirmingDiscard, confirming, nameof(IsConfirmingDiscard));
    }
}
