using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RackCad.Application.StructuralSections;
using RackCad.Application.Systems.Cantilever;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>One option of a choice of the Cantilever editor, shown as a segment (no drop-down: O-D of Selectivo).</summary>
    public sealed class CantileverChoiceOptionItem : ObservableObject
    {
        private bool _isSelected;

        internal CantileverChoiceOptionItem(CantileverChoiceItem owner, string value, string label)
        {
            Owner = owner;
            Value = value;
            Label = label;
        }

        public CantileverChoiceItem Owner { get; }

        public string Value { get; }

        public string Label { get; }

        public bool IsSelected { get => _isSelected; internal set => SetProperty(ref _isSelected, value); }

        public string AutomationName => Owner.Label + ": " + Label;
    }

    /// <summary>
    /// A choice of the editor (Caras, Lado, Altura de columna, Arreglo, Placa de extremo…): its options as segments, the hint and the error. A
    /// click on an option goes to the draft at once; the option that is shown changes nothing. Refreshed in place, as the fields.
    /// </summary>
    public sealed class CantileverChoiceItem : ObservableObject
    {
        private Func<string, string> _commit;
        private string _selected;
        private string _hint;
        private string _error = string.Empty;
        private bool _isReadOnly;

        public CantileverChoiceItem(string key, string label, IReadOnlyList<(string Value, string Label)> options, string selected, string hint,
            Func<string, string> commit, bool isReadOnly = false)
        {
            Key = key;
            Label = label;
            Options = options.Select(o => new CantileverChoiceOptionItem(this, o.Value, o.Label)).ToList();
            _commit = commit;
            Apply(selected, hint, isReadOnly);
        }

        public string Key { get; }

        public string Label { get; }

        public IReadOnlyList<CantileverChoiceOptionItem> Options { get; }

        public string SelectedValue => _selected;

        public string Hint { get => _hint; private set => SetProperty(ref _hint, value ?? string.Empty); }

        public string Error { get => _error; private set => SetProperty(ref _error, value ?? string.Empty); }

        public bool IsReadOnly
        {
            get => _isReadOnly;
            private set
            {
                if (SetProperty(ref _isReadOnly, value)) OnPropertyChanged(nameof(IsEnabled));
            }
        }

        public bool IsEnabled => !_isReadOnly;

        /// <summary>The gesture of an option: it goes to the draft, or the choice says why not.</summary>
        public void Choose(string value)
        {
            if (_isReadOnly || _commit == null || string.IsNullOrEmpty(value) || string.Equals(value, _selected, StringComparison.Ordinal)) return;
            Error = _commit(value) ?? string.Empty;
        }

        internal void Refresh(CantileverChoiceItem next)
        {
            _commit = next._commit;
            Apply(next._selected, next._hint, next._isReadOnly);
            Error = string.Empty;
        }

        private void Apply(string selected, string hint, bool isReadOnly)
        {
            _selected = selected;
            foreach (var option in Options) option.IsSelected = string.Equals(option.Value, selected, StringComparison.Ordinal);
            OnPropertyChanged(nameof(SelectedValue));
            Hint = hint;
            IsReadOnly = isReadOnly;
        }
    }

    /// <summary>One row of the list of a section picker: the designation of the catalogue and, in the help, its id (CT-04).</summary>
    public sealed class CantileverSectionChoiceItem
    {
        internal CantileverSectionChoiceItem(CantileverSectionPickerItem owner, StructuralSectionChoice choice)
        {
            Owner = owner;
            Id = choice.Id.Value;
            Designation = choice.Designation;
            Family = StructuralSectionSearch.Label(choice.Family);
        }

        public CantileverSectionPickerItem Owner { get; }

        public string Id { get; }

        public string Designation { get; }

        public string Family { get; }

        public string AutomationName => Owner.Label + ": elegir " + Designation;
    }

    /// <summary>
    /// The search of a section inside the page (CT-04, O-D of Selectivo: no drop-down): the section chosen (by its designation), a box to
    /// search by designation or id, and the list of the families the piece admits, at most <see cref="MaxShown"/> rows. A click on a row goes
    /// to the draft at once; the list opens with «Cambiar» and closes when a section is chosen.
    /// </summary>
    public sealed class CantileverSectionPickerItem : ObservableObject
    {
        public const int MaxShown = 40;

        private readonly IReadOnlyList<StructuralSectionChoice> _all;
        private Func<string, string> _commit;
        private string _sectionId;
        private string _shown;
        private string _hint;
        private string _error = string.Empty;
        private string _search = string.Empty;
        private bool _isOpen;
        private bool _isReadOnly;

        public CantileverSectionPickerItem(string key, string label, IReadOnlyList<StructuralSectionChoice> all, string sectionId, string shown, string hint,
            Func<string, string> commit, bool isReadOnly = false)
        {
            Key = key;
            Label = label;
            _all = all ?? Array.Empty<StructuralSectionChoice>();
            _commit = commit;
            Apply(sectionId, shown, hint, isReadOnly);
            Filter();
        }

        public string Key { get; }

        public string Label { get; }

        public string SectionId => _sectionId;

        /// <summary>The designation of the section chosen, or «Sin elegir».</summary>
        public string ShownText { get => _shown; private set => SetProperty(ref _shown, value); }

        public string Hint { get => _hint; private set => SetProperty(ref _hint, value ?? string.Empty); }

        public string Error { get => _error; private set => SetProperty(ref _error, value ?? string.Empty); }

        public bool IsReadOnly
        {
            get => _isReadOnly;
            private set
            {
                if (SetProperty(ref _isReadOnly, value)) OnPropertyChanged(nameof(IsEnabled));
            }
        }

        public bool IsEnabled => !_isReadOnly;

        public bool IsOpen
        {
            get => _isOpen;
            set
            {
                if (_isReadOnly && value) return;
                if (SetProperty(ref _isOpen, value)) OnPropertyChanged(nameof(ToggleText));
            }
        }

        public string ToggleText => _isOpen ? "Cerrar la lista" : string.IsNullOrEmpty(_sectionId) ? "Elegir…" : "Cambiar…";

        public string SearchText
        {
            get => _search;
            set
            {
                if (!SetProperty(ref _search, value ?? string.Empty)) return;
                Filter();
            }
        }

        public ObservableCollection<CantileverSectionChoiceItem> Choices { get; } = new();

        public string CountText { get; private set; } = string.Empty;

        public void Toggle() => IsOpen = !IsOpen;

        /// <summary>A row of the list was clicked: the section goes to the draft, or the picker says why not.</summary>
        public void Choose(CantileverSectionChoiceItem choice)
        {
            if (choice == null || _isReadOnly || _commit == null) return;
            if (string.Equals(choice.Id, _sectionId, StringComparison.OrdinalIgnoreCase))
            {
                IsOpen = false;
                return;
            }

            var refused = _commit(choice.Id);
            Error = refused ?? string.Empty;
            if (refused == null) IsOpen = false;
        }

        internal void Refresh(CantileverSectionPickerItem next)
        {
            _commit = next._commit;
            Apply(next._sectionId, next._shown, next._hint, next._isReadOnly);
            Error = string.Empty;
            if (_isReadOnly) IsOpen = false;
        }

        private void Apply(string sectionId, string shown, string hint, bool isReadOnly)
        {
            _sectionId = sectionId ?? string.Empty;
            ShownText = string.IsNullOrWhiteSpace(shown) ? "Sin elegir" : shown;
            Hint = hint;
            IsReadOnly = isReadOnly;
            OnPropertyChanged(nameof(SectionId));
            OnPropertyChanged(nameof(ToggleText));
        }

        private void Filter()
        {
            var found = StructuralSectionSearch.Filter(_all, _search);
            Choices.Clear();
            foreach (var choice in found.Take(MaxShown)) Choices.Add(new CantileverSectionChoiceItem(this, choice));
            CountText = found.Count > MaxShown
                ? "Mostrando " + MaxShown + " de " + found.Count + ": escribe para acotar."
                : found.Count == 0 ? "Ninguna sección coincide." : found.Count + (found.Count == 1 ? " sección." : " secciones.");
            OnPropertyChanged(nameof(CountText));
        }
    }

    /// <summary>One cell of the matrix of arms: station × level × side, with ONE datum (the cut length, CT-01) and its marks; refreshed in place.</summary>
    public sealed class CantileverCellItem : ObservableObject
    {
        private string _text = string.Empty;
        private string _detail = string.Empty;
        private bool _isSelected;
        private bool _isPrimary;
        private bool _hasException;
        private bool _hasStop;
        private bool _isMissing;
        private bool _isModified;
        private string _accessibleText = string.Empty;

        public CantileverCellItem(CantileverLineCell cell) => Cell = cell;

        public CantileverLineCell Cell { get; }

        public string Text { get => _text; private set => SetProperty(ref _text, value); }

        /// <summary>The section of the arm («Ver detalle en celdas»).</summary>
        public string Detail { get => _detail; private set => SetProperty(ref _detail, value); }

        public bool IsSelected { get => _isSelected; private set => SetProperty(ref _isSelected, value); }

        public bool IsPrimary { get => _isPrimary; private set => SetProperty(ref _isPrimary, value); }

        /// <summary>«E»: the cell has an exception (its own arm).</summary>
        public bool HasException { get => _hasException; private set => SetProperty(ref _hasException, value); }

        /// <summary>«T»: its arm has an end plate with stop.</summary>
        public bool HasStop { get => _hasStop; private set => SetProperty(ref _hasStop, value); }

        public bool IsMissing { get => _isMissing; private set => SetProperty(ref _isMissing, value); }

        /// <summary>Its arm differs from the one of the base (modified in the draft).</summary>
        public bool IsModified { get => _isModified; private set => SetProperty(ref _isModified, value); }

        public string AccessibleText { get => _accessibleText; private set => SetProperty(ref _accessibleText, value); }

        internal void Set(string text, string detail, bool isSelected, bool isPrimary, bool hasException, bool hasStop, bool isMissing, bool isModified, string accessible)
        {
            Text = text;
            Detail = detail;
            IsSelected = isSelected;
            IsPrimary = isPrimary;
            HasException = hasException;
            HasStop = hasStop;
            IsMissing = isMissing;
            IsModified = isModified;
            AccessibleText = accessible;
        }
    }

    /// <summary>One row of the matrix: «N3 +Y» … «N1 −Y», top to bottom.</summary>
    public sealed class CantileverMatrixRow
    {
        public CantileverMatrixRow(int levelIndex, CantileverArmSide side, string label, string automationName, IReadOnlyList<CantileverCellItem> cells)
        {
            LevelIndex = levelIndex;
            Side = side;
            Label = label;
            AutomationName = automationName;
            Cells = cells;
        }

        public int LevelIndex { get; }

        public CantileverArmSide Side { get; }

        public string Label { get; }

        public string AutomationName { get; }

        public IReadOnlyList<CantileverCellItem> Cells { get; }
    }

    /// <summary>One column head of the matrix: «E1» …, a button that selects the station.</summary>
    public sealed record CantileverColumnHead(int StationIndex, string Title, string AutomationName);

    /// <summary>One computed value of the line («Calculado»).</summary>
    public sealed record CantileverCalcItem(string Label, string Value, string Hint);

    /// <summary>One line of «Para que la línea se resuelva»: what is missing and «Ir a …».</summary>
    public sealed record CantileverMissingItem(string Message, string GoLabel, string Tab, string Component, string AutomationName);

    /// <summary>One line of a «Receta»: what the BOM will count of the piece.</summary>
    public sealed record CantileverRecipeItem(string Key, string Value);

    /// <summary>One panel of «Componentes» (Columna y base · Brazo por omisión · Separador · Tensor), as a segment that selects it.</summary>
    public sealed class CantileverComponentTab : ObservableObject
    {
        private bool _isSelected;
        private bool _hasError;

        public CantileverComponentTab(string key, string label)
        {
            Key = key;
            Label = label;
        }

        public string Key { get; }

        public string Label { get; }

        public bool IsSelected { get => _isSelected; internal set => SetProperty(ref _isSelected, value); }

        public bool HasError { get => _hasError; internal set => SetProperty(ref _hasError, value); }

        public string AutomationName => "Componente: " + Label;
    }

    /// <summary>One row of the table of the advanced sequence (CT-10), a draft until «Aplicar secuencia».</summary>
    public sealed class CantileverSegmentRowItem : ObservableObject
    {
        private readonly Action _changed;
        private string _from;
        private string _to;
        private bool _braced;

        // Correction 1 of the review: the exact elevations the row was made with, kept while its text is the one shown for them.
        private readonly double? _fromValue;
        private readonly double? _toValue;

        internal CantileverSegmentRowItem(int index, CantileverSegmentRow row, Action changed)
        {
            Index = index;
            _from = row.From ?? string.Empty;
            _to = row.To ?? string.Empty;
            _braced = row.Braced;
            _fromValue = row.FromValue;
            _toValue = row.ToValue;
            _changed = changed;
        }

        public int Index { get; }

        public string Number => (Index + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);

        public string From
        {
            get => _from;
            set
            {
                if (!SetProperty(ref _from, (value ?? string.Empty).Trim())) return;
                OnPropertyChanged(nameof(Height));
                _changed?.Invoke();
            }
        }

        public string To
        {
            get => _to;
            set
            {
                if (!SetProperty(ref _to, (value ?? string.Empty).Trim())) return;
                OnPropertyChanged(nameof(Height));
                _changed?.Invoke();
            }
        }

        public bool Braced
        {
            get => _braced;
            set
            {
                if (!SetProperty(ref _braced, value)) return;
                _changed?.Invoke();
            }
        }

        /// <summary>«Alto»: derived from «Desde» and «Hasta».</summary>
        public string Height =>
            UiSupport.TryNum(_from, out var a) && UiSupport.TryNum(_to, out var b) && b > a ? CantileverDraftEditor.Num(b - a) : "—";

        public string FromName => "Tramo " + Number + ": desde (in)";

        public string ToName => "Tramo " + Number + ": hasta (in)";

        public string BracedName => "Tramo " + Number + ": con tensores";

        public string SplitName => "Dividir el tramo " + Number;

        public string RemoveName => "Quitar el tramo " + Number;

        internal CantileverSegmentRow ToRow() => new(_from, _to, _braced, _fromValue, _toValue);
    }

    /// <summary>One tramo of the diagram of the sequence, top to bottom.</summary>
    public sealed record CantileverSequenceItem(string Text, bool IsBraced);

    /// <summary>One box of «Restaurar layout…» (CT-07).</summary>
    public sealed class CantileverRestoreBoxItem : ObservableObject
    {
        private readonly Action _changed;
        private bool _isChecked;

        internal CantileverRestoreBoxItem(string key, string label, int count, IReadOnlyList<string> losses, Action changed)
        {
            Key = key;
            Label = label + " (" + (count == 0 ? "ninguna" : count.ToString(System.Globalization.CultureInfo.InvariantCulture)) + ")";
            Count = count;
            Losses = losses;
            _isChecked = true;
            _changed = changed;
        }

        public string Key { get; }

        public string Label { get; }

        public int Count { get; }

        public IReadOnlyList<string> Losses { get; }

        public bool IsEnabled => Count > 0;

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (Count == 0) return;
                if (SetProperty(ref _isChecked, value)) _changed?.Invoke();
            }
        }
    }
}
