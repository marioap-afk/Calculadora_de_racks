using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>One option of a list of the Selectivo editor: the value it writes and what it shows.</summary>
    public sealed record SelectiveChoiceOption(string Value, string Label);

    /// <summary>
    /// One value of the Selectivo editor chosen from a list (a post or a larguero of the catalog, a peralte the catalog allows, the detail of
    /// the dimensions): its label, its options, the hint when the selection has several values, and its error. Like the field of the
    /// editor, the item lives as long as the editor and a recomposition refreshes it in place. Choosing the value it shows changes nothing.
    /// </summary>
    public sealed class SelectiveChoiceItem : ObservableObject
    {
        private Func<string, string> _commit;
        private IReadOnlyList<SelectiveChoiceOption> _options;
        private string _selectedValue;
        private string _shownValue;
        private string _hint;
        private string _error = string.Empty;
        private bool _isReadOnly;
        private bool _committing;

        public SelectiveChoiceItem(string key, string label, IReadOnlyList<SelectiveChoiceOption> options, string selectedValue, string hint,
            Func<string, string> commit, bool isReadOnly = false)
        {
            Key = key;
            Label = label;
            _options = options ?? Array.Empty<SelectiveChoiceOption>();
            _selectedValue = selectedValue;
            _shownValue = selectedValue;
            _hint = hint ?? string.Empty;
            _commit = commit;
            _isReadOnly = isReadOnly;
        }

        public string Key { get; }

        public string Label { get; }

        public IReadOnlyList<SelectiveChoiceOption> Options => _options;

        /// <summary>«Varía» when the values of the selection differ and nothing is chosen; empty otherwise.</summary>
        public string Hint
        {
            get => _hint;
            private set => SetProperty(ref _hint, value ?? string.Empty);
        }

        public string Error
        {
            get => _error;
            private set => SetProperty(ref _error, value ?? string.Empty);
        }

        public bool IsReadOnly
        {
            get => _isReadOnly;
            private set
            {
                if (SetProperty(ref _isReadOnly, value)) OnPropertyChanged(nameof(IsEnabled));
            }
        }

        public bool IsEnabled => !_isReadOnly;

        /// <summary>
        /// The value chosen. A choice of the user goes to the draft at once; the value it showed, or no value (a list that empties while it
        /// is refreshed), changes nothing.
        /// </summary>
        public string SelectedValue
        {
            get => _selectedValue;
            set
            {
                if (string.IsNullOrEmpty(value) || string.Equals(value, _selectedValue, StringComparison.Ordinal)) return;
                _selectedValue = value;
                OnPropertyChanged(nameof(SelectedValue));
                Commit();
            }
        }

        private void Commit()
        {
            if (_committing || _commit == null || _isReadOnly) return;
            if (string.Equals(_selectedValue, _shownValue, StringComparison.Ordinal))
            {
                Error = string.Empty;
                return;
            }

            _committing = true;
            try
            {
                Error = _commit(_selectedValue) ?? string.Empty;
            }
            finally
            {
                _committing = false;
            }
        }

        // A recomposition refreshed the choice: what it shows and writes, its options (replaced only when they change), and its state.
        internal void Refresh(SelectiveChoiceItem next)
        {
            _commit = next._commit;
            _shownValue = next._shownValue;
            if (!_options.SequenceEqual(next._options))
            {
                _options = next._options;
                OnPropertyChanged(nameof(Options));
            }

            Hint = next._hint;
            IsReadOnly = next._isReadOnly;
            _selectedValue = next._selectedValue;
            OnPropertyChanged(nameof(SelectedValue));
            Error = string.Empty;
        }
    }

    /// <summary>
    /// One cell of the matrix of the Selectivo editor: front × level over the fondos of the scope, with one datum (the pallet, width ×
    /// height; decision 1 of the Owner on Selectivo), «varía» when the fondos differ, and the floor line of a level on the floor with no
    /// larguero (decision 4). Refreshed in place.
    /// </summary>
    public sealed class SelectiveCellItem : ObservableObject
    {
        private string _text = string.Empty;
        private string _detail = string.Empty;
        private bool _showDetail;
        private bool _exists;
        private bool _isSelected;
        private bool _isPrimary;
        private bool _isOnFloor;
        private bool _varies;
        private string _accessibleText = string.Empty;

        public SelectiveCellItem(int frontIndex, int levelIndex)
        {
            FrontIndex = frontIndex;
            LevelIndex = levelIndex;
        }

        public int FrontIndex { get; }

        public int LevelIndex { get; }

        public string Text { get => _text; private set => SetProperty(ref _text, value); }

        /// <summary>«Ver detalle en celdas» (decision 1): the pallets per level and the peralte, or «piso».</summary>
        public string Detail { get => _detail; private set => SetProperty(ref _detail, value); }

        public bool ShowDetail { get => _showDetail; private set => SetProperty(ref _showDetail, value); }

        public bool Exists { get => _exists; private set => SetProperty(ref _exists, value); }

        public bool IsSelected { get => _isSelected; private set => SetProperty(ref _isSelected, value); }

        public bool IsPrimary { get => _isPrimary; private set => SetProperty(ref _isPrimary, value); }

        /// <summary>N1 of a front with no «Larguero a piso»: the pallet is on the floor, with no larguero (the floor line of decision 4).</summary>
        public bool IsOnFloor { get => _isOnFloor; private set => SetProperty(ref _isOnFloor, value); }

        /// <summary>The fondos of the scope do not have the same pallet in this cell («varía»; decision 2).</summary>
        public bool Varies { get => _varies; private set => SetProperty(ref _varies, value); }

        public string AccessibleText { get => _accessibleText; private set => SetProperty(ref _accessibleText, value); }

        internal void Set(string text, string detail, bool showDetail, bool exists, bool isSelected, bool isPrimary, bool isOnFloor, bool varies, string accessible)
        {
            Text = text;
            Detail = detail;
            ShowDetail = showDetail && exists;
            Exists = exists;
            IsSelected = isSelected;
            IsPrimary = isPrimary;
            IsOnFloor = isOnFloor;
            Varies = varies;
            AccessibleText = accessible;
        }
    }

    /// <summary>One row of the matrix: «N4» … «N1», top to bottom.</summary>
    public sealed class SelectiveMatrixRow
    {
        public SelectiveMatrixRow(int levelIndex, string label, IReadOnlyList<SelectiveCellItem> cells)
        {
            LevelIndex = levelIndex;
            Label = label;
            Cells = cells;
        }

        public int LevelIndex { get; }

        public string Label { get; }

        public IReadOnlyList<SelectiveCellItem> Cells { get; }
    }

    /// <summary>One column head of the matrix: «F1» …, with «½» for a front in tramos (decision 7) and the fondos that have it; refreshed in place.</summary>
    public sealed class SelectiveColumnHead : ObservableObject
    {
        private string _title;
        private bool _isPartial;
        private string _accessibleText = string.Empty;

        public SelectiveColumnHead(int frontIndex)
        {
            FrontIndex = frontIndex;
            _title = "F" + (frontIndex + 1).ToString(CultureInfo.InvariantCulture);
        }

        public int FrontIndex { get; }

        public string Title { get => _title; private set => SetProperty(ref _title, value); }

        /// <summary>«Todos los fondos» and only some fondos have this front (decision 2: «un frente que solo existe en un fondo lo indica»).</summary>
        public bool IsPartial { get => _isPartial; private set => SetProperty(ref _isPartial, value); }

        public string AccessibleText { get => _accessibleText; private set => SetProperty(ref _accessibleText, value); }

        internal void Set(string title, bool isPartial, string accessible)
        {
            Title = title;
            IsPartial = isPartial;
            AccessibleText = accessible;
        }
    }

    /// <summary>One computed value of the rack (read only).</summary>
    public sealed record SelectiveCalcItem(string Label, string Value);

    /// <summary>One post of «Avanzado»: its own peralte (empty = the one of the rack) and its cabecera in each fondo; refreshed in place.</summary>
    public sealed class SelectivePostRow : ObservableObject
    {
        private string _cabecera;

        public SelectivePostRow(int postIndex, DynamicFieldItem field, string cabecera)
        {
            PostIndex = postIndex;
            Post = "P" + (postIndex + 1).ToString(CultureInfo.InvariantCulture);
            Field = field;
            _cabecera = cabecera ?? string.Empty;
        }

        public int PostIndex { get; }

        public string Post { get; }

        public DynamicFieldItem Field { get; }

        /// <summary>«Calculada» or «Personalizada», by fondo: the custom cabeceras are kept and edited in the classic editor.</summary>
        public string Cabecera { get => _cabecera; private set => SetProperty(ref _cabecera, value ?? string.Empty); }

        internal void Refresh(SelectivePostRow next)
        {
            Field.Refresh(next.Field);
            Cabecera = next._cabecera;
        }
    }

    /// <summary>One tramo of «Medio frente» in the inspector of the front, while it is written (applied with «Aplicar tramos»).</summary>
    public sealed class SelectiveSegmentRow : ObservableObject
    {
        private string _lengthText;
        private bool _loaded;

        public SelectiveSegmentRow(int index, bool isRest, string lengthText, bool loaded)
        {
            Index = index;
            IsRest = isRest;
            _lengthText = lengthText ?? string.Empty;
            _loaded = loaded;
        }

        public int Index { get; }

        public string Title => "Tramo " + (Index + 1).ToString(CultureInfo.InvariantCulture);

        /// <summary>The last tramo is calculated: what is left of the front.</summary>
        public bool IsRest { get; }

        public bool HasLength => !IsRest;

        public string LengthText { get => _lengthText; set => SetProperty(ref _lengthText, value ?? string.Empty); }

        /// <summary>«con largueros».</summary>
        public bool Loaded { get => _loaded; set => SetProperty(ref _loaded, value); }
    }

    /// <summary>
    /// The editor of ONE Selectivo rack in the RackCad window (I-64 A-14, line D, Selectivo E1-B): the draft the Owner approved, with the
    /// tabs Rack, Matriz (the first one) and Avanzado and no preview (decision 11), over the same recomposition as the classic editor
    /// (<see cref="SelectiveDraftEditor"/>). It follows the editor of Dinámico without changing it: the same fields, the same one way to
    /// confirm (a field goes to the draft when it is left or with Enter; leaving it without changing its text changes nothing), the same
    /// state (Limpio / Modificado / Escribiendo, where «Modificado» is a difference of the JSON of the design with what was last read or
    /// written, never a gesture), the same «Descartar» and «Cerrar el editor».
    ///
    /// <para>
    /// The scope has two axes. The fondos (decision 2): «Todos los fondos», where it starts, or «Fondo n»; what is read over the scope says
    /// «Varía» when the fondos differ, and a front that only some fondos have is said. The cells: the selection of the matrix, which this
    /// view-model keeps; the draft receives positions (<see cref="SelectiveMatrixPosition"/>) and explicit fronts.
    /// </para>
    /// <para>
    /// A write that the draft refuses writes nothing and its reason goes to its field; a write that cannot be recomposed goes back to the
    /// last valid recomposition (<see cref="SelectiveDraftEditor.RestoreLastValid"/>, I-09 of Dinámico), with the selection as it was. A
    /// holgura governed by a project variable or a formula shows its effective value read only, with the reason and the name of the
    /// variable, and nothing here offers «=Variable» (decision 12: the work of <c>LinkedPropertyEditor</c> stays, out of this view).
    /// Nothing here writes the drawing: «Actualizar» hands the editor to the host, and while AutoCAD writes nothing can be edited.
    /// </para>
    /// </summary>
    public sealed partial class SelectiveEditorViewModel : ObservableObject
    {
        private const string Varies = "Varía";
        private const string WritingText = "AutoCAD está escribiendo el rack: espera a que termine o pulsa «Dejar de esperar».";

        private static readonly SelectivePalletDesignStore Store = new SelectivePalletDesignStore();
        private static readonly PropertyId Tolerance = ProjectPropertyIds.SelectivePalletTolerance;
        private static readonly PropertyId Clearance = ProjectPropertyIds.SelectiveVerticalClearance;
        private static readonly SelectiveDraftEdit Done = new SelectiveDraftEdit(1, 0, null);

        private static readonly IReadOnlyList<(DimensionDetail Detail, string Label)> DimensionDetails = new[]
        {
            (DimensionDetail.None, "Ninguna"),
            (DimensionDetail.Minimal, "Mínimo"),
            (DimensionDetail.Standard, "Estándar"),
            (DimensionDetail.Detailed, "Detallado"),
        };

        private readonly SelectiveDraftEditor _editor;
        private readonly Action<SelectiveEditorViewModel> _update;
        private readonly IReadOnlyDictionary<PropertyId, LinkedPropertyEditState> _openedStates;
        private readonly IReadOnlyDictionary<PropertyId, string> _variableNames;
        private readonly List<SelectiveMatrixPosition> _selection = new List<SelectiveMatrixPosition>();
        private SelectiveMatrixPosition _primary;
        private SelectiveMatrixPosition _anchor;

        private int _selectedTab = 1;
        private bool _dirty;
        private bool _writing;
        private bool _confirmingClose;
        private bool _showDetail;
        private string _loadError;
        private string _baselineJson;
        private string _baselineName = string.Empty;
        private string _message = string.Empty;
        private bool _messageIsError;

        private IReadOnlyList<object> _essentials = Array.Empty<object>();
        private IReadOnlyList<DynamicFieldItem> _separatorFields = Array.Empty<DynamicFieldItem>();
        private IReadOnlyList<object> _annotationFields = Array.Empty<object>();
        private IReadOnlyList<object> _cellFields = Array.Empty<object>();
        private IReadOnlyList<DynamicFieldItem> _frontFields = Array.Empty<DynamicFieldItem>();
        private IReadOnlyList<SelectivePostRow> _postRows = Array.Empty<SelectivePostRow>();
        private IReadOnlyList<SelectiveCalcItem> _calculated = Array.Empty<SelectiveCalcItem>();
        private IReadOnlyList<string> _halfFronts = Array.Empty<string>();
        private IReadOnlyList<SelectiveColumnHead> _columns = Array.Empty<SelectiveColumnHead>();
        private IReadOnlyList<SelectiveMatrixRow> _rows = Array.Empty<SelectiveMatrixRow>();
        private IReadOnlyList<string> _fondoOptions = Array.Empty<string>();

        // «Medio frente»: the tramos being written in the inspector, for one front in one scope, until «Aplicar tramos».
        private List<SelectiveSegmentRow> _segmentRows = new List<SelectiveSegmentRow>();
        private (int Front, int? Scope)? _segmentKey;
        private string _segmentError = string.Empty;

        /// <param name="editor">The draft of the rack, as it was read (its design is the baseline of «Modificado» and of «Descartar»).</param>
        /// <param name="rackId">The identity of the rack in the drawing.</param>
        /// <param name="update">What «Actualizar» does with this editor (the host writes the drawing); null when it is not offered.</param>
        /// <param name="linkedStates">The states of the linked properties the rack was opened with: «Descartar» loads the draft again with them.</param>
        /// <param name="linkedVariableNames">The names of the variables that govern them, for display only.</param>
        public SelectiveEditorViewModel(
            SelectiveDraftEditor editor,
            string rackId,
            Action<SelectiveEditorViewModel> update = null,
            IReadOnlyDictionary<PropertyId, LinkedPropertyEditState> linkedStates = null,
            IReadOnlyDictionary<PropertyId, string> linkedVariableNames = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            RackId = rackId;
            _update = update;
            _openedStates = linkedStates == null ? null : new Dictionary<PropertyId, LinkedPropertyEditState>(linkedStates);
            _variableNames = linkedVariableNames == null ? null : new Dictionary<PropertyId, string>(linkedVariableNames);

            // A rack that the draft could not recompose when it was opened has no valid design to compare, to roll back to or to write:
            // it is shown read only, with the reason (S-08).
            if (editor.Design == null) _loadError = string.IsNullOrWhiteSpace(editor.LastError) ? "sin detalle." : editor.LastError;

            _selection.Add(new SelectiveMatrixPosition(0, 0));
            TakeBaseline();
            Rebuild();
            if (_loadError != null) Say(LoadFailedText, error: true);
            else if (!string.IsNullOrWhiteSpace(editor.LoadNotice)) Say(editor.LoadNotice, error: false);
        }

        /// <summary>«Cerrar el editor» was confirmed (or the draft had nothing to lose): the host forgets this editor.</summary>
        public event Action<SelectiveEditorViewModel> CloseRequested;

        // ------------------------------------------------------------------ what the editor shows

        public SelectiveDraftEditor Draft => _editor;

        public string RackId { get; }

        public string Name
        {
            get => _editor.Name ?? string.Empty;
            set
            {
                var next = value ?? string.Empty;
                if (string.Equals(_editor.Name ?? string.Empty, next, StringComparison.Ordinal)) return;
                if (!CanEdit)
                {
                    OnPropertyChanged(nameof(Name));
                    return;
                }

                _editor.Name = next;
                OnPropertyChanged(nameof(Name));
                UpdateDirty();
            }
        }

        /// <summary>0 = Rack, 1 = Matriz (the first tab, as the Owner decided for Dinámico), 2 = Avanzado.</summary>
        public int SelectedTab
        {
            get => _selectedTab;
            set => SetProperty(ref _selectedTab, value);
        }

        public bool IsDirty => _dirty;

        /// <summary>AutoCAD is writing this draft: nothing can be edited until it answers or the window stops waiting.</summary>
        public bool IsWriting => _writing;

        /// <summary>The rack could not be recomposed when it was opened: it is read only here (S-08).</summary>
        public bool IsLoadFailed => _loadError != null;

        public bool CanEdit => !_writing && _loadError == null;

        public bool IsNameReadOnly => !CanEdit;

        public bool CanDiscard => _dirty && CanEdit;

        /// <summary>«Cerrar el editor» is offered while AutoCAD does not write this draft.</summary>
        public bool CanClose => !_writing;

        public string StatusText => _writing ? "Escribiendo" : _dirty ? "Modificado" : "Limpio";

        public string Message => _message;

        public bool MessageIsError => _messageIsError;

        public bool CanUpdate => _update != null && _loadError == null && _dirty && !_writing && _editor.Design != null && string.IsNullOrEmpty(_editor.LastError);

        public string UpdateBlockedText
        {
            get
            {
                if (_update == null) return "Aquí no se puede escribir el dibujo.";
                if (_loadError != null) return LoadFailedText;
                if (_writing) return WritingText;
                if (!_dirty) return "Sin cambios que escribir.";
                if (!string.IsNullOrEmpty(_editor.LastError)) return _editor.LastError;
                return string.Empty;
            }
        }

        private string LoadFailedText =>
            "No se pudo recomponer el rack al abrirlo: " + _loadError + " Aquí queda de solo lectura; edítalo con RACKEDITAR (el editor clásico).";

        /// <summary>«Cerrar el editor» of a draft with changes asks first, inline: discard them and close, or cancel.</summary>
        public bool IsConfirmingClose => _confirmingClose;

        /// <summary>«Lo esencial» of the rack: the post and its peralte, the fronts and the depths of the fondos of the scope, the fondos and the holguras.</summary>
        public IReadOnlyList<object> Essentials => _essentials;

        /// <summary>The separation of each gap between consecutive fondos (none with one fondo).</summary>
        public IReadOnlyList<DynamicFieldItem> SeparatorFields => _separatorFields;

        /// <summary>«Poste y anotaciones»: the annotation scale and the dimensions.</summary>
        public IReadOnlyList<object> AnnotationFields => _annotationFields;

        public IReadOnlyList<SelectiveCalcItem> Calculated => _calculated;

        public IReadOnlyList<SelectiveColumnHead> Columns => _columns;

        public IReadOnlyList<SelectiveMatrixRow> Rows => _rows;

        /// <summary>The inspector: the pallet, the larguero and the manual values of the selected cells.</summary>
        public IReadOnlyList<object> CellFields => _cellFields;

        /// <summary>The inspector: the levels, the floor larguero and the manual height of the fronts of the selection.</summary>
        public IReadOnlyList<DynamicFieldItem> FrontFields => _frontFields;

        /// <summary>«Avanzado»: the own peralte of each post and its cabecera.</summary>
        public IReadOnlyList<SelectivePostRow> PostRows => _postRows;

        /// <summary>«Avanzado»: the fronts in tramos.</summary>
        public IReadOnlyList<string> HalfFronts => _halfFronts;

        public string HalfFrontsText => _halfFronts.Count == 0 ? "Ningún frente está partido en tramos." : string.Join(" · ", _halfFronts);

        /// <summary>«Ver detalle en celdas» (decision 1): each cell adds its pallets per level and its peralte.</summary>
        public bool ShowDetail
        {
            get => _showDetail;
            set
            {
                if (_showDetail == value) return;
                _showDetail = value;
                OnPropertyChanged(nameof(ShowDetail));
                RebuildSelection();
            }
        }

        // ------------------------------------------------------------------ the scope of the fondos (decision 2)

        /// <summary>The rack has more than one fondo: the matrix offers «Todos los fondos / Fondo 1 / Fondo n».</summary>
        public bool HasFondos => _editor.FondoCount > 1;

        public IReadOnlyList<string> FondoOptions => _fondoOptions;

        /// <summary>0 = «Todos los fondos» (where the window starts), n = «Fondo n». It changes what is shown and written, not the draft.</summary>
        public int FondoScopeIndex
        {
            get => _editor.FondoScope is int fondo ? fondo + 1 : 0;
            set
            {
                // A list that is being replaced loses its selection (-1): that is not a choice of the user.
                if (value < 0) return;
                var scope = value == 0 ? (int?)null : value - 1;
                if (scope == _editor.FondoScope) return;
                _editor.SetFondoScope(scope);
                Rebuild();
                OnPropertyChanged(nameof(FondoScopeIndex));
                OnPropertyChanged(nameof(FondoScopeText));
            }
        }

        /// <summary>Where the matrix and the inspector write (decision 2: «lo que ves es donde escribes»).</summary>
        public string FondoScopeText
        {
            get
            {
                if (!HasFondos) return string.Empty;
                return _editor.FondoScope is int fondo
                    ? "Lo que ves es donde escribes: solo en el fondo " + (fondo + 1).ToString(CultureInfo.InvariantCulture) + "."
                    : "Lo que ves es donde escribes: cada celda escribe en el mismo frente y nivel de todos los fondos que lo tienen; «varía» si no coinciden.";
            }
        }

        // ------------------------------------------------------------------ the inspector

        public IReadOnlyList<SelectiveMatrixPosition> SelectedPositions => _selection.ToList();

        public IReadOnlyList<int> SelectedFronts => _selection.Select(p => p.FrontIndex).Distinct().OrderBy(f => f).ToList();

        public SelectiveMatrixPosition PrimaryPosition => _primary;

        public string InspectorTitle
        {
            get
            {
                if (_selection.Count == 1) return "Celda F" + (_selection[0].FrontIndex + 1) + " · N" + (_selection[0].LevelIndex + 1);
                var fronts = SelectedFronts.Count;
                return _selection.Count.ToString(CultureInfo.InvariantCulture) + " celdas en " + fronts.ToString(CultureInfo.InvariantCulture)
                       + (fronts == 1 ? " frente" : " frentes");
            }
        }

        /// <summary>What the selection does not have in every fondo of the scope, and what N1 on the floor means; empty otherwise.</summary>
        public string InspectorNote
        {
            get
            {
                var notes = new List<string>();
                var partial = SelectedFronts
                    .Select(f => (Front: f, In: _editor.ScopeFondos.Where(k => f < _editor.FrontCount(k)).ToList()))
                    .Where(x => x.In.Count < _editor.ScopeFondos.Count)
                    .ToList();
                foreach (var (front, fondos) in partial)
                {
                    notes.Add("F" + (front + 1) + " solo existe en " + FondoList(fondos) + ": se escribe solo donde existe.");
                }

                if (partial.Count == 0 && _editor.ReadCells(_selection, c => 0).Missing > 0)
                    notes.Add("Parte de la selección no existe en todos los fondos de la vista: se escribe solo donde existe.");

                if (_selection.Any(p => IsOnFloor(p.FrontIndex, p.LevelIndex)))
                    notes.Add("N1 es la tarima en el piso: no lleva larguero, salvo con «Larguero a piso» (en el frente).");

                return string.Join(" ", notes);
            }
        }

        /// <summary>
        /// «Larguero a piso» of the fronts of the selection: true when all have it, false when none, null when they differ. Writing it puts
        /// it in, or takes it out of, those fronts in the fondos of the scope.
        /// </summary>
        public bool? SelectionFloorBeam
        {
            get
            {
                var value = _editor.ReadFronts(SelectedFronts, f => f.FloorBeam);
                return value.Count == 0 ? false : value.Varies ? (bool?)null : value.Value;
            }
            set
            {
                if (!value.HasValue || value == SelectionFloorBeam)
                {
                    OnPropertyChanged(nameof(SelectionFloorBeam));
                    return;
                }

                var fronts = SelectedFronts;
                var error = Apply(() => _editor.SetFloorBeam(fronts, value.Value));
                if (error != null) Say(error, error: true);
                OnPropertyChanged(nameof(SelectionFloorBeam));
            }
        }

        // ------------------------------------------------------------------ «Poste y anotaciones»

        public bool DrawBasePlate
        {
            get => _editor.DrawBasePlate;
            set { if (_editor.DrawBasePlate != value) SetFlag(() => _editor.DrawBasePlate = value, nameof(DrawBasePlate)); }
        }

        public bool NumberFronts
        {
            get => _editor.NumberFronts;
            set { if (_editor.NumberFronts != value) SetFlag(() => _editor.NumberFronts = value, nameof(NumberFronts)); }
        }

        public bool NumberLevels
        {
            get => _editor.NumberLevels;
            set { if (_editor.NumberLevels != value) SetFlag(() => _editor.NumberLevels = value, nameof(NumberLevels)); }
        }

        public bool DrawRackName
        {
            get => _editor.DrawRackName;
            set { if (_editor.DrawRackName != value) SetFlag(() => _editor.DrawRackName = value, nameof(DrawRackName)); }
        }

        /// <summary>«Mostrar tarimas» (decision 10): a visual reference in the frontal only; it does not go into the BOM.</summary>
        public bool DrawPallets
        {
            get => _editor.DrawPallets;
            set { if (_editor.DrawPallets != value) SetFlag(() => _editor.DrawPallets = value, nameof(DrawPallets)); }
        }

        /// <summary>«Mostrar cotas en: Frontal», with the policy of the classic editor (ADR-0035).</summary>
        public bool DimensionsFrontal
        {
            get => _editor.ShowsDimensions(DimensionViewKind.Frontal);
            set { if (DimensionsFrontal != value) SetViews(value, DimensionsLateral, DimensionsPlanta, nameof(DimensionsFrontal)); }
        }

        public bool DimensionsLateral
        {
            get => _editor.ShowsDimensions(DimensionViewKind.Lateral);
            set { if (DimensionsLateral != value) SetViews(DimensionsFrontal, value, DimensionsPlanta, nameof(DimensionsLateral)); }
        }

        public bool DimensionsPlanta
        {
            get => _editor.ShowsDimensions(DimensionViewKind.Planta);
            set { if (DimensionsPlanta != value) SetViews(DimensionsFrontal, DimensionsLateral, value, nameof(DimensionsPlanta)); }
        }

        // ------------------------------------------------------------------ «Medio frente» (decision 7: in the inspector, with no dialog)

        /// <summary>The one front of the selection whose tramos the inspector writes; null when the selection touches several fronts.</summary>
        public int? SegmentFront => SelectedFronts.Count == 1 ? SelectedFronts[0] : (int?)null;

        public IReadOnlyList<SelectiveSegmentRow> SegmentRows => _segmentRows;

        public bool HasSegmentRows => _segmentRows.Count > 0;

        public bool CanEditSegments => CanEdit && SegmentFront.HasValue;

        public bool CanStartSegments => CanEditSegments && _segmentRows.Count == 0;

        public bool CanChangeSegments => CanEditSegments && _segmentRows.Count > 0;

        public string SegmentError => _segmentError;

        public string SegmentNote
        {
            get
            {
                if (!SegmentFront.HasValue) return "Elige un solo frente para partirlo en tramos.";
                var front = SegmentFront.Value;
                var width = _editor.FrontWidth(front);
                var widthText = width > 0.0 ? " de ≈ " + Num(width) + " in" : string.Empty;
                var segments = _editor.ScopeFondos.Select(k => _editor.Front(k, front)).Where(f => f != null).Select(f => SegmentsKey(f.Segments)).Distinct().ToList();
                if (segments.Count > 1) return "Los fondos de la vista no tienen los mismos tramos en F" + (front + 1) + ": aplicar aquí los iguala.";
                if (_segmentRows.Count == 0) return "F" + (front + 1) + " es un frente de un solo tramo" + widthText + ".";
                return "F" + (front + 1) + widthText + " en " + _segmentRows.Count.ToString(CultureInfo.InvariantCulture)
                       + " tramos. El último se calcula: lo que sobra del frente. Se escribe con «Aplicar tramos».";
            }
        }

        /// <summary>«Partir en tramos»: the classic seed, a loaded tramo with its length to write and the calculated rest.</summary>
        public void StartSegments()
        {
            if (!CanStartSegments) return;
            _segmentRows = new List<SelectiveSegmentRow>
            {
                new SelectiveSegmentRow(0, false, string.Empty, true),
                new SelectiveSegmentRow(1, true, string.Empty, false),
            };
            _segmentError = string.Empty;
            RaiseSegments();
        }

        /// <summary>«+ Agregar tramo»: one more loaded tramo at the end, as the classic dialog adds it.</summary>
        public void AddSegment()
        {
            if (!CanChangeSegments) return;
            var rows = PendingSegments();
            rows.Add(new SelectiveSegment { Length = 0.0, Loaded = true });
            _segmentRows = RowsOf(rows);
            RaiseSegments();
        }

        /// <summary>«Quitar» of a tramo of the inspector (it is written with «Aplicar tramos»).</summary>
        public void RemoveSegment(int index)
        {
            if (!CanChangeSegments || index < 0 || index >= _segmentRows.Count) return;
            var rows = PendingSegments();
            rows.RemoveAt(index);
            _segmentRows = RowsOf(rows);
            RaiseSegments();
        }

        /// <summary>«Aplicar tramos»: the tramos of the inspector go to the front in the fondos of the scope, with the checks of the classic dialog.</summary>
        public void ApplySegments()
        {
            if (!CanEditSegments) return;
            var front = SegmentFront.Value;
            var segments = new List<SelectiveSegment>();
            for (var i = 0; i < _segmentRows.Count; i++)
            {
                var row = _segmentRows[i];
                var length = 0.0;
                if (!row.IsRest && !string.IsNullOrWhiteSpace(row.LengthText) && (!UiSupport.TryNum(row.LengthText, out length) || length <= 0.0))
                {
                    SetSegmentError("La longitud del tramo " + (i + 1).ToString(CultureInfo.InvariantCulture) + " debe ser un número mayor que 0.");
                    return;
                }

                segments.Add(new SelectiveSegment { Length = row.IsRest ? 0.0 : length, Loaded = row.Loaded });
            }

            var error = Apply(() => _editor.SetSegments(front, segments));
            if (error != null)
            {
                SetSegmentError(error);
                return;
            }

            _segmentKey = null;
            SetSegmentError(string.Empty);
            RebuildSegments();
        }

        /// <summary>«Sin medio frente»: the front goes back to one tramo in the fondos of the scope.</summary>
        public void ClearSegments()
        {
            if (!CanEditSegments) return;
            var front = SegmentFront.Value;
            var error = Apply(() => _editor.SetSegments(front, Array.Empty<SelectiveSegment>()));
            if (error != null)
            {
                SetSegmentError(error);
                return;
            }

            _segmentKey = null;
            SetSegmentError(string.Empty);
            RebuildSegments();
        }

        // ------------------------------------------------------------------ the selection of the matrix (the scope of the cells)

        /// <summary>A click on a cell: one cell; with Ctrl, add or remove it; with Mayús, the rectangle from the anchor (the last plain click).</summary>
        public void ClickCell(int frontIndex, int levelIndex, bool control, bool shift)
        {
            var clicked = new SelectiveMatrixPosition(frontIndex, levelIndex);
            if (!Exists(frontIndex, levelIndex)) return;

            if (shift)
            {
                var from = Exists(_anchor.FrontIndex, _anchor.LevelIndex) ? _anchor : _primary;
                _selection.Clear();
                for (var f = Math.Min(from.FrontIndex, frontIndex); f <= Math.Max(from.FrontIndex, frontIndex); f++)
                    for (var l = Math.Min(from.LevelIndex, levelIndex); l <= Math.Max(from.LevelIndex, levelIndex); l++)
                        Add(f, l);
                Add(frontIndex, levelIndex);
                _primary = clicked;
            }
            else if (control)
            {
                if (_selection.Contains(clicked))
                {
                    // The selection is never empty.
                    if (_selection.Count > 1)
                    {
                        _selection.Remove(clicked);
                        if (_primary == clicked) _primary = _selection[_selection.Count - 1];
                    }
                }
                else
                {
                    Add(frontIndex, levelIndex);
                    _primary = clicked;
                }

                _anchor = clicked;
            }
            else
            {
                _selection.Clear();
                Add(frontIndex, levelIndex);
                _primary = clicked;
                _anchor = clicked;
            }

            RebuildSelection();
        }

        /// <summary>«F1»…: every cell of that front in the fondos of the scope.</summary>
        public void SelectFront(int frontIndex)
        {
            var levels = _editor.ScopeLevelCount(frontIndex);
            if (levels == 0) return;
            _selection.Clear();
            for (var l = 0; l < levels; l++) Add(frontIndex, l);
            _primary = _anchor = new SelectiveMatrixPosition(frontIndex, 0);
            RebuildSelection();
        }

        /// <summary>«N1»…: that level in every front that has it.</summary>
        public void SelectLevel(int levelIndex)
        {
            var fronts = Enumerable.Range(0, _editor.ScopeFrontCount).Where(f => Exists(f, levelIndex)).ToList();
            if (fronts.Count == 0) return;
            _selection.Clear();
            foreach (var f in fronts) Add(f, levelIndex);
            _primary = _anchor = new SelectiveMatrixPosition(fronts[0], levelIndex);
            RebuildSelection();
        }

        /// <summary>«Todo»: every cell of the fondos of the scope.</summary>
        public void SelectAll()
        {
            _selection.Clear();
            for (var f = 0; f < _editor.ScopeFrontCount; f++)
                for (var l = 0; l < _editor.ScopeLevelCount(f); l++)
                    Add(f, l);
            if (_selection.Count == 0) return;
            _primary = _anchor = _selection[0];
            RebuildSelection();
        }

        // ------------------------------------------------------------------ the gestures of the rack

        /// <summary>«Descartar»: the draft goes back to what was last read from the drawing or written to it in this editor.</summary>
        public void Discard()
        {
            if (!CanEdit)
            {
                Say(BlockedEditText, error: true);
                return;
            }

            SelectivePalletDesign baseline;
            try
            {
                baseline = _baselineJson == null ? null : Store.Deserialize(_baselineJson)?.ToDomain();
            }
            catch (Exception ex)
            {
                Say("No se pudo volver a lo leído del dibujo: " + ex.Message + " El borrador sigue como está.", error: true);
                return;
            }

            if (baseline != null && !_editor.LoadFrom(baseline, _openedStates, _baselineName, _variableNames))
            {
                _loadError = string.IsNullOrWhiteSpace(_editor.LastError) ? "sin detalle." : _editor.LastError;
                Say(LoadFailedText, error: true);
                Rebuild();
                return;
            }

            TakeBaseline();
            _confirmingClose = false;
            _segmentKey = null;
            Say("Borrador descartado: se muestra lo último que se leyó del dibujo o se escribió en él desde este editor.", error: false);
            Rebuild();
            OnPropertyChanged(nameof(IsConfirmingClose));
        }

        /// <summary>«Actualizar»: hands this editor to the host, which writes the drawing. Nothing happens when there is nothing to write.</summary>
        public void RequestWrite()
        {
            if (!CanUpdate)
            {
                Say(UpdateBlockedText, error: true);
                return;
            }

            _update(this);
        }

        /// <summary>«Cerrar el editor»: closes it at once when the draft has nothing to lose, and asks first when it has changes.</summary>
        public void RequestClose()
        {
            if (_writing)
            {
                Say(WritingText, error: true);
                return;
            }

            if (_dirty)
            {
                SetConfirmingClose(true);
                return;
            }

            CloseRequested?.Invoke(this);
        }

        /// <summary>«Descartar y cerrar» of the inline question.</summary>
        public void ConfirmClose()
        {
            if (!_confirmingClose || _writing) return;
            SetConfirmingClose(false);
            CloseRequested?.Invoke(this);
        }

        /// <summary>«Cancelar» of the inline question: the editor and its draft stay.</summary>
        public void CancelClose() => SetConfirmingClose(false);

        /// <summary>The host handed this draft to AutoCAD: nothing can be edited until it answers or the window stops waiting.</summary>
        public void BeginWriting()
        {
            if (_writing) return;
            _writing = true;
            SetConfirmingClose(false);
            Say("Escribiendo el rack en el dibujo…", error: false);
            Rebuild();
        }

        /// <summary>The answer arrived, or the window stopped waiting: the editor can be edited again.</summary>
        public void EndWriting()
        {
            if (!_writing) return;
            _writing = false;
            Rebuild();
        }

        /// <summary>
        /// The host wrote what was sent: that design and that name are the drawing now, and «Descartar» goes back to them. The draft stays
        /// «Modificado» only if it differs from what was written.
        /// </summary>
        public void MarkWritten(string message, string writtenJson, string writtenName)
        {
            _baselineJson = writtenJson;
            _baselineName = (writtenName ?? string.Empty).Trim();
            Say(message, error: false);
            UpdateDirty();
            RaiseState();
        }

        /// <summary>What the host or the view needs to say.</summary>
        public void Say(string text, bool error)
        {
            _message = text ?? string.Empty;
            _messageIsError = error && _message.Length > 0;
            OnPropertyChanged(nameof(Message));
            OnPropertyChanged(nameof(MessageIsError));
        }

        // ------------------------------------------------------------------ internals: the state

        private string BlockedEditText => _loadError != null ? LoadFailedText : WritingText;

        private string DesignJson() => _editor.Design == null ? null : Store.Serialize(SelectivePalletDesignDocument.From(_editor.Design, null, null));

        private void TakeBaseline()
        {
            _baselineJson = DesignJson();
            _baselineName = (_editor.Name ?? string.Empty).Trim();
            _dirty = false;
            RaiseState();
        }

        // «Modificado» is a difference of the design (its JSON) or of the name with what the drawing has (as last read or written), never a
        // gesture: leaving a field, a value refused, or a change undone leave the draft clean.
        private void UpdateDirty()
        {
            var dirty = !string.Equals(DesignJson(), _baselineJson, StringComparison.Ordinal)
                        || !string.Equals((_editor.Name ?? string.Empty).Trim(), _baselineName, StringComparison.Ordinal);
            if (dirty == _dirty) return;
            _dirty = dirty;
            if (!dirty) SetConfirmingClose(false);
            RaiseState();
        }

        private void SetConfirmingClose(bool value)
        {
            if (_confirmingClose == value) return;
            _confirmingClose = value;
            OnPropertyChanged(nameof(IsConfirmingClose));
        }

        // Every change of the draft: the write of the draft (which refuses a value it cannot take, writing nothing), then the recomposition.
        // A change that cannot be recomposed goes back to the last valid recomposition (I-09 of Dinámico) with the selection as it was, and
        // its reason goes back to the field. What the write omitted, and what the reduction of fronts or fondos lost, is said (S-09).
        private string Apply(Func<SelectiveDraftEdit> change, string reduces = null)
        {
            if (!CanEdit)
            {
                Say(BlockedEditText, error: true);
                return BlockedEditText;
            }

            var keptBefore = reduces == null ? default : Kept();
            var selection = (Cells: _selection.ToList(), Primary: _primary, Anchor: _anchor);
            var edit = change() ?? Done;
            if (edit.Refused) return edit.Error;

            if (!_editor.Recompose())
            {
                var reason = _editor.LastError;
                _editor.RestoreLastValid();
                _selection.Clear();
                _selection.AddRange(selection.Cells);
                _primary = selection.Primary;
                _anchor = selection.Anchor;
                Say(reason, error: true);
                Rebuild();
                UpdateDirty();
                return reason;
            }

            var notices = new List<string>();
            if (!string.IsNullOrWhiteSpace(edit.Notice)) notices.Add(edit.Notice);
            if (reduces != null)
            {
                var lost = LossText(keptBefore, Kept(), reduces);
                if (lost != null) notices.Add(lost);
            }

            Say(string.Join(" ", notices), error: false);
            Rebuild();
            UpdateDirty();
            return null;
        }

        // The custom cabeceras and the own peraltes of the posts the draft holds now.
        private (int Cabeceras, int Peraltes) Kept()
        {
            var cabeceras = 0;
            for (var k = 0; k < _editor.FondoCount; k++)
                for (var p = 0; p < _editor.PostCount; p++)
                    if (_editor.HasCustomCabecera(k, p)) cabeceras++;
            var peraltes = Enumerable.Range(0, _editor.PostCount).Count(p => _editor.PostPeralteOverride(p).HasValue);
            return (cabeceras, peraltes);
        }

        // S-09: reducing the fronts or the fondos drops the custom cabeceras and the own peraltes of the posts that disappear, as the
        // classic editor does, which says nothing; the window says it after the write.
        private static string LossText((int Cabeceras, int Peraltes) before, (int Cabeceras, int Peraltes) after, string what)
        {
            var cabeceras = Math.Max(0, before.Cabeceras - after.Cabeceras);
            var peraltes = Math.Max(0, before.Peraltes - after.Peraltes);
            if (cabeceras == 0 && peraltes == 0) return null;
            var parts = new List<string>();
            if (cabeceras > 0) parts.Add(Count(cabeceras, "cabecera personalizada", "cabeceras personalizadas"));
            if (peraltes > 0) parts.Add(Count(peraltes, "peralte propio", "peraltes propios"));
            return "Al reducir " + what + " se quitaron " + string.Join(" y ", parts) + " de los postes que ya no existen, como en el editor clásico. "
                   + "Volver a subirlos no los recupera; «Descartar» vuelve a lo último que se leyó o se escribió.";
        }

        private void SetFlag(Action change, string propertyName)
        {
            var error = Apply(() =>
            {
                change();
                return Done;
            });
            if (error != null) Say(error, error: true);
            OnPropertyChanged(propertyName);
        }

        private void SetViews(bool frontal, bool lateral, bool planta, string propertyName)
        {
            SetFlag(() => _editor.SetDimensionViews(frontal, lateral, planta), propertyName);
            OnPropertyChanged(nameof(DimensionsFrontal));
            OnPropertyChanged(nameof(DimensionsLateral));
            OnPropertyChanged(nameof(DimensionsPlanta));
        }

        private void RaiseState()
        {
            OnPropertyChanged(nameof(IsDirty));
            OnPropertyChanged(nameof(IsWriting));
            OnPropertyChanged(nameof(IsLoadFailed));
            OnPropertyChanged(nameof(CanEdit));
            OnPropertyChanged(nameof(CanClose));
            OnPropertyChanged(nameof(IsNameReadOnly));
            OnPropertyChanged(nameof(CanDiscard));
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(CanUpdate));
            OnPropertyChanged(nameof(UpdateBlockedText));
            OnPropertyChanged(nameof(CanEditSegments));
            OnPropertyChanged(nameof(CanStartSegments));
            OnPropertyChanged(nameof(CanChangeSegments));
        }

        // ------------------------------------------------------------------ internals: the lists, refreshed in place

        // The lists keep their items: a recomposition refreshes them in place, so the focus, the box being written and the button under the
        // mouse survive it (I-05 of Dinámico). Only a list whose fields change is replaced.
        private void Rebuild()
        {
            Merge(ref _essentials, BuildEssentials(), nameof(Essentials));
            MergeFields(ref _separatorFields, BuildSeparatorFields(), nameof(SeparatorFields));
            Merge(ref _annotationFields, BuildAnnotationFields(), nameof(AnnotationFields));
            MergePosts(BuildPostRows());
            _calculated = BuildCalculated();
            _halfFronts = BuildHalfFronts();
            OnPropertyChanged(nameof(Calculated));
            OnPropertyChanged(nameof(HalfFronts));
            OnPropertyChanged(nameof(HalfFrontsText));

            var options = _editor.FondoCount > 1
                ? new[] { "Todos los fondos" }.Concat(Enumerable.Range(1, _editor.FondoCount).Select(k => "Fondo " + k.ToString(CultureInfo.InvariantCulture))).ToList()
                : new List<string>();
            if (!options.SequenceEqual(_fondoOptions))
            {
                _fondoOptions = options;
                OnPropertyChanged(nameof(FondoOptions));
            }

            OnPropertyChanged(nameof(HasFondos));
            OnPropertyChanged(nameof(FondoScopeIndex));
            OnPropertyChanged(nameof(FondoScopeText));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(DrawBasePlate));
            OnPropertyChanged(nameof(NumberFronts));
            OnPropertyChanged(nameof(NumberLevels));
            OnPropertyChanged(nameof(DrawRackName));
            OnPropertyChanged(nameof(DrawPallets));
            OnPropertyChanged(nameof(DimensionsFrontal));
            OnPropertyChanged(nameof(DimensionsLateral));
            OnPropertyChanged(nameof(DimensionsPlanta));
            RebuildSelection();
            RaiseState();
        }

        private void RebuildSelection()
        {
            CleanSelection();
            var fronts = _editor.ScopeFrontCount;
            var levels = Math.Max(1, Enumerable.Range(0, fronts).Select(_editor.ScopeLevelCount).DefaultIfEmpty(1).Max());

            // The shape (fronts × levels) changed: new rows and columns. Otherwise the same items are refreshed in place.
            if (_columns.Count != fronts || _rows.Count != levels)
            {
                _columns = Enumerable.Range(0, fronts).Select(f => new SelectiveColumnHead(f)).ToList();
                _rows = Enumerable.Range(0, levels).Reverse()
                    .Select(l => new SelectiveMatrixRow(l, "N" + (l + 1), Enumerable.Range(0, fronts).Select(f => new SelectiveCellItem(f, l)).ToList()))
                    .ToList();
                OnPropertyChanged(nameof(Columns));
                OnPropertyChanged(nameof(Rows));
            }

            var scope = _editor.ScopeFondos;
            foreach (var column in _columns)
            {
                var f = column.FrontIndex;
                var holders = scope.Where(k => f < _editor.FrontCount(k)).ToList();
                var half = holders.Any(k => _editor.Front(k, f)?.IsSplit == true);
                var partial = scope.Count > 1 && holders.Count < scope.Count;
                var accessible = "Frente " + (f + 1) + (partial ? ", solo en " + FondoList(holders) : string.Empty)
                                 + (half ? ", con medio frente" : string.Empty) + ": seleccionar la columna";
                column.Set("F" + (f + 1) + (half ? " ½" : string.Empty), partial, accessible);
            }

            foreach (var row in _rows)
            {
                foreach (var item in row.Cells)
                {
                    var f = item.FrontIndex;
                    var l = item.LevelIndex;
                    if (!Exists(f, l))
                    {
                        item.Set("—", string.Empty, false, false, false, false, false, false,
                            "F" + (f + 1) + " · N" + (l + 1) + ": no existe en los fondos de la vista");
                        continue;
                    }

                    var at = new[] { new SelectiveMatrixPosition(f, l) };
                    var frente = _editor.ReadCells(at, c => c.Frente);
                    var alto = _editor.ReadCells(at, c => c.Alto);
                    var count = _editor.ReadCells(at, c => c.PalletCount);
                    var peralte = _editor.ReadCells(at, c => c.BeamPeralte);
                    var varies = frente.Varies || alto.Varies;
                    var onFloor = IsOnFloor(f, l);
                    var text = varies ? "varía" : Num(frente.Value) + "×" + Num(alto.Value);
                    var detail = "×" + (count.Varies ? "?" : count.Value.ToString(CultureInfo.InvariantCulture))
                                 + (onFloor ? " · piso" : " · P" + (peralte.Varies ? "?" : Num(peralte.Value)));
                    var selected = _selection.Contains(at[0]);
                    var accessible = "F" + (f + 1) + " · N" + (l + 1) + ": " + (varies ? "la tarima varía entre los fondos" : text)
                                     + (onFloor ? ", en el piso, sin larguero" : string.Empty);
                    item.Set(text, detail, _showDetail, true, selected, selected && _primary == at[0], onFloor, varies, accessible);
                }
            }

            Merge(ref _cellFields, BuildCellFields(), nameof(CellFields));
            MergeFields(ref _frontFields, BuildFrontFields(), nameof(FrontFields));
            OnPropertyChanged(nameof(SelectionFloorBeam));
            OnPropertyChanged(nameof(SelectedPositions));
            OnPropertyChanged(nameof(SelectedFronts));
            OnPropertyChanged(nameof(PrimaryPosition));
            OnPropertyChanged(nameof(InspectorTitle));
            OnPropertyChanged(nameof(InspectorNote));
            OnSafetyChanged(); // «Extremos y seguridad» (entrega 2): T/P of the selection and the table by post
            RebuildSegments();
        }

        // The tramos of the inspector follow the front and the scope; while they are being written (not applied yet) a recomposition of
        // another value keeps them.
        private void RebuildSegments()
        {
            var front = SegmentFront;
            var key = front.HasValue ? (front.Value, _editor.FondoScope) : ((int, int?)?)null;
            if (key != _segmentKey)
            {
                _segmentKey = key;
                _segmentRows = front.HasValue ? RowsOf(StoredSegments(front.Value)) : new List<SelectiveSegmentRow>();
                _segmentError = string.Empty;
            }

            RaiseSegments();
        }

        private void RaiseSegments()
        {
            OnPropertyChanged(nameof(SegmentFront));
            OnPropertyChanged(nameof(SegmentRows));
            OnPropertyChanged(nameof(HasSegmentRows));
            OnPropertyChanged(nameof(CanEditSegments));
            OnPropertyChanged(nameof(CanStartSegments));
            OnPropertyChanged(nameof(CanChangeSegments));
            OnPropertyChanged(nameof(SegmentNote));
            OnPropertyChanged(nameof(SegmentError));
        }

        private void SetSegmentError(string text)
        {
            _segmentError = text ?? string.Empty;
            OnPropertyChanged(nameof(SegmentError));
        }

        // The tramos of the front in the first fondo of the scope that has it (when the fondos differ, the note says so).
        private IReadOnlyList<SelectiveSegment> StoredSegments(int front)
            => _editor.ScopeFondos.Select(k => _editor.Front(k, front)).FirstOrDefault(f => f != null)?.Segments ?? Array.Empty<SelectiveSegment>();

        private List<SelectiveSegment> PendingSegments()
        {
            var list = new List<SelectiveSegment>();
            foreach (var row in _segmentRows)
            {
                var length = !row.IsRest && UiSupport.TryNum(row.LengthText, out var value) && value > 0.0 ? value : 0.0;
                list.Add(new SelectiveSegment { Length = length, Loaded = row.Loaded });
            }

            return list;
        }

        private static List<SelectiveSegmentRow> RowsOf(IReadOnlyList<SelectiveSegment> segments)
        {
            var rows = new List<SelectiveSegmentRow>();
            for (var i = 0; i < segments.Count; i++)
            {
                var rest = i == segments.Count - 1;
                rows.Add(new SelectiveSegmentRow(i, rest, rest || !(segments[i].Length > 0.0) ? string.Empty : Num(segments[i].Length), segments[i].Loaded));
            }

            return rows;
        }

        private static string SegmentsKey(IReadOnlyList<SelectiveSegment> segments)
            => string.Join(";", (segments ?? Array.Empty<SelectiveSegment>()).Select(s => s.Length.ToString("R", CultureInfo.InvariantCulture) + (s.Loaded ? "+" : "-")));

        private static string KeyOf(object item) => item is DynamicFieldItem field ? field.Key : (item as SelectiveChoiceItem)?.Key;

        private void Merge(ref IReadOnlyList<object> current, IReadOnlyList<object> next, string propertyName)
        {
            var same = current.Count == next.Count;
            for (var i = 0; same && i < current.Count; i++)
                same = current[i].GetType() == next[i].GetType() && string.Equals(KeyOf(current[i]), KeyOf(next[i]), StringComparison.Ordinal);

            if (same)
            {
                for (var i = 0; i < current.Count; i++)
                {
                    if (current[i] is DynamicFieldItem field) field.Refresh((DynamicFieldItem)next[i]);
                    else ((SelectiveChoiceItem)current[i]).Refresh((SelectiveChoiceItem)next[i]);
                }

                return;
            }

            current = next;
            OnPropertyChanged(propertyName);
        }

        private void MergeFields(ref IReadOnlyList<DynamicFieldItem> current, IReadOnlyList<DynamicFieldItem> next, string propertyName)
        {
            if (current.Count == next.Count && current.Select(f => f.Key).SequenceEqual(next.Select(f => f.Key), StringComparer.Ordinal))
            {
                for (var i = 0; i < current.Count; i++) current[i].Refresh(next[i]);
                return;
            }

            current = next;
            OnPropertyChanged(propertyName);
        }

        private void MergePosts(IReadOnlyList<SelectivePostRow> next)
        {
            if (_postRows.Count == next.Count)
            {
                for (var i = 0; i < next.Count; i++) _postRows[i].Refresh(next[i]);
                return;
            }

            _postRows = next;
            OnPropertyChanged(nameof(PostRows));
        }

        // ------------------------------------------------------------------ internals: the fields

        // «Lo esencial» (the rack and the fondos of the scope). The holguras governed by a project variable or a formula are read only, with
        // their effective value and the reason, and with no «=» (decision 12).
        private IReadOnlyList<object> BuildEssentials()
        {
            var fronts = _editor.ReadFondos(f => f.FrontCount);
            var depth = _editor.ReadFondos(f => f.PalletDepth);
            var cabecera = _editor.ReadFondos(f => f.CabeceraDepthOverride);
            return new object[]
            {
                new SelectiveChoiceItem("post", "Poste", _editor.PostChoices.Select(p => new SelectiveChoiceOption(p.Id.Trim(), Label(p.Label, p.Id))).ToList(),
                    _editor.PostId?.Trim(), string.Empty, value => Apply(() =>
                    {
                        _editor.PostId = value;
                        return Done;
                    }), !CanEdit),
                Number("peralte", "Peralte de poste", "in", _editor.PostPeralte, v => v > 0.0, "El peralte de poste debe ser un número mayor que cero.", v =>
                {
                    _editor.PostPeralte = v;
                    return Done;
                }),
                ScopedInt("fronts", "Frentes", fronts, 1, "La cantidad de frentes debe ser un entero mayor o igual que 1.", v => _editor.SetFrontCount(v), "los frentes"),
                ScopedNumber("depth", "Profundidad de tarima", "in", depth, v => v > 0.0, "La profundidad de tarima debe ser un número mayor que cero.", v => _editor.SetPalletDepth(v)),
                ScopedOptional("cabecera", "Profundidad de cabecera (vacía = tarima − 6 in)", "in", cabecera,
                    "La profundidad de cabecera debe ser un número mayor que cero, o vacía.", v => _editor.SetCabeceraDepth(v)),
                new DynamicFieldItem("fondos", "Número de fondos", string.Empty, _editor.FondoCount.ToString(CultureInfo.InvariantCulture), string.Empty, text =>
                {
                    if (!UiSupport.TryInt(text, out var count) || count < 1 || count > SelectiveRackDefaults.MaxDepthCount)
                        return "El número de fondos debe ser un entero de 1 a " + SelectiveRackDefaults.MaxDepthCount.ToString(CultureInfo.InvariantCulture) + ".";
                    return Apply(() => _editor.SetFondoCount(count), "los fondos");
                }, !CanEdit),
                Holgura("tolerance", "Tolerancia horizontal", Tolerance, _editor.PalletTolerance, v => _editor.PalletTolerance = v),
                Holgura("clearance", "Holgura vertical", Clearance, _editor.VerticalClearance, v => _editor.VerticalClearance = v),
            };
        }

        private IReadOnlyList<DynamicFieldItem> BuildSeparatorFields()
        {
            var separators = _editor.Separators;
            return Enumerable.Range(0, separators.Count).Select(gap => Number(
                "sep" + gap.ToString(CultureInfo.InvariantCulture),
                "Separación fondo " + (gap + 1) + "–" + (gap + 2),
                "in",
                separators[gap],
                v => v > 0.0,
                "La separación debe ser un número mayor que cero.",
                v => _editor.SetSeparator(gap, v))).ToList();
        }

        private IReadOnlyList<object> BuildAnnotationFields() => new object[]
        {
            Number("scale", "Escala de texto (1 = 6\")", string.Empty, _editor.AnnotationScale, v => v > 0.0, "La escala de texto debe ser mayor que cero.", v =>
            {
                _editor.AnnotationScale = v;
                return Done;
            }),
            new SelectiveChoiceItem("dims", "Cotas", DimensionDetails.Select(d => new SelectiveChoiceOption(d.Detail.ToString(), d.Label)).ToList(),
                _editor.Dimensions.ToString(), string.Empty, value =>
                {
                    if (!Enum.TryParse<DimensionDetail>(value, out var detail)) return "Elige el detalle de las cotas.";
                    return Apply(() =>
                    {
                        _editor.Dimensions = detail;
                        return Done;
                    });
                }, !CanEdit),
            new DynamicFieldItem("dimStyle", "Estilo de cota (vacío = automático)", string.Empty, _editor.DimensionStyle?.Trim() ?? string.Empty, string.Empty, text => Apply(() =>
            {
                _editor.DimensionStyle = string.IsNullOrWhiteSpace(text) ? null : text.Trim();
                return Done;
            }), !CanEdit),
        };

        // The inspector of the cells: the pallet, the larguero and its peralte (only those the catalog allows for the larguero of every cell,
        // as the list of the classic editor), and the manual values.
        private IReadOnlyList<object> BuildCellFields()
        {
            var cells = _selection.ToList();
            var beam = _editor.ReadCells(cells, c => c.BeamId?.Trim(), StringComparer.OrdinalIgnoreCase);
            var beams = _editor.ScopeFondos
                .SelectMany(k => cells.Select(p => _editor.Cell(k, p.FrontIndex, p.LevelIndex)))
                .Where(c => c != null)
                .Select(c => c.BeamId?.Trim() ?? string.Empty)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            IEnumerable<string> allowed = null;
            foreach (var id in beams)
            {
                var peraltes = _editor.BeamPeraltes(id).Select(Num).ToList();
                allowed = allowed == null ? peraltes : allowed.Intersect(peraltes, StringComparer.Ordinal).ToList();
            }

            var peralteOptions = (allowed ?? Enumerable.Empty<string>()).Select(p => new SelectiveChoiceOption(p, p + " in")).ToList();
            var peralte = _editor.ReadCells(cells, c => c.BeamPeralte);
            var beamOptions = _editor.BeamChoices.Select(b => new SelectiveChoiceOption(b.Id.Trim(), Label(b.Label, b.Id))).ToList();
            var known = beam.HasValue ? beamOptions.FirstOrDefault(o => string.Equals(o.Value, beam.Value, StringComparison.OrdinalIgnoreCase))?.Value : null;

            return new object[]
            {
                CellNumber("cFront", "Ancho de tarima", "in", cells, c => c.Frente, (p, v) => _editor.SetPalletFront(p, v), "El ancho de tarima debe ser un número mayor que cero."),
                CellNumber("cHeight", "Alto de tarima", "in", cells, c => c.Alto, (p, v) => _editor.SetPalletHeight(p, v), "El alto de tarima debe ser un número mayor que cero."),
                CellInt("cCount", "Tarimas por nivel", cells),
                new SelectiveChoiceItem("cBeam", "Larguero", beamOptions, known, beam.Varies ? Varies : string.Empty,
                    value => Apply(() => _editor.SetBeam(cells, value)), !CanEdit || beam.Count == 0),
                new SelectiveChoiceItem("cPeralte", "Peralte del larguero", peralteOptions, peralte.HasValue ? Num(peralte.Value) : null,
                    peralte.Varies ? Varies : peralteOptions.Count == 0 ? "El catálogo no tiene un peralte común para estos largueros." : string.Empty,
                    value =>
                    {
                        if (!UiSupport.TryNum(value, out var chosen)) return "Elige un peralte del catálogo.";
                        return Apply(() => _editor.SetBeamPeralte(cells, chosen));
                    }, !CanEdit || peralte.Count == 0 || peralteOptions.Count == 0),
                CellOptional("cLength", "Largo manual del larguero (vacío = automático)", cells, c => c.BeamLength, (p, v) => _editor.SetBeamLengthOverride(p, v),
                    "El largo del larguero debe ser un número mayor que cero, o vacío para automático."),
                CellOptional("cClear", "Claro manual debajo (vacío = automático)", cells, c => c.Clear, (p, v) => _editor.SetClearOverride(p, v),
                    "El claro debe ser un número mayor que cero, o vacío para automático."),
            };
        }

        // The inspector of the fronts that the selection touches, in the fondos of the scope.
        private IReadOnlyList<DynamicFieldItem> BuildFrontFields()
        {
            var fronts = SelectedFronts;
            var levels = _editor.ReadFronts(fronts, f => f.LevelCount);
            var floor = _editor.ReadFronts(fronts, f => f.FloorBeam);
            var rise = _editor.ReadFronts(fronts, f => f.FloorBeamRise);
            var height = _editor.ReadFronts(fronts, f => f.HeightOverride);
            var anyFloor = floor.Varies || (floor.HasValue && floor.Value);
            return new[]
            {
                new DynamicFieldItem("fLevels", "Niveles", string.Empty, levels.HasValue ? levels.Value.ToString(CultureInfo.InvariantCulture) : string.Empty,
                    levels.Varies ? Varies : string.Empty, text =>
                    {
                        if (!UiSupport.TryInt(text, out var count) || count < 1) return "El número de niveles debe ser un entero mayor o igual que 1.";
                        return Apply(() => _editor.SetLevelCount(fronts, count));
                    }, !CanEdit || levels.Count == 0),
                new DynamicFieldItem("fRise", "Elevación del larguero a piso", "in", rise.HasValue ? Num(rise.Value) : string.Empty,
                    rise.Varies ? Varies : anyFloor ? string.Empty : "Solo con «Larguero a piso».", text =>
                    {
                        if (!UiSupport.TryNum(text, out var value) || value < 0.0) return "La elevación del larguero a piso debe ser un número mayor o igual que cero.";
                        return Apply(() => _editor.SetFloorBeamRise(fronts, value));
                    }, !CanEdit || rise.Count == 0 || !anyFloor),
                new DynamicFieldItem("fHeight", "Altura manual (vacía = calculada)", "in", height.HasValue && height.Value.HasValue ? Num(height.Value.Value) : string.Empty,
                    height.Varies ? Varies : string.Empty, text =>
                    {
                        if (string.IsNullOrWhiteSpace(text)) return Apply(() => _editor.SetHeightOverride(fronts, null));
                        if (!UiSupport.TryNum(text, out var value) || value <= 0.0) return "La altura manual debe ser un número mayor que cero, o vacía para calculada.";
                        return Apply(() => _editor.SetHeightOverride(fronts, value));
                    }, !CanEdit || height.Count == 0),
            };
        }

        // «Avanzado»: the own peralte of each post (empty = the one of the rack, the rule of the classic box) and its cabecera by fondo.
        private IReadOnlyList<SelectivePostRow> BuildPostRows()
        {
            var rows = new List<SelectivePostRow>();
            for (var p = 0; p < _editor.PostCount; p++)
            {
                var post = p;
                var own = _editor.PostPeralteOverride(post);
                var field = new DynamicFieldItem("post" + post.ToString(CultureInfo.InvariantCulture), "Peralte propio", "in", own.HasValue ? Num(own.Value) : string.Empty,
                    string.Empty, text =>
                    {
                        if (string.IsNullOrWhiteSpace(text)) return Apply(() => _editor.SetPostPeralte(post, null));
                        if (!UiSupport.TryNum(text, out var value) || value <= 0.0) return "Peralte de poste inválido (deja vacío para usar el peralte del rack).";
                        return Apply(() => _editor.SetPostPeralte(post, value));
                    }, !CanEdit);
                var cabeceras = Enumerable.Range(0, _editor.FondoCount)
                    .Where(k => post <= _editor.FrontCount(k))
                    .Select(k => (_editor.FondoCount > 1 ? "Fondo " + (k + 1) + ": " : string.Empty) + (_editor.HasCustomCabecera(k, post) ? "personalizada" : "calculada"));
                rows.Add(new SelectivePostRow(post, field, "Cabecera " + string.Join(" · ", cabeceras)));
            }

            return rows;
        }

        private IReadOnlyList<string> BuildHalfFronts()
        {
            var list = new List<string>();
            for (var k = 0; k < _editor.FondoCount; k++)
            {
                for (var f = 0; f < _editor.FrontCount(k); f++)
                {
                    var front = _editor.Front(k, f);
                    if (front == null || !front.IsSplit) continue;
                    list.Add((_editor.FondoCount > 1 ? "Fondo " + (k + 1) + " · " : string.Empty) + "F" + (f + 1) + ": "
                             + Count(front.Segments.Count, "tramo", "tramos"));
                }
            }

            return list;
        }

        // «Calculado»: what the last valid recomposition resolved, read only (the heights by post as the drawing gives them, with the custom
        // cabeceras; the larguero of each front; the length between the axes of the end posts; the depths).
        private IReadOnlyList<SelectiveCalcItem> BuildCalculated()
        {
            var system = _editor.System;
            if (system == null) return new[] { new SelectiveCalcItem("Calculado", "—") };
            var items = new List<SelectiveCalcItem>();
            var fondos = SelectiveDepthLayout.Count(system);
            try
            {
                items.Add(new SelectiveCalcItem("Altura de cabecera (la más alta)", Num(system.Height) + " in"));
                for (var k = 0; k < fondos; k++)
                {
                    var bays = SelectiveDepthLayout.BaysOfFondo(system, k) ?? new List<SelectiveBay>();
                    var row = k == 0 ? system.PostCabeceras : k - 1 < system.ExtraFondoPostCabeceras.Count ? system.ExtraFondoPostCabeceras[k - 1] : null;
                    var fallback = SelectivePostGeometry.FallbackHeight(bays, system.Height);
                    var posts = Enumerable.Range(0, bays.Count + 1).Select(i =>
                    {
                        var custom = row != null && i < row.Count ? row[i] : null;
                        var h = custom != null && custom.Height > 0.0 ? custom.Height : SelectivePostGeometry.PostHeight(bays, i, fallback);
                        return "P" + (i + 1) + " " + Num(h);
                    });
                    items.Add(new SelectiveCalcItem("Altura por poste" + (fondos > 1 ? ", fondo " + (k + 1) : string.Empty), string.Join(" · ", posts) + " in"));
                }

                var master = SelectiveDepthLayout.BaysOfFondo(system, SelectiveDepthLayout.MasterFondoIndex(system)) ?? new List<SelectiveBay>();
                items.Add(new SelectiveCalcItem("Largo del larguero por frente",
                    master.Count == 0 ? "—" : string.Join(" · ", master.Select((b, i) => "F" + (i + 1) + " " + Num(b.BeamLength))) + " in"));
                var grid = SelectiveDepthLayout.MasterGrid(system, _editor.Catalog).PostXs;
                items.Add(new SelectiveCalcItem("Largo entre ejes de los postes extremos", grid.Count < 2 ? "—" : Num(grid[grid.Count - 1] - grid[0]) + " in"));
                items.Add(new SelectiveCalcItem("Profundidad de cabecera",
                    string.Join(" · ", Enumerable.Range(0, fondos).Select(k => (fondos > 1 ? "fondo " + (k + 1) + " " : string.Empty) + Num(SelectiveDepthLayout.CabeceraDepthOfFondo(system, k)))) + " in"));
                if (fondos > 1) items.Add(new SelectiveCalcItem("Profundidad total del rack", Num(SelectiveDepthLayout.TotalFondoDepth(system)) + " in"));
            }
            catch (Exception ex)
            {
                items.Add(new SelectiveCalcItem("No se pudo calcular", ex.Message));
            }

            return items;
        }

        private DynamicFieldItem Number(string key, string label, string unit, double current, Func<double, bool> valid, string invalid, Func<double, SelectiveDraftEdit> set) =>
            new DynamicFieldItem(key, label, unit, Num(current), string.Empty, text =>
            {
                if (!UiSupport.TryNum(text, out var value) || !valid(value)) return invalid;
                return Apply(() => set(value));
            }, !CanEdit);

        private DynamicFieldItem ScopedNumber(string key, string label, string unit, SelectiveScopedValue<double> current, Func<double, bool> valid, string invalid,
            Func<double, SelectiveDraftEdit> set) =>
            new DynamicFieldItem(key, label, unit, current.HasValue ? Num(current.Value) : string.Empty, current.Varies ? Varies : string.Empty, text =>
            {
                if (!UiSupport.TryNum(text, out var value) || !valid(value)) return invalid;
                return Apply(() => set(value));
            }, !CanEdit || current.Count == 0);

        private DynamicFieldItem ScopedInt(string key, string label, SelectiveScopedValue<int> current, int minimum, string invalid, Func<int, SelectiveDraftEdit> set, string reduces) =>
            new DynamicFieldItem(key, label, string.Empty, current.HasValue ? current.Value.ToString(CultureInfo.InvariantCulture) : string.Empty,
                current.Varies ? Varies : string.Empty, text =>
                {
                    if (!UiSupport.TryInt(text, out var value) || value < minimum) return invalid;
                    return Apply(() => set(value), reduces);
                }, !CanEdit || current.Count == 0);

        private DynamicFieldItem ScopedOptional(string key, string label, string unit, SelectiveScopedValue<double?> current, string invalid, Func<double?, SelectiveDraftEdit> set) =>
            new DynamicFieldItem(key, label, unit, current.HasValue && current.Value.HasValue ? Num(current.Value.Value) : string.Empty,
                current.Varies ? Varies : string.Empty, text =>
                {
                    if (string.IsNullOrWhiteSpace(text)) return Apply(() => set(null));
                    if (!UiSupport.TryNum(text, out var value) || value <= 0.0) return invalid;
                    return Apply(() => set(value));
                }, !CanEdit || current.Count == 0);

        // A holgura: its value while it belongs to the rack; the effective value, read only, with the reason and the name of the variable while
        // a project variable or a formula governs it. No «=» anywhere (decision 12).
        private DynamicFieldItem Holgura(string key, string label, PropertyId property, double current, Action<double> set)
        {
            if (_editor.IsLinked(property))
                return new DynamicFieldItem(key, label, "in", Num(current), _editor.LinkedReadOnlyReason(property), null, isReadOnly: true);

            return Number(key, label, "in", current, v => v >= 0.0, label + ": debe ser un número mayor o igual que cero.", v =>
            {
                set(v);
                return Done;
            });
        }

        private DynamicFieldItem CellNumber(string key, string label, string unit, IReadOnlyList<SelectiveMatrixPosition> cells, Func<SelectiveEditorCell, double> read,
            Func<IReadOnlyList<SelectiveMatrixPosition>, double, SelectiveDraftEdit> write, string invalid)
        {
            var value = _editor.ReadCells(cells, read);
            return new DynamicFieldItem(key, label, unit, value.HasValue ? Num(value.Value) : string.Empty, value.Varies ? Varies : string.Empty, text =>
            {
                if (!UiSupport.TryNum(text, out var number) || number <= 0.0) return invalid;
                return Apply(() => write(cells, number));
            }, !CanEdit || value.Count == 0);
        }

        private DynamicFieldItem CellInt(string key, string label, IReadOnlyList<SelectiveMatrixPosition> cells)
        {
            var value = _editor.ReadCells(cells, c => c.PalletCount);
            return new DynamicFieldItem(key, label, string.Empty, value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : string.Empty,
                value.Varies ? Varies : string.Empty, text =>
                {
                    if (!UiSupport.TryInt(text, out var count) || count < 1) return "Las tarimas por nivel deben ser un entero mayor o igual que 1.";
                    return Apply(() => _editor.SetPalletCount(cells, count));
                }, !CanEdit || value.Count == 0);
        }

        private DynamicFieldItem CellOptional(string key, string label, IReadOnlyList<SelectiveMatrixPosition> cells, Func<SelectiveEditorCell, double?> read,
            Func<IReadOnlyList<SelectiveMatrixPosition>, double?, SelectiveDraftEdit> write, string invalid)
        {
            var value = _editor.ReadCells(cells, read);
            return new DynamicFieldItem(key, label, "in", value.HasValue && value.Value.HasValue ? Num(value.Value.Value) : string.Empty,
                value.Varies ? Varies : string.Empty, text =>
                {
                    if (string.IsNullOrWhiteSpace(text)) return Apply(() => write(cells, null));
                    if (!UiSupport.TryNum(text, out var number) || number <= 0.0) return invalid;
                    return Apply(() => write(cells, number));
                }, !CanEdit || value.Count == 0);
        }

        // ------------------------------------------------------------------ internals: the matrix

        private bool Exists(int frontIndex, int levelIndex)
            => frontIndex >= 0 && levelIndex >= 0 && _editor.ScopeFondos.Any(k => levelIndex < _editor.LevelCount(k, frontIndex));

        // N1 of a front that some fondo of the scope has with no «Larguero a piso»: its pallet is on the floor, with no larguero.
        private bool IsOnFloor(int frontIndex, int levelIndex)
            => levelIndex == 0 && _editor.ScopeFondos.Select(k => _editor.Front(k, frontIndex)).Any(f => f != null && f.LevelCount > 0 && !f.FloorBeam);

        private void Add(int frontIndex, int levelIndex)
        {
            var position = new SelectiveMatrixPosition(frontIndex, levelIndex);
            if (!Exists(frontIndex, levelIndex) || _selection.Contains(position)) return;
            _selection.Add(position);
            _selection.Sort();
        }

        // The selection keeps only cells the fondos of the scope have (a change of the scope or of the shape), and is never empty.
        private void CleanSelection()
        {
            _selection.RemoveAll(p => !Exists(p.FrontIndex, p.LevelIndex));
            if (_selection.Count == 0)
            {
                var first = Enumerable.Range(0, _editor.ScopeFrontCount).Where(f => Exists(f, 0)).Select(f => new SelectiveMatrixPosition(f, 0)).FirstOrDefault();
                _selection.Add(first);
            }

            if (!_selection.Contains(_primary)) _primary = _selection[_selection.Count - 1];
        }

        private static string FondoList(IReadOnlyList<int> fondos)
        {
            if (fondos.Count == 0) return "ningún fondo";
            var names = fondos.Select(k => (k + 1).ToString(CultureInfo.InvariantCulture)).ToList();
            return names.Count == 1 ? "el fondo " + names[0] : "los fondos " + string.Join(", ", names.Take(names.Count - 1)) + " y " + names[names.Count - 1];
        }

        private static string Label(string label, string id) => string.IsNullOrWhiteSpace(label) ? id?.Trim() ?? string.Empty : label.Trim();

        private static string Count(int count, string one, string many) => count.ToString(CultureInfo.InvariantCulture) + " " + (count == 1 ? one : many);

        // The values are shown with up to three decimals, as the boxes of the classic editor; the draft keeps the full value (S-02).
        private static string Num(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
    }
}
