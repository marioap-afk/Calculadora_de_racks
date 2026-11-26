using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using RackCad.Application.CustomProperties;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.VentanaRackCad;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>One view of a rack whose views have different properties, as a choice of the source of the unify (VA-08).</summary>
    public sealed class RackPropertiesSourceItem : ObservableObject
    {
        private bool _isChosen;

        internal RackPropertiesSourceItem(RackPropertiesViewSummary view, int position)
        {
            View = view;
            Position = position;
        }

        internal RackPropertiesViewSummary View { get; }

        public int Position { get; }

        public string Label => View.Label;

        public string Content => View.Content;

        public bool CanBeSource => View.CanBeSource;

        public bool IsChosen { get => _isChosen; internal set => SetProperty(ref _isChosen, value); }

        public string AutomationName => "Usar " + View.Label + " como origen (vista " + Position.ToString(CultureInfo.InvariantCulture) + ")";
    }

    /// <summary>
    /// The tab «Propiedades» at the end of the editors of Selectivo and Dinámico of the RackCad window (I-64 A-14, cuarta tanda, va; VA-07,
    /// note of the Owner: «Hay que crear una pestaña en cada rack en donde se puedan escribir las propiedades»). The custom properties of the
    /// rack, read with it by the door, are a draft of the editor: they make it «Modificado», «Descartar» drops them, and its «Actualizar»
    /// writes them in every view, after the redraw, in the same order of the door. Views with different properties are unified into the
    /// draft from the view the user chooses, after a confirmation (VA-08); none is chosen at first.
    /// </summary>
    public sealed class RackPropertiesTabViewModel : ObservableObject, ICustomPropertiesTable
    {
        private RackPropertiesRead _read;
        private IReadOnlyList<CustomPropertyDraftEntry> _base = Array.Empty<CustomPropertyDraftEntry>();
        private IReadOnlyList<CustomPropertyDraftProblem> _problems = Array.Empty<CustomPropertyDraftProblem>();
        private RackPropertiesSourceItem _source;
        private RackPropertiesViewSummary _unifiedFrom;
        private bool _confirmed;
        private bool _locked;
        private int _nextKey;

        /// <summary>The draft changed: the editor measures its state again.</summary>
        public event Action Changed;

        public ObservableCollection<CustomPropertyRowItem> Properties { get; } = new();

        public ObservableCollection<RackPropertiesSourceItem> Views { get; } = new();

        public bool HasRead => _read != null;

        /// <summary>One collection for every view (or one unified in the draft): the table can be written.</summary>
        public bool IsEditable => _read != null && (_read.Writable || _unifiedFrom != null);

        public bool IsDivergent => _read != null && _read.Divergent && _unifiedFrom == null;

        public bool IsNotDivergent => !IsDivergent && IsEditable;

        public string ReadOnlyText => _read == null
            ? "Las propiedades de este rack no se leyeron con el editor: siguen en RACKPROPIEDADES."
            : !IsEditable && !IsDivergent ? _read.Reason ?? string.Empty : string.Empty;

        public bool IsReadOnly => !IsEditable && !IsDivergent;

        public bool IsDirty => IsEditable && (_unifiedFrom != null || !CustomPropertiesBatch.IsUnchanged(DocumentOfBase(), Draft()));

        public bool HasProblems => IsEditable && _problems.Count > 0;

        /// <summary>Why «Actualizar» cannot write the properties; empty when it can.</summary>
        public string BlockedText => HasProblems
            ? "Propiedades: " + (_problems.Count == 1 ? "hay 1 valor no válido." : "hay " + _problems.Count + " valores no válidos.")
            : string.Empty;

        public string TabHeader => HasProblems || IsDivergent ? "Propiedades (con problemas)" : "Propiedades";

        public string PropertiesTitle => "Propiedades personalizadas";

        public string PropertiesNote => _read == null ? string.Empty
            : "Nombre y valor de texto guardados en " + ViewsText(_read.ViewCount) + " del rack. Van al borrador del rack y se escriben con su «Actualizar», junto con el resto del editor.";

        public string PropertiesHelp =>
            "Hasta 50 propiedades. El nombre es obligatorio, de hasta 80 caracteres y sin repetir otro. El valor puede quedar vacío, admite varias líneas y llega a 1000 caracteres.";

        public string PropertiesRemovedText
        {
            get
            {
                var removed = _base.Where(b => Properties.All(p => !string.Equals(p.Id, b.Id, StringComparison.OrdinalIgnoreCase))).Select(b => "«" + b.Name + "»").ToList();
                return removed.Count == 0 || _unifiedFrom != null ? string.Empty : "Se quitarán al actualizar: " + ProjectVariablesBatch.List(removed) + ".";
            }
        }

        public string PropertiesCount => Properties.Count + " de " + CustomPropertiesMutations.MaxEntriesOnCreate;

        public bool CanEditProperties => IsEditable && !_locked;

        public bool CanAddProperty => CanEditProperties && Properties.Count < CustomPropertiesMutations.MaxEntriesOnCreate;

        public bool HasNoProperties => IsEditable && Properties.Count == 0;

        // ------------------------------------------------------------------ the unify (VA-08)

        public bool Confirmed
        {
            get => _confirmed;
            set
            {
                if (_source == null || _locked) value = false;
                if (SetProperty(ref _confirmed, value)) RaiseUnify();
            }
        }

        public bool CanConfirm => _source != null && !_locked;

        public bool CanUnify => IsDivergent && _source != null && _confirmed && !_locked;

        public string UnifyWhy => _source == null
            ? "Elige la vista origen: ninguna viene elegida."
            : !_confirmed ? "Marca la casilla para confirmar que revisaste el contenido de cada vista."
            : "Lo unificado va al borrador; «Actualizar» lo escribe en " + ViewsText(_read?.ViewCount ?? 0) + ".";

        public string UnifiedText => _unifiedFrom == null ? string.Empty
            : "Unificado en el borrador desde «" + _unifiedFrom.Label + "». Puedes retocarlo; «Actualizar» lo escribe en " + ViewsText(_read?.ViewCount ?? 0) + ".";

        public void ChooseSource(RackPropertiesSourceItem item)
        {
            if (_locked || item == null || !item.CanBeSource || !Views.Contains(item)) return;
            _source = item;
            foreach (var view in Views) view.IsChosen = ReferenceEquals(view, item);
            _confirmed = false;
            OnPropertyChanged(nameof(Confirmed));
            RaiseUnify();
        }

        /// <summary>«Unificar en el borrador»: the properties of the chosen view become the draft of the rack, to retouch before «Actualizar».</summary>
        public void Unify()
        {
            if (!CanUnify) return;
            _unifiedFrom = _source.View;
            Fill(_unifiedFrom.Entries ?? Array.Empty<CustomPropertyDraftEntry>());
            Recompute();
        }

        // ------------------------------------------------------------------ the editor

        internal void Load(RackPropertiesRead read)
        {
            _read = read;
            _unifiedFrom = null;
            _source = null;
            _confirmed = false;
            _base = read?.Writable == true ? read.Entries ?? Array.Empty<CustomPropertyDraftEntry>() : Array.Empty<CustomPropertyDraftEntry>();
            Views.Clear();
            if (read != null && read.Divergent)
            {
                var position = 0;
                foreach (var view in read.Views ?? Array.Empty<RackPropertiesViewSummary>()) Views.Add(new RackPropertiesSourceItem(view, ++position));
            }

            Fill(_base);
            Recompute();
        }

        /// <summary>«Descartar» of the editor: back to what was read with the rack (or written last).</summary>
        internal void Reset() => Load(_read);

        /// <summary>The answer of a write: the properties the drawing holds now (with the ids the creates minted), or null when they did not change.</summary>
        internal void Written(RackPropertiesRead after)
        {
            if (after != null) Load(after);
        }

        internal void Lock(bool locked)
        {
            if (_locked == locked) return;
            _locked = locked;
            RaiseAll();
        }

        /// <summary>What «Actualizar» writes, or null when the tab writes nothing.</summary>
        internal RackPropertiesWrite BuildWrite()
            => IsDirty && !HasProblems ? new RackPropertiesWrite(_read.Fingerprint, Draft(), _unifiedFrom?.Handle) : null;

        public void AddProperty()
        {
            if (!CanAddProperty) return;
            Properties.Add(new CustomPropertyRowItem("nueva-" + (++_nextKey).ToString(CultureInfo.InvariantCulture), null, string.Empty, string.Empty));
            Recompute();
        }

        public void RemoveProperty(CustomPropertyRowItem row)
        {
            if (!CanEditProperties || row == null || !Properties.Remove(row)) return;
            Recompute();
        }

        public void CommitProperties()
        {
            if (!IsEditable) return;
            Recompute();
        }

        private IReadOnlyList<CustomPropertyDraftEntry> Draft() => Properties.Select(p => p.ToDraft()).ToList();

        private CustomPropertiesDocument DocumentOfBase()
        {
            var document = CustomPropertiesDocument.CreateNew();
            foreach (var entry in _base)
                document.Entries.Add(new CustomPropertyEntryDocument { Id = entry.Id, Name = entry.Name, Value = entry.Value });
            return document;
        }

        private void Fill(IReadOnlyList<CustomPropertyDraftEntry> entries)
        {
            Properties.Clear();
            foreach (var entry in entries) Properties.Add(new CustomPropertyRowItem(entry.Id ?? ("nueva-" + (++_nextKey)), entry.Id, entry.Name, entry.Value));
        }

        private void Recompute()
        {
            // The base of a unified draft is the collection of its source: what each row introduces is judged against it.
            var over = _unifiedFrom == null ? DocumentOfBase() : DocumentOf(_unifiedFrom.Entries);
            _problems = IsEditable
                ? CustomPropertiesBatch.Check(new CustomPropertiesStore().Read(CustomPropertiesPayload.Present(new CustomPropertiesStore().Serialize(over))), Draft())
                : Array.Empty<CustomPropertyDraftProblem>();
            var baseRows = _unifiedFrom == null ? _base : _unifiedFrom.Entries ?? Array.Empty<CustomPropertyDraftEntry>();
            for (var i = 0; i < Properties.Count; i++)
            {
                var row = Properties[i];
                row.Position = i + 1;
                var before = baseRows.FirstOrDefault(b => string.Equals(b.Id, row.Id, StringComparison.OrdinalIgnoreCase));
                row.Tag = row.IsNew ? "Nueva"
                    : before != null && (!string.Equals(before.Name, row.Name.Trim(), StringComparison.Ordinal) || !string.Equals(before.Value, row.Value, StringComparison.Ordinal))
                        ? "Modificada" : string.Empty;
                row.NameError = string.Join(" ", _problems.Where(p => p.Index == i && p.Field == CustomPropertyDraftField.Name).Select(p => p.Text));
                row.ValueError = string.Join(" ", _problems.Where(p => p.Index == i && p.Field == CustomPropertyDraftField.Value).Select(p => p.Text));
            }

            RaiseAll();
            Changed?.Invoke();
        }

        private static CustomPropertiesDocument DocumentOf(IReadOnlyList<CustomPropertyDraftEntry> entries)
        {
            var document = CustomPropertiesDocument.CreateNew();
            foreach (var entry in entries ?? Array.Empty<CustomPropertyDraftEntry>())
                document.Entries.Add(new CustomPropertyEntryDocument { Id = entry.Id, Name = entry.Name, Value = entry.Value });
            return document;
        }

        private static string ViewsText(int count) => count == 1 ? "la vista" : "las " + count.ToString(CultureInfo.InvariantCulture) + " vistas";

        private void RaiseUnify()
        {
            OnPropertyChanged(nameof(CanConfirm));
            OnPropertyChanged(nameof(CanUnify));
            OnPropertyChanged(nameof(UnifyWhy));
        }

        private void RaiseAll()
        {
            foreach (var name in new[]
                     {
                         nameof(HasRead), nameof(IsEditable), nameof(IsDivergent), nameof(IsNotDivergent), nameof(ReadOnlyText), nameof(IsReadOnly),
                         nameof(IsDirty), nameof(HasProblems), nameof(BlockedText), nameof(TabHeader), nameof(PropertiesNote), nameof(PropertiesRemovedText),
                         nameof(PropertiesCount), nameof(CanEditProperties), nameof(CanAddProperty), nameof(HasNoProperties), nameof(UnifiedText),
                         nameof(Confirmed),
                     })
                OnPropertyChanged(name);
            RaiseUnify();
        }
    }
}
