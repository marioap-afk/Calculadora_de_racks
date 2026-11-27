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
    /// larguero (decision 4), and T / P of the entrega 2 (in <c>SelectiveEditorViewModel.Safety.cs</c>). Refreshed in place.
    /// </summary>
    public sealed partial class SelectiveCellItem : ObservableObject
    {
        private string _text = string.Empty;
        private string _detail = string.Empty;
        private bool _showDetail;
        private bool _exists;
        private bool _isSelected;
        private bool _isPrimary;
        private bool _isOnFloor;
        private bool _varies;
        private bool _isModified;
        private bool _isDistinct;
        private bool _isManual;
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

        /// <summary>The cell differs from what was last read from the drawing or written to it (the legend: «Modificada en el borrador»).</summary>
        public bool IsModified { get => _isModified; private set => SetProperty(ref _isModified, value); }

        /// <summary>The cell is not like most cells of the view (the legend: «Distinta del resto»); a modified cell says modified instead.</summary>
        public bool IsDistinct { get => _isDistinct; private set => SetProperty(ref _isDistinct, value); }

        /// <summary>The cell has a manual larguero length or a manual clear (the legend: «M Valor manual»).</summary>
        public bool IsManual { get => _isManual; private set => SetProperty(ref _isManual, value); }

        /// <summary>What a screen reader says of the cell, and its name for UI Automation: its position first («Frente 2 · Nivel 3: 42×60»).</summary>
        public string AccessibleText { get => _accessibleText; private set => SetProperty(ref _accessibleText, value); }

        internal void Set(string text, string detail, bool showDetail, bool exists, bool isSelected, bool isPrimary, bool isOnFloor, bool varies, string accessible,
            bool isModified = false, bool isDistinct = false, bool isManual = false)
        {
            Text = text;
            Detail = detail;
            ShowDetail = showDetail && exists;
            Exists = exists;
            IsSelected = isSelected;
            IsPrimary = isPrimary;
            IsOnFloor = isOnFloor;
            Varies = varies;
            IsModified = isModified && exists;
            IsDistinct = isDistinct && exists && !isModified;
            IsManual = isManual && exists;
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

        /// <summary>The name of the head of the row for UI Automation (the approved draft): «Nivel 1, el piso: seleccionar la fila».</summary>
        public string AccessibleText => "Nivel " + (LevelIndex + 1).ToString(CultureInfo.InvariantCulture) + (LevelIndex == 0 ? ", el piso" : string.Empty) + ": seleccionar la fila";

        public IReadOnlyList<SelectiveCellItem> Cells { get; }
    }

    /// <summary>The scope of the inspector (the approved draft: «Celda · Frente · Nivel · Todo»).</summary>
    public enum SelectiveEditScope
    {
        Cell,
        Front,
        Level,
        All,
    }

    /// <summary>One button of a segmented choice of the editor (the fondos of the view, the scope of the inspector): its label and whether it is the chosen one.</summary>
    public sealed class SelectiveSegmentOption : ObservableObject
    {
        private string _label;
        private bool _isOn;

        public SelectiveSegmentOption(int value, string label, string group)
        {
            Value = value;
            _label = label ?? string.Empty;
            Group = group ?? string.Empty;
        }

        /// <summary>What it chooses: the index of the option (0 = «Todos los fondos», n = «Fondo n»), or the <see cref="SelectiveEditScope"/>.</summary>
        public int Value { get; }

        public string Group { get; }

        public string Label
        {
            get => _label;
            private set
            {
                if (SetProperty(ref _label, value ?? string.Empty)) OnPropertyChanged(nameof(AccessibleText));
            }
        }

        public bool IsOn
        {
            get => _isOn;
            private set
            {
                if (SetProperty(ref _isOn, value)) OnPropertyChanged(nameof(ItemStatus));
            }
        }

        /// <summary>The name for UI Automation: the group and the option («Fondos de la vista: Fondo 2»), unique in the editor.</summary>
        public string AccessibleText => Group + ": " + Label;

        /// <summary>«elegido» for the chosen option (what aria-pressed says in the draft).</summary>
        public string ItemStatus => _isOn ? "elegido" : string.Empty;

        internal void Set(string label, bool isOn)
        {
            Label = label;
            IsOn = isOn;
        }
    }

    /// <summary>A titled group of fields of the editor (the approved draft: «Estructura», «Tarima», «Larguero y holguras»).</summary>
    public sealed record SelectiveFieldGroup(string Title, IReadOnlyList<object> Fields);

    /// <summary>
    /// One row of the table «Fondos» of the «Rack» tab (two or more fondos; the approved draft): what goes by fondo, its fronts, the depth
    /// of its pallet and of its cabecera, and the separation to the next fondo (none for the last one), with its state. Its fields are
    /// refreshed in place.
    /// </summary>
    public sealed class SelectiveFondoRow : ObservableObject
    {
        private string _state = "Correcto";
        private bool _hasProblem;

        public SelectiveFondoRow(int fondo, DynamicFieldItem fronts, DynamicFieldItem depth, DynamicFieldItem cabecera, DynamicFieldItem separator)
        {
            Fondo = fondo;
            Fronts = fronts;
            Depth = depth;
            Cabecera = cabecera;
            Separator = separator;
            foreach (var field in Fields) field.PropertyChanged += OnFieldChanged;
        }

        public int Fondo { get; }

        public string Title => "Fondo " + (Fondo + 1).ToString(CultureInfo.InvariantCulture);

        public DynamicFieldItem Fronts { get; }

        public DynamicFieldItem Depth { get; }

        public DynamicFieldItem Cabecera { get; }

        /// <summary>The separation to the next fondo; null for the last one («Último»).</summary>
        public DynamicFieldItem Separator { get; }

        public bool IsLast => Separator == null;

        public bool HasSeparator => Separator != null;

        /// <summary>«Correcto», or the problem of the last value refused in the row.</summary>
        public string State { get => _state; private set => SetProperty(ref _state, value); }

        public bool HasProblem { get => _hasProblem; private set => SetProperty(ref _hasProblem, value); }

        internal IEnumerable<DynamicFieldItem> Fields => new[] { Fronts, Depth, Cabecera, Separator }.Where(f => f != null);

        internal void Refresh(SelectiveFondoRow next)
        {
            Fronts.Refresh(next.Fronts);
            Depth.Refresh(next.Depth);
            Cabecera.Refresh(next.Cabecera);
            Separator?.Refresh(next.Separator);
        }

        private void OnFieldChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(DynamicFieldItem.Error)) return;
            var errors = Fields.Select(f => f.Error).Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
            HasProblem = errors.Count > 0;
            State = errors.Count == 0 ? "Correcto" : (errors.Count == 1 ? "1 problema: " : errors.Count + " problemas: ") + string.Join(" ", errors);
        }
    }

    /// <summary>The height of one post under the matrix (the approved draft, «Altura por poste»): «P1 144 in», marked when a manual height governs it.</summary>
    public sealed record SelectivePostHeightChip(string Id, string Value, bool IsManual, string AccessibleText);

    /// <summary>The heights of the posts of one fondo of the view.</summary>
    public sealed record SelectivePostHeightBlock(string Title, bool HasTitle, string AccessibleText, IReadOnlyList<SelectivePostHeightChip> Chips);

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

    /// <summary>One computed value of the rack (read only), with how it is computed (the approved draft).</summary>
    public sealed record SelectiveCalcItem(string Label, string Value, string Hint = "");

    /// <summary>One post of «Avanzado»: its own peralte (empty = the one of the rack), its height and its cabecera in each fondo; refreshed in place.</summary>
    public sealed partial class SelectivePostRow : ObservableObject // T4 cb: partial (its cabeceras, SelectiveEditorViewModel.Cabecera.cs)
    {
        private string _cabecera;
        private string _height;

        public SelectivePostRow(int postIndex, DynamicFieldItem field, string cabecera, string height = "")
        {
            PostIndex = postIndex;
            Post = "P" + (postIndex + 1).ToString(CultureInfo.InvariantCulture);
            Field = field;
            _cabecera = cabecera ?? string.Empty;
            _height = height ?? string.Empty;
        }

        public int PostIndex { get; }

        public string Post { get; }

        public DynamicFieldItem Field { get; }

        /// <summary>«Calculada» or «Personalizada», by fondo: the custom cabeceras are kept and edited in the classic editor.</summary>
        public string Cabecera { get => _cabecera; private set => SetProperty(ref _cabecera, value ?? string.Empty); }

        /// <summary>The height of the post, by fondo («144 in / 120 in»), as the drawing gives it (the column «Altura» of the draft).</summary>
        public string Height { get => _height; private set => SetProperty(ref _height, value ?? string.Empty); }

        internal void Refresh(SelectivePostRow next)
        {
            Field.Refresh(next.Field);
            Cabecera = next._cabecera;
            Height = next._height;
        }
    }

    /// <summary>One tramo of «Medio frente» in the inspector of the front, while it is written (applied with «Aplicar tramos»).</summary>
    public sealed class SelectiveSegmentRow : ObservableObject
    {
        private string _lengthText;
        private bool _loaded;
        private string _restText = "(resto)";

        public SelectiveSegmentRow(int index, bool isRest, string lengthText, bool loaded)
        {
            Index = index;
            IsRest = isRest;
            _lengthText = lengthText ?? string.Empty;
            _loaded = loaded;
        }

        public int Index { get; }

        public string Title => "Tramo " + Number;

        /// <summary>The number of the tramo, for the column «Tramo» of the table.</summary>
        public string Number => (Index + 1).ToString(CultureInfo.InvariantCulture);

        /// <summary>The names for UI Automation, unique in every row (the approved draft): «Tramo 2: longitud», «Tramo 2 con largueros», «Quitar el tramo 2».</summary>
        public string LengthName => Title + ": longitud";

        public string LoadedName => Title + " con largueros";

        public string RemoveName => "Quitar el tramo " + Number;

        /// <summary>The last tramo is calculated: what is left of the front.</summary>
        public bool IsRest { get; }

        public bool HasLength => !IsRest;

        public string LengthText { get => _lengthText; set => SetProperty(ref _lengthText, value ?? string.Empty); }

        /// <summary>«(resto) ≈ n in» of the last tramo: the width of the front less the tramos written above it.</summary>
        public string RestText { get => _restText; internal set => SetProperty(ref _restText, value ?? "(resto)"); }

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
        private const string WaitingWriteText = "AutoCAD está escribiendo el rack: espera a que termine o pulsa «Dejar de esperar».";

        // S-28 (M-11 of the review of the entrega 1): once the command took the write, «Dejar de esperar» is no longer offered (I-02 of the
        // fifth review of Dinámico), and its answer will come.
        private const string TakenWriteText = "AutoCAD ya tomó la escritura del rack: espera a que termine, o termina o cancela la orden en AutoCAD; la ventana recibirá su respuesta.";

        // S-21 (M-04): tramos written in the inspector and not applied are a change that «Actualizar» does not write.
        private const string PendingSegmentsText = "Aplica los tramos con «Aplicar tramos» o descártalos antes de escribir.";

        // S-20 (I-01), the label of the approved draft when the fronts of the selection differ.
        private const string FloorBeamText = "Larguero a piso en N1";
        private const string FloorBeamVariesText = "Larguero a piso en N1 (varía; marcar lo pone en todos)";

        // S-23 (M-07), the hints of the approved draft for N1 on the floor.
        private const string NoBeamText = "Sin larguero (piso)";
        private const string NotInN1Text = "No en N1";

        // S-27 (M-10): the first entry of the list of dimension styles, as in the classic editor (no named style).
        private const string AutomaticStyle = "(Automático)";

        // The names of the groups of segmented buttons, for UI Automation.
        private const string ScopeGroup = "Alcance de la edición";
        private const string FondoViewGroup = "Fondos de la vista";

        // The groups of «Lo esencial» of the approved draft, by the keys of their fields.
        private static readonly (string Title, string[] Keys)[] EssentialLayout =
        {
            ("Estructura", new[] { "fronts", "rLevels", "fondos", "rCount" }),
            ("Tarima", new[] { "rWidth", "rHeight", "depth", "cabecera" }),
            ("Larguero y holguras", new[] { "rBeam", "rPeralte", "tolerance", "clearance" }),
            ("Poste", new[] { "peralte" }), // T5 pp: the peralte of every post, seen without unfolding «Poste y anotaciones»
        };

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

        private readonly IReadOnlyList<string> _drawingStyles;
        private readonly string _loadedStyle;

        private int _selectedTab = 1;
        private bool _dirty;
        private bool _designDirty;
        private bool _writing;
        private bool _writeTaken;
        private bool _confirmingClose;
        private bool _showDetail;
        private string _loadError;
        private string _baselineJson;
        private string _baselineName = string.Empty;
        private string _message = string.Empty;
        private bool _messageIsError;

        private IReadOnlyList<object> _essentials = Array.Empty<object>();
        private IReadOnlyList<SelectiveFieldGroup> _essentialGroups = Array.Empty<SelectiveFieldGroup>();
        private IReadOnlyList<SelectiveFondoRow> _fondoRows = Array.Empty<SelectiveFondoRow>();
        private IReadOnlyList<object> _annotationFields = Array.Empty<object>();
        private IReadOnlyList<object> _cellFields = Array.Empty<object>();
        private IReadOnlyList<object> _palletFields = Array.Empty<object>();
        private IReadOnlyList<object> _beamFields = Array.Empty<object>();
        private IReadOnlyList<object> _manualFields = Array.Empty<object>();
        private IReadOnlyList<DynamicFieldItem> _frontFields = Array.Empty<DynamicFieldItem>();
        private IReadOnlyList<SelectivePostRow> _postRows = Array.Empty<SelectivePostRow>();
        private IReadOnlyList<SelectiveCalcItem> _calculated;
        private SelectiveRackSystem _calculatedFor;
        private SelectiveAutomaticValues _automatic = new SelectiveAutomaticValues(Array.Empty<double>(), Array.Empty<double>());
        private IReadOnlyList<SelectivePostHeightBlock> _postHeights = Array.Empty<SelectivePostHeightBlock>();
        private IReadOnlyList<string> _halfFronts = Array.Empty<string>();
        private IReadOnlyList<SelectiveColumnHead> _columns = Array.Empty<SelectiveColumnHead>();
        private IReadOnlyList<SelectiveMatrixRow> _rows = Array.Empty<SelectiveMatrixRow>();
        private IReadOnlyList<string> _fondoOptions = Array.Empty<string>();
        private IReadOnlyList<SelectiveSegmentOption> _fondoViews = Array.Empty<SelectiveSegmentOption>();
        private readonly IReadOnlyList<SelectiveSegmentOption> _scopes = new[]
        {
            new SelectiveSegmentOption((int)SelectiveEditScope.Cell, "Celda", ScopeGroup),
            new SelectiveSegmentOption((int)SelectiveEditScope.Front, "Frente", ScopeGroup),
            new SelectiveSegmentOption((int)SelectiveEditScope.Level, "Nivel", ScopeGroup),
            new SelectiveSegmentOption((int)SelectiveEditScope.All, "Todo", ScopeGroup),
        };

        // The scope of the inspector (the approved draft): how the selection was made, and what its buttons choose.
        private SelectiveEditScope _scope = SelectiveEditScope.Cell;

        // What every cell held when the draft was last read from the drawing or written to it, for the mark «Modificada en el borrador».
        private Dictionary<(int Fondo, int Front, int Level), string> _baselineCells = new Dictionary<(int, int, int), string>();

        // «Medio frente»: the tramos being written in the inspector, for one front in one scope, until «Aplicar tramos».
        private List<SelectiveSegmentRow> _segmentRows = new List<SelectiveSegmentRow>();
        private (int Front, int? Scope)? _segmentKey;
        private string _segmentError = string.Empty;

        // S-21 (M-04): whether the rows of the inspector differ from the tramos the draft holds for their front, as last measured (before the
        // change in progress), and what to say when a change of front or of fondos dropped them.
        private bool _segmentsPending;
        private string _droppedSegments;

        /// <param name="editor">The draft of the rack, as it was read (its design is the baseline of «Modificado» and of «Descartar»).</param>
        /// <param name="rackId">The identity of the rack in the drawing.</param>
        /// <param name="update">What «Actualizar» does with this editor (the host writes the drawing); null when it is not offered.</param>
        /// <param name="linkedStates">The states of the linked properties the rack was opened with: «Descartar» loads the draft again with them.</param>
        /// <param name="linkedVariableNames">The names of the variables that govern them, for display only.</param>
        /// <param name="dimensionStyles">The dimension styles of the drawing, for the list of «Estilo de cota» (S-27); null = none read.</param>
        public SelectiveEditorViewModel(
            SelectiveDraftEditor editor,
            string rackId,
            Action<SelectiveEditorViewModel> update = null,
            IReadOnlyDictionary<PropertyId, LinkedPropertyEditState> linkedStates = null,
            IReadOnlyDictionary<PropertyId, string> linkedVariableNames = null,
            IReadOnlyList<string> dimensionStyles = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            RackId = rackId;
            _update = update;
            _openedStates = linkedStates == null ? null : new Dictionary<PropertyId, LinkedPropertyEditState>(linkedStates);
            _variableNames = linkedVariableNames == null ? null : new Dictionary<PropertyId, string>(linkedVariableNames);
            _drawingStyles = (dimensionStyles ?? Array.Empty<string>())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToList();
            _loadedStyle = NamedStyle(editor.DimensionStyle);

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

        /// <summary>0 = Rack, 1 = Matriz (the first tab, as the Owner decided for Dinámico), 2 = Extremos y seguridad, 3 = Avanzado.</summary>
        public int SelectedTab
        {
            get => _selectedTab;
            set
            {
                // «Calculado» reads the BOM: the view is told to read it again only while it is shown (the Rack tab), and when it is shown.
                if (!SetProperty(ref _selectedTab, value)) return;
                if (value == 0) OnPropertyChanged(nameof(Calculated));
                if (value == SafetyTabIndex) OnSafetyTabShown(); // E2 seguridad: the same rule for what that tab reads from the BOM
            }
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

        // E2 P3: a new Selectivo of the window is written with «Insertar en el dibujo…», never with «Actualizar».
        public bool CanUpdate => _update != null && !_isNewRack && _loadError == null && _dirty && !_writing && !_segmentsPending && !PropertiesBlockUpdate && _editor.Design != null
                                 && string.IsNullOrEmpty(_editor.LastError);

        public string UpdateBlockedText
        {
            get
            {
                if (_isNewRack) return NewRackUpdateText;
                if (_update == null) return "Aquí no se puede escribir el dibujo.";
                if (_loadError != null) return LoadFailedText;
                if (_writing) return WritingText;
                if (_segmentsPending) return PendingSegmentsText;
                if (!_dirty) return "Sin cambios que escribir.";
                if (!string.IsNullOrEmpty(_editor.LastError)) return _editor.LastError;
                if (PropertiesBlockUpdate) return PropertiesTab.BlockedText; // T4 va (VA-07)
                return string.Empty;
            }
        }

        private string LoadFailedText =>
            "No se pudo recomponer el rack al abrirlo: " + _loadError + " Aquí queda de solo lectura; edítalo con RACKEDITAR (el editor clásico).";

        /// <summary>What the editor says while AutoCAD writes it: «Dejar de esperar» only while the command has not taken the write (S-28).</summary>
        public string WritingText => _writeTaken ? TakenWriteText : WaitingWriteText;

        /// <summary>«Cerrar el editor» of a draft with changes asks first, inline: discard them and close, or cancel.</summary>
        public bool IsConfirmingClose => _confirmingClose;

        /// <summary>
        /// «Lo esencial» of the rack, as the approved draft has it: the fronts, levels, fondos and pallets per level, the pallet (width,
        /// height, depth; and the depth of the cabecera with one fondo), the larguero and its peralte, the holguras, and (T5 pp) the peralte
        /// of the post. Every field writes the WHOLE rack, in every fondo, whatever the fondos of the view of «Matriz» are: the same write as
        /// «Todo» of the matrix with «Todos los fondos». In the order of <see cref="EssentialGroups"/>.
        /// </summary>
        public IReadOnlyList<object> Essentials => _essentials;

        /// <summary>«Lo esencial» in the groups of the approved draft: «Estructura», «Tarima» and «Larguero y holguras» (the same field items);
        /// T5 pp adds «Poste».</summary>
        public IReadOnlyList<SelectiveFieldGroup> EssentialGroups => _essentialGroups;

        /// <summary>The line under «Lo esencial» (the approved draft).</summary>
        public string EssentialsHelp => "Escribe en todo el rack" + (HasFondos ? ", en todos los fondos" : string.Empty)
                                        + ". Para un frente, un nivel o una celda, usa «Matriz».";

        /// <summary>The table «Fondos» (two or more fondos): what goes by fondo, each row written in its fondo only.</summary>
        public IReadOnlyList<SelectiveFondoRow> FondoRows => _fondoRows;

        /// <summary>The separation of each gap between consecutive fondos (none with one fondo): the column «Separación al siguiente» of <see cref="FondoRows"/>.</summary>
        public IReadOnlyList<DynamicFieldItem> SeparatorFields => _fondoRows.Where(r => r.Separator != null).Select(r => r.Separator).ToList();

        /// <summary>«Poste y anotaciones»: the post, the annotation scale and the dimensions (the approved draft; T5 pp: the peralte of the
        /// post is in «Lo esencial» › «Poste»).</summary>
        public IReadOnlyList<object> AnnotationFields => _annotationFields;

        /// <summary>The summary of the folded «Poste y anotaciones» (the approved draft): «Poste 3 in · cotas: Estándar · con tarimas».</summary>
        public string AnnotationSummary => "Poste " + Num(_editor.PostPeralte) + " in · cotas: "
                                           + (DimensionDetails.FirstOrDefault(d => d.Detail == _editor.Dimensions).Label ?? _editor.Dimensions.ToString())
                                           + (_editor.DrawPallets ? " · con tarimas" : " · sin tarimas");

        /// <summary>
        /// «Calculado» (the approved draft): the larguero length, the height of the cabecera, the separation N2 → N3, the depth of the
        /// cabecera, and the counts of cabeceras, largueros, pallet positions and separators. The counts of pieces are read from the BOM
        /// (<see cref="SelectiveDraftEditor.BomCounts"/>, on a copy: reading them changes nothing of the draft); they are computed when the list
        /// is read, once per recomposition, and the view is told to read it only while the Rack tab is shown.
        /// </summary>
        public IReadOnlyList<SelectiveCalcItem> Calculated
        {
            get
            {
                if (_calculated == null || !ReferenceEquals(_calculatedFor, _editor.System))
                {
                    _calculated = BuildCalculated();
                    _calculatedFor = _editor.System;
                    CalculatedBuilds++;
                }

                return _calculated;
            }
        }

        /// <summary>Test seam (M-03 of the review of the polish): how many times «Calculado» was counted (each one reads the BOM).</summary>
        internal int CalculatedBuilds { get; private set; }

        public IReadOnlyList<SelectiveColumnHead> Columns => _columns;

        public IReadOnlyList<SelectiveMatrixRow> Rows => _rows;

        /// <summary>«Altura por poste» under the matrix (the approved draft): the posts of each fondo of the view with their height.</summary>
        public IReadOnlyList<SelectivePostHeightBlock> PostHeights => _postHeights;

        /// <summary>The inspector: the pallet, the larguero and the manual values of the selected cells (in the order of the three groups below).</summary>
        public IReadOnlyList<object> CellFields => _cellFields;

        /// <summary>The group «Tarima» of the inspector: width, height and pallets per level.</summary>
        public IReadOnlyList<object> PalletFields => _palletFields;

        /// <summary>The group «Larguero» of the inspector: the larguero and its peralte.</summary>
        public IReadOnlyList<object> BeamFields => _beamFields;

        /// <summary>The group «Manual» of the inspector: the manual length of the larguero and the manual clear.</summary>
        public IReadOnlyList<object> ManualFields => _manualFields;

        /// <summary>The summary of «Larguero» (the approved draft): «Peralte 4 in», «Peralte varía» or «Sin larguero (piso)».</summary>
        public string BeamSummary
        {
            get
            {
                if (_selection.Count > 0 && _selection.All(p => HasNoLarguero(p.FrontIndex, p.LevelIndex))) return NoBeamText;
                var peralte = _editor.ReadCells(_selection, c => c.BeamPeralte);
                return peralte.Varies ? "Peralte varía" : peralte.HasValue ? "Peralte " + Num(peralte.Value) + " in" : string.Empty;
            }
        }

        /// <summary>The summary of «Manual» (the approved draft): «Automático» or «n celdas con valor manual».</summary>
        public string ManualSummary
        {
            get
            {
                var manual = _editor.ScopeFondos
                    .SelectMany(k => _selection.Select(p => _editor.Cell(k, p.FrontIndex, p.LevelIndex)))
                    .Count(c => c != null && (c.BeamLength.HasValue || c.Clear.HasValue));
                return manual == 0 ? "Automático" : Count(manual, "celda con valor manual", "celdas con valor manual");
            }
        }

        /// <summary>The inspector: the levels, the floor larguero and the manual height of the fronts of the selection.</summary>
        public IReadOnlyList<DynamicFieldItem> FrontFields => _frontFields;

        /// <summary>The title of the section of the fronts (the approved draft): «Frente 2», «Frentes 1, 2», with the fondos when there are several.</summary>
        public string FrontTitle
        {
            get
            {
                var fronts = SelectedFronts.Select(f => f + 1).ToList();
                var fondos = _editor.ScopeFondos.Where(k => SelectedFronts.Any(f => f < _editor.FrontCount(k))).ToList();
                return ListText(fronts, "Frente", "Frentes")
                       + (HasFondos ? " · " + (fondos.Count == 1 ? "fondo " + (fondos[0] + 1) : "fondos " + JoinAnd(fondos.Select(k => k + 1))) : string.Empty);
            }
        }

        /// <summary>The summary of the section of the fronts (the approved draft): «3 niveles · larguero a piso · medio frente».</summary>
        public string FrontSummary
        {
            get
            {
                var levels = _editor.ReadFronts(SelectedFronts, f => f.LevelCount);
                var floor = _editor.ReadFronts(SelectedFronts, f => f.FloorBeam);
                var split = SegmentFront is int front && _editor.ScopeFondos.Select(k => _editor.Front(k, front)).Any(f => f?.IsSplit == true);
                return (levels.Varies ? "niveles: varía" : levels.HasValue ? Count(levels.Value, "nivel", "niveles") : string.Empty)
                       + (floor.HasValue && floor.Value ? " · larguero a piso" : string.Empty)
                       + (split ? " · medio frente" : string.Empty);
            }
        }

        /// <summary>«Avanzado»: the own peralte of each post, its height and its cabecera.</summary>
        public IReadOnlyList<SelectivePostRow> PostRows => _postRows;

        /// <summary>The head of the column of the heights of «Postes y cabeceras» (the approved draft).</summary>
        public string PostHeightHeader => HasFondos ? "Altura (por fondo)" : "Altura";

        /// <summary>The summary of «Postes y cabeceras» (the approved draft): «3 postes · 1 peralte propio · 0 cabeceras personalizadas».</summary>
        public string PostsSummary
        {
            get
            {
                var (cabeceras, peraltes) = Kept();
                return Count(_editor.PostCount, "poste", "postes") + " · " + Count(peraltes, "peralte propio", "peraltes propios") + " · "
                       + Count(cabeceras, "cabecera personalizada", "cabeceras personalizadas");
            }
        }

        /// <summary>«Avanzado»: the fronts in tramos.</summary>
        public IReadOnlyList<string> HalfFronts => _halfFronts;

        public string HalfFrontsText => _halfFronts.Count == 0 ? "Ningún frente está partido." : "Frentes partidos: " + string.Join(" · ", _halfFronts) + ".";

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

        /// <summary>The segmented buttons over the matrix (the approved draft): «Todos los fondos · Fondo 1 · Fondo n», the chosen one marked.</summary>
        public IReadOnlyList<SelectiveSegmentOption> FondoViews => _fondoViews;

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
                SayDropped();
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
                    : "Lo que ves es donde escribes: cada celda escribe en el mismo frente y nivel de todos los fondos que lo tienen.";
            }
        }

        // ------------------------------------------------------------------ the inspector

        public IReadOnlyList<SelectiveMatrixPosition> SelectedPositions => _selection.ToList();

        public IReadOnlyList<int> SelectedFronts => _selection.Select(p => p.FrontIndex).Distinct().OrderBy(f => f).ToList();

        public IReadOnlyList<int> SelectedLevels => _selection.Select(p => p.LevelIndex).Distinct().OrderBy(l => l).ToList();

        public SelectiveMatrixPosition PrimaryPosition => _primary;

        /// <summary>The scope of the inspector: how the selection was made (a click, «F…», «N…», «Todo») or chosen with its buttons.</summary>
        public SelectiveEditScope Scope => _scope;

        /// <summary>The buttons «Celda · Frente · Nivel · Todo» of the inspector (the approved draft); «Celdas (n)» for several cells.</summary>
        public IReadOnlyList<SelectiveSegmentOption> ScopeOptions => _scopes;

        /// <summary>The title of the inspector (the approved draft): «Frente 1 · Nivel 1», «n celdas», «Frentes 1, 2», «Niveles 2, 3», «Todo el rack», «Todo el fondo 2».</summary>
        public string InspectorTitle
        {
            get
            {
                switch (_scope)
                {
                    case SelectiveEditScope.All:
                        return HasFondos && _editor.FondoScope is int fondo ? "Todo el fondo " + (fondo + 1).ToString(CultureInfo.InvariantCulture) : "Todo el rack";
                    case SelectiveEditScope.Front:
                        return ListText(SelectedFronts.Select(f => f + 1).ToList(), "Frente", "Frentes");
                    case SelectiveEditScope.Level:
                        return ListText(SelectedLevels.Select(l => l + 1).ToList(), "Nivel", "Niveles");
                    default:
                        return _selection.Count == 1
                            ? "Frente " + (_selection[0].FrontIndex + 1).ToString(CultureInfo.InvariantCulture) + " · Nivel " + (_selection[0].LevelIndex + 1).ToString(CultureInfo.InvariantCulture)
                            : Count(_selection.Count, "celda", "celdas");
                }
            }
        }

        /// <summary>
        /// «Destino: …» of the inspector (the approved draft): how many cells a write of the inspector reaches (each selected position in
        /// every fondo of the view that has it), in which fondos, and in which fronts.
        /// </summary>
        public string TargetText
        {
            get
            {
                var targets = _editor.ScopeFondos.SelectMany(k => _selection.Where(p => p.LevelIndex < _editor.LevelCount(k, p.FrontIndex)).Select(p => k)).ToList();
                var fondos = targets.Distinct().OrderBy(k => k).ToList();
                return "Destino: " + Count(targets.Count, "celda", "celdas")
                       + (HasFondos && fondos.Count > 0 ? " en " + FondoList(fondos) : string.Empty)
                       + " · " + ListText(SelectedFronts.Select(f => f + 1).ToList(), "frente", "frentes") + ".";
            }
        }

        /// <summary>
        /// A button of the scope of the inspector: «Celda» keeps the main cell, «Frente» takes every cell of the fronts of the selection,
        /// «Nivel» the levels of the selection in every front that has them, «Todo» every cell of the fondos of the view.
        /// </summary>
        public void SetScope(SelectiveEditScope scope)
        {
            switch (scope)
            {
                case SelectiveEditScope.Cell:
                {
                    var primary = _primary;
                    _selection.Clear();
                    Add(primary.FrontIndex, primary.LevelIndex);
                    _primary = _anchor = primary;
                    break;
                }

                case SelectiveEditScope.Front:
                {
                    var fronts = SelectedFronts;
                    _selection.Clear();
                    foreach (var f in fronts)
                        for (var l = 0; l < _editor.ScopeLevelCount(f); l++) Add(f, l);
                    break;
                }

                case SelectiveEditScope.Level:
                {
                    var levels = SelectedLevels;
                    _selection.Clear();
                    for (var f = 0; f < _editor.ScopeFrontCount; f++)
                        foreach (var l in levels) Add(f, l);
                    break;
                }

                default:
                    _selection.Clear();
                    foreach (var p in _editor.ScopePositions()) Add(p.FrontIndex, p.LevelIndex);
                    break;
            }

            _scope = scope;
            RebuildSelection();
            SayDropped();
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
        /// it in, or takes it out of, those fronts in the fondos of the scope. When the fronts differ («varía»), a click puts it in every
        /// one of them, as the approved draft says («marcar lo pone en todos»), never takes it out: WPF turns an indeterminate box into
        /// false, and that false means «all» here (S-20, I-01 of the review of the entrega 1).
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
                var shown = SelectionFloorBeam;
                var next = shown.HasValue ? value : value.HasValue ? true : (bool?)null;
                if (!next.HasValue || next == shown)
                {
                    RaiseFloorBeam();
                    return;
                }

                var fronts = SelectedFronts;
                var error = Apply(() => _editor.SetFloorBeam(fronts, next.Value));
                if (error != null) Say(error, error: true);
                RaiseFloorBeam();
            }
        }

        /// <summary>The label of the box: it says «varía» when the fronts of the selection differ, with what a click does (the approved draft).</summary>
        public string SelectionFloorBeamLabel => SelectionFloorBeam.HasValue ? FloorBeamText : FloorBeamVariesText;

        private void RaiseFloorBeam()
        {
            OnPropertyChanged(nameof(SelectionFloorBeam));
            OnPropertyChanged(nameof(SelectionFloorBeamLabel));
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

        /// <summary>
        /// The tramos of the inspector differ from those of their front in the draft: written and not applied. They are a change (the editor
        /// is «Modificado», «Cerrar el editor» asks and the drawing does not close), and «Actualizar» does not write until they are applied or
        /// discarded (S-21, M-04 of the review of the entrega 1).
        /// </summary>
        public bool HasPendingSegments => _segmentsPending;

        /// <summary>«Descartar tramos»: the rows go back to the tramos the front has in the draft.</summary>
        public bool CanDiscardSegments => CanEdit && _segmentsPending;

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
                if (_segmentRows.Count == 1) return "Con un solo tramo, F" + (front + 1) + " es un frente completo: agrega otro tramo o usa «Sin medio frente»."; // T4 ins
                return "F" + (front + 1) + widthText + " en " + _segmentRows.Count.ToString(CultureInfo.InvariantCulture)
                       + " tramos. El último se calcula: lo que sobra del frente. Se escribe con «Aplicar tramos».";
            }
        }

        /// <summary>«Partir en tramos»: the classic seed, a loaded tramo with its length to write and the calculated rest.</summary>
        public void StartSegments()
        {
            if (!CanStartSegments) return;
            ReplaceSegmentRows(new List<SelectiveSegmentRow>
            {
                new SelectiveSegmentRow(0, false, string.Empty, true),
                new SelectiveSegmentRow(1, true, string.Empty, false),
            });
            _segmentError = string.Empty;
            SegmentGesture("Dos tramos, como la semilla del diálogo clásico: escribe la longitud del tramo 1 y pulsa «Aplicar tramos»."); // T4 ins
            RefreshDirty();
            RaiseSegments();
        }

        /// <summary>«+ Agregar tramo»: one more loaded tramo at the end, as the classic dialog adds it.</summary>
        public void AddSegment()
        {
            if (!CanChangeSegments) return;
            var rows = PendingSegments();
            rows.Add(new SelectiveSegment { Length = 0.0, Loaded = true });
            ReplaceSegmentRows(RowsOf(rows));
            SegmentGesture("El tramo " + (rows.Count - 1).ToString(CultureInfo.InvariantCulture) + " deja de ser el resto: escribe su longitud."); // T4 ins
            RefreshDirty();
            RaiseSegments();
        }

        /// <summary>«Quitar» of a tramo of the inspector (it is written with «Aplicar tramos»).</summary>
        public void RemoveSegment(int index)
        {
            if (!CanChangeSegments || index < 0 || index >= _segmentRows.Count) return;
            var countBefore = _segmentRows.Count; // T4 ins: what the gesture says (dolor 15)
            var previousLength = index > 0 ? _segmentRows[index - 1].LengthText : null;
            var rows = PendingSegments();
            rows.RemoveAt(index);
            ReplaceSegmentRows(RowsOf(rows));
            SegmentRemovedGesture(index, countBefore, previousLength);
            RefreshDirty();
            RaiseSegments();
        }

        /// <summary>«Descartar tramos»: the tramos written and not applied go back to those the front has in the draft (S-21).</summary>
        public void DiscardSegments()
        {
            if (!CanDiscardSegments || !_segmentKey.HasValue) return;
            var (front, scope) = _segmentKey.Value;
            ReplaceSegmentRows(RowsOf(StoredSegments(front, scope)));
            _segmentError = string.Empty;
            SegmentGesture(string.Empty); // T4 ins
            RefreshDirty();
            RaiseSegments();
            Say("Se descartaron los tramos sin aplicar de F" + (front + 1).ToString(CultureInfo.InvariantCulture) + ".", error: false);
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

            // Applied: the rows are the tramos of the front now, read again from the draft (nothing was dropped).
            _segmentKey = null;
            SetSegmentError(string.Empty);
            SegmentGesture(string.Empty); // T4 ins
            RebuildSegments(announceDrop: false);
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

            // A whole front now, which the user asked for: rows written and not applied go with it, with no notice of a drop.
            _segmentKey = null;
            SetSegmentError(string.Empty);
            SegmentGesture(string.Empty); // T4 ins
            RebuildSegments(announceDrop: false);
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

            _scope = SelectiveEditScope.Cell;
            RebuildSelection();
            SayDropped();
        }

        /// <summary>«F1»…: every cell of that front in the fondos of the scope.</summary>
        public void SelectFront(int frontIndex)
        {
            var levels = _editor.ScopeLevelCount(frontIndex);
            if (levels == 0) return;
            _selection.Clear();
            for (var l = 0; l < levels; l++) Add(frontIndex, l);
            _primary = _anchor = new SelectiveMatrixPosition(frontIndex, 0);
            _scope = SelectiveEditScope.Front;
            RebuildSelection();
            SayDropped();
        }

        /// <summary>«N1»…: that level in every front that has it.</summary>
        public void SelectLevel(int levelIndex)
        {
            var fronts = Enumerable.Range(0, _editor.ScopeFrontCount).Where(f => Exists(f, levelIndex)).ToList();
            if (fronts.Count == 0) return;
            _selection.Clear();
            foreach (var f in fronts) Add(f, levelIndex);
            _primary = _anchor = new SelectiveMatrixPosition(fronts[0], levelIndex);
            _scope = SelectiveEditScope.Level;
            RebuildSelection();
            SayDropped();
        }

        /// <summary>«Todo»: every cell of the fondos of the scope.</summary>
        public void SelectAll()
        {
            _selection.Clear();
            foreach (var p in _editor.ScopePositions()) Add(p.FrontIndex, p.LevelIndex);
            if (_selection.Count == 0) return;
            _primary = _anchor = _selection[0];
            _scope = SelectiveEditScope.All;
            RebuildSelection();
            SayDropped();
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

            // «Descartar» also discards the tramos written and not applied (S-21): the rows are read again from the draft, with no notice of a drop.
            _segmentKey = null;
            _segmentsPending = false;
            _droppedSegments = null;
            if (baseline != null && !_editor.LoadFrom(baseline, _openedStates, _baselineName, _variableNames))
            {
                _loadError = string.IsNullOrWhiteSpace(_editor.LastError) ? "sin detalle." : _editor.LastError;
                Say(LoadFailedText, error: true);
                Rebuild();
                return;
            }

            TakeBaseline();
            _propertiesTab?.Reset(); // T4 va: «Descartar» drops the properties written in the tab too (VA-07)
            _confirmingClose = false;
            Say(_isNewRack
                ? "Borrador descartado."
                : "Borrador descartado: se muestra lo último que se leyó del dibujo o se escribió en él desde este editor.", error: false);
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
            _writeTaken = false;
            SetConfirmingClose(false);
            _propertiesTab?.Lock(true); // T4 va
            Say("Escribiendo el rack en el dibujo…", error: false);
            Rebuild();
        }

        /// <summary>
        /// The command of AutoCAD took the write (<c>RequestStarted</c>): its answer will come, and the window no longer offers «Dejar de
        /// esperar», so the editor no longer says to press it (S-28).
        /// </summary>
        public void WriteTaken()
        {
            if (!_writing || _writeTaken) return;
            _writeTaken = true;
            RaiseState();
        }

        /// <summary>The answer arrived, or the window stopped waiting: the editor can be edited again.</summary>
        public void EndWriting()
        {
            if (!_writing) return;
            _writing = false;
            _writeTaken = false;
            _propertiesTab?.Lock(false); // T4 va
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

            // What was written is what the draft holds (nothing can be edited while AutoCAD writes): the marks «Modificada» start from it.
            if (!_designDirty) TakeBaselineCells();
            RebuildSelection();
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
            _designDirty = false;
            TakeBaselineCells();
            _segmentsPending = MeasurePendingSegments();
            _dirty = _segmentsPending;
            RaiseState();
        }

        // What every cell of every fondo holds now (its pallet, larguero and manual values, and the «Larguero a piso» of its front).
        private void TakeBaselineCells()
        {
            var cells = new Dictionary<(int Fondo, int Front, int Level), string>();
            var marks = SafetyMarks(); // E2 seguridad: T and P of the cell are part of its state («Modificada», the approved draft)
            for (var k = 0; k < _editor.FondoCount; k++)
                for (var f = 0; f < _editor.FrontCount(k); f++)
                    for (var l = 0; l < _editor.LevelCount(k, f); l++)
                        cells[(k, f, l)] = StateKey(k, f, l) + marks.StateOf(k, f, l);
            _baselineCells = cells;
        }

        // The content of a cell (as «Distinta del resto» compares it) and the floor larguero of its front (as «Modificada» compares it).
        private static string CellKey(SelectiveEditorCell cell)
            => cell == null
                ? string.Empty
                : string.Join("|", Num(cell.Frente), Num(cell.Alto), cell.PalletCount.ToString(CultureInfo.InvariantCulture), (cell.BeamId ?? string.Empty).Trim().ToUpperInvariant(),
                    Num(cell.BeamPeralte), cell.BeamLength.HasValue ? Num(cell.BeamLength.Value) : "-", cell.Clear.HasValue ? Num(cell.Clear.Value) : "-");

        private string StateKey(int fondo, int front, int level)
            => CellKey(_editor.Cell(fondo, front, level)) + "|" + (_editor.Front(fondo, front)?.FloorBeam == true ? "piso" : "-");

        // «Modificado» is a difference of the design (its JSON) or of the name with what the drawing has (as last read or written), never a
        // gesture: leaving a field, a value refused, or a change undone leave the draft clean. Tramos written in the inspector and not applied
        // are a change too (S-21), so the editor asks before closing and the drawing does not close.
        private void UpdateDirty()
        {
            _designDirty = !string.Equals(DesignJson(), _baselineJson, StringComparison.Ordinal)
                           || !string.Equals((_editor.Name ?? string.Empty).Trim(), _baselineName, StringComparison.Ordinal);
            RefreshDirty();
        }

        // The state again with the design measured last and the tramos of the inspector measured now (each keystroke in a tramo comes here,
        // with no serialization of the design).
        private void RefreshDirty()
        {
            var pending = MeasurePendingSegments();
            var dirty = _designDirty || pending || CabeceraPending || PropertiesDirty; // T4 cb: a cabecera with changes not applied (D-08); T4 va: the tab «Propiedades» (VA-07)
            if (dirty == _dirty && pending == _segmentsPending) return;
            _segmentsPending = pending;
            _dirty = dirty;
            if (!dirty) SetConfirmingClose(false);
            RaiseState();
        }

        // The rows of the inspector against the tramos their front has in the draft, in the first fondo of their scope that has it, compared
        // as the inspector shows them (up to three decimals), so a row nobody touched is never pending.
        private bool MeasurePendingSegments()
        {
            if (!_segmentKey.HasValue) return false;
            var (front, scope) = _segmentKey.Value;
            return !string.Equals(RowsKey(_segmentRows), RowsKey(RowsOf(StoredSegments(front, scope))), StringComparison.Ordinal);
        }

        // A gesture that changed the front or the fondos of the inspector says what it dropped (S-21).
        private void SayDropped()
        {
            var dropped = TakeDropped();
            if (dropped != null) Say(dropped, error: false);
        }

        private string TakeDropped()
        {
            var dropped = _droppedSegments;
            _droppedSegments = null;
            return dropped;
        }

        private void SetConfirmingClose(bool value)
        {
            if (_confirmingClose == value) return;
            if (value) CloseRestore(); // E2 Restaurar: the question of closing closes the panel of «Restaurar layout…» (the approved draft)
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
                Rebuild();
                UpdateDirty();
                Say(string.Join(" ", new[] { reason, TakeDropped() }.Where(t => !string.IsNullOrWhiteSpace(t))), error: true);
                return reason;
            }

            var notices = new List<string>();
            if (!string.IsNullOrWhiteSpace(edit.Notice)) notices.Add(edit.Notice);
            if (reduces != null)
            {
                var lost = LossText(keptBefore, Kept(), reduces);
                if (lost != null) notices.Add(lost);
            }

            // What the change dropped from the tramos of the inspector (fewer fronts or fondos move the selection) is said with it (S-21).
            Rebuild();
            UpdateDirty();
            var dropped = TakeDropped();
            if (dropped != null) notices.Add(dropped);
            Say(string.Join(" ", notices), error: false);
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
        // classic editor does, which says nothing; the window says it after the write. What «Descartar» does is said as for «Restaurar»: a
        // new Selectivo goes back to the defaults (M-4 of the review of the entrega 2).
        private string LossText((int Cabeceras, int Peraltes) before, (int Cabeceras, int Peraltes) after, string what)
        {
            var cabeceras = Math.Max(0, before.Cabeceras - after.Cabeceras);
            var peraltes = Math.Max(0, before.Peraltes - after.Peraltes);
            if (cabeceras == 0 && peraltes == 0) return null;
            var parts = new List<string>();
            if (cabeceras > 0) parts.Add(Count(cabeceras, "cabecera personalizada", "cabeceras personalizadas"));
            if (peraltes > 0) parts.Add(Count(peraltes, "peralte propio", "peraltes propios"));
            return "Al reducir " + what + " se quitaron " + string.Join(" y ", parts) + " de los postes que ya no existen, como en el editor clásico. "
                   + "Volver a subirlos no los recupera; "
                   + (_isNewRack ? SelectiveLayoutRestoreResult.NewRackDiscardText : SelectiveLayoutRestoreResult.DiscardText);
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
            OnPropertyChanged(nameof(WritingText));
            OnPropertyChanged(nameof(CanEditSegments));
            OnPropertyChanged(nameof(CanStartSegments));
            OnPropertyChanged(nameof(CanChangeSegments));
            OnPropertyChanged(nameof(HasPendingSegments));
            OnPropertyChanged(nameof(CanDiscardSegments));
            OnPropertyChanged(nameof(CanInsertView));
            OnPropertyChanged(nameof(InsertViewBlockedText));
            RaiseSafetyState(); // E2 seguridad
            RaiseStateIns(); // T4 ins: the button and the panel «Insertar vista…», «Aplicar tramos»
            RaiseAllPosts(); // T5 pp: the row «Todos los postes» of «Postes y cabeceras»
        }

        // ------------------------------------------------------------------ internals: the lists, refreshed in place

        // The lists keep their items: a recomposition refreshes them in place, so the focus, the box being written and the button under the
        // mouse survive it (I-05 of Dinámico). Only a list whose fields change is replaced.
        private void Rebuild()
        {
            if (Merge(ref _essentials, BuildEssentials(), nameof(Essentials)))
            {
                _essentialGroups = EssentialLayout
                    .Select(g => new SelectiveFieldGroup(g.Title, g.Keys.Select(key => _essentials.FirstOrDefault(f => KeyOf(f) == key)).Where(f => f != null).ToList()))
                    .Where(g => g.Fields.Count > 0)
                    .ToList();
                OnPropertyChanged(nameof(EssentialGroups));
            }

            MergeFondoRows(BuildFondoRows());
            Merge(ref _annotationFields, BuildAnnotationFields(), nameof(AnnotationFields));
            MergePosts(BuildPostRows());
            RefreshCabeceraCells(); // T4 cb: «Editar cabecera…» / «Volver a la calculada» of each post and fondo
            _calculated = null;
            _halfFronts = BuildHalfFronts();
            if (_selectedTab == 0) OnPropertyChanged(nameof(Calculated));
            OnPropertyChanged(nameof(HalfFronts));
            OnPropertyChanged(nameof(HalfFrontsText));
            OnPropertyChanged(nameof(EssentialsHelp));
            OnPropertyChanged(nameof(AnnotationSummary));
            OnPropertyChanged(nameof(PostHeightHeader));
            OnPropertyChanged(nameof(PostsSummary));

            var options = _editor.FondoCount > 1
                ? new[] { "Todos los fondos" }.Concat(Enumerable.Range(1, _editor.FondoCount).Select(k => "Fondo " + k.ToString(CultureInfo.InvariantCulture))).ToList()
                : new List<string>();
            if (!options.SequenceEqual(_fondoOptions))
            {
                _fondoOptions = options;
                _fondoViews = options.Select((label, index) => new SelectiveSegmentOption(index, label, FondoViewGroup)).ToList();
                OnPropertyChanged(nameof(FondoOptions));
                OnPropertyChanged(nameof(FondoViews));
            }

            foreach (var view in _fondoViews) view.Set(_fondoOptions[view.Value], view.Value == FondoScopeIndex);

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
            RefreshRestore(); // E2 Restaurar: the open panel of «Restaurar layout…» follows the draft
            RefreshInsertPanel(); // T4 ins: the open panel of «Insertar vista…» follows the draft (DG-08)
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

            // What every cell of the view holds now, read once: its content, its state (with the floor larguero of its front) and whether it
            // has a manual value the drawing uses. «Distinta del resto» (the approved draft): the content most cells of the view have; a cell
            // with another one is distinct, when the view has more than one content. M-02 of the review of the polish: a manual larguero length
            // in a fondo that is not the width master is kept, but the resolver does not use it, so it is no «M» (it says so, unused).
            var master = _editor.WidthMasterFondo;
            var marks = SafetyMarks(); // E2 seguridad: T and P, read once for the matrix
            var now = new Dictionary<(int Fondo, int Front, int Level), (string Content, string State, bool Manual, bool Unused)>();
            foreach (var k in scope)
            {
                for (var f = 0; f < _editor.FrontCount(k); f++)
                {
                    var floor = _editor.Front(k, f)?.FloorBeam == true ? "piso" : "-";
                    for (var l = 0; l < _editor.LevelCount(k, f); l++)
                    {
                        var cell = _editor.Cell(k, f, l);
                        var content = CellKey(cell);
                        var length = cell?.BeamLength.HasValue == true;
                        now[(k, f, l)] = (content, content + "|" + floor + marks.StateOf(k, f, l), cell?.Clear.HasValue == true || (length && k == master), length && k != master);
                    }
                }
            }

            var contents = now.Values.GroupBy(v => v.Content, StringComparer.Ordinal).OrderByDescending(g => g.Count()).ToList();
            var usual = contents.Count > 1 ? contents[0].Key : null;

            foreach (var row in _rows)
            {
                foreach (var item in row.Cells)
                {
                    var f = item.FrontIndex;
                    var l = item.LevelIndex;
                    var position = "Frente " + (f + 1).ToString(CultureInfo.InvariantCulture) + " · Nivel " + (l + 1).ToString(CultureInfo.InvariantCulture);
                    var (tope, parrilla) = (marks.Tope(f, l), marks.Parrilla(f, l));
                    item.SetSafety(tope, parrilla);
                    if (!Exists(f, l))
                    {
                        item.Set("—", string.Empty, false, false, false, false, false, false, position + ": no existe en los fondos de la vista");
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
                    var holders = scope.Where(k => now.ContainsKey((k, f, l))).Select(k => now[(k, f, l)]).ToList();
                    var modified = scope.Any(k => now.TryGetValue((k, f, l), out var held)
                                                  && (!_baselineCells.TryGetValue((k, f, l), out var was) || !string.Equals(was, held.State, StringComparison.Ordinal)));
                    var distinct = usual != null && holders.Any(h => !string.Equals(h.Content, usual, StringComparison.Ordinal));
                    var manual = holders.Any(h => h.Manual);
                    var states = new List<string>();
                    if (onFloor) states.Add("en el piso, sin larguero");
                    if (modified) states.Add("modificada en el borrador");
                    else if (distinct) states.Add("distinta del resto");
                    if (manual) states.Add("con un valor manual");
                    if (holders.Any(h => h.Unused)) states.Add("con un largo manual que no cambia el dibujo");
                    states.AddRange(SafetyStates(tope, parrilla));
                    var accessible = position + ": " + (varies ? "la tarima varía entre los fondos" : text) + (states.Count > 0 ? "; " + string.Join(", ", states) : string.Empty);
                    item.Set(text, detail, _showDetail, true, selected, selected && _primary == at[0], onFloor, varies, accessible, modified, distinct, manual);
                }
            }

            // The automatic values beside the manual ones, resolved once for the selection (the cells and their fronts).
            _automatic = _editor.AutomaticValues(_selection, SelectedFronts);
            if (Merge(ref _cellFields, BuildCellFields(), nameof(CellFields)))
            {
                _palletFields = _cellFields.Where(f => new[] { "cFront", "cHeight", "cCount" }.Contains(KeyOf(f))).ToList();
                _beamFields = _cellFields.Where(f => new[] { "cBeam", "cPeralte" }.Contains(KeyOf(f))).ToList();
                _manualFields = _cellFields.Where(f => new[] { "cLength", "cClear" }.Contains(KeyOf(f))).ToList();
                OnPropertyChanged(nameof(PalletFields));
                OnPropertyChanged(nameof(BeamFields));
                OnPropertyChanged(nameof(ManualFields));
            }

            MergeFields(ref _frontFields, BuildFrontFields(), nameof(FrontFields));
            _postHeights = BuildPostHeights();
            foreach (var option in _scopes)
            {
                var label = option.Value == (int)SelectiveEditScope.Cell && _scope == SelectiveEditScope.Cell && _selection.Count > 1
                    ? "Celdas (" + _selection.Count.ToString(CultureInfo.InvariantCulture) + ")"
                    : option.Value == (int)SelectiveEditScope.Cell ? "Celda" : option.Label;
                option.Set(label, option.Value == (int)_scope);
            }

            RaiseFloorBeam();
            OnPropertyChanged(nameof(PostHeights));
            OnPropertyChanged(nameof(SelectedPositions));
            OnPropertyChanged(nameof(SelectedFronts));
            OnPropertyChanged(nameof(SelectedLevels));
            OnPropertyChanged(nameof(PrimaryPosition));
            OnPropertyChanged(nameof(Scope));
            OnPropertyChanged(nameof(InspectorTitle));
            OnPropertyChanged(nameof(TargetText));
            OnPropertyChanged(nameof(InspectorNote));
            OnPropertyChanged(nameof(BeamSummary));
            OnPropertyChanged(nameof(ManualSummary));
            OnPropertyChanged(nameof(FrontTitle));
            OnPropertyChanged(nameof(FrontSummary));
            OnSafetyChanged(); // «Extremos y seguridad» (entrega 2): T/P of the selection and the table by post
            RebuildSegments();
        }

        // The tramos of the inspector follow the front and the scope; while they are being written (not applied yet) a recomposition of
        // another value keeps them. Another front, or other fondos of the view, read them again from the draft: tramos written and not
        // applied are dropped, and the gesture says so in the line of messages (S-21).
        private void RebuildSegments(bool announceDrop = true)
        {
            var front = SegmentFront;
            var key = front.HasValue ? (front.Value, _editor.FondoScope) : ((int, int?)?)null;
            if (key != _segmentKey)
            {
                if (announceDrop && _segmentsPending && _segmentKey.HasValue)
                {
                    _droppedSegments = "Los tramos sin aplicar de F" + (_segmentKey.Value.Front + 1).ToString(CultureInfo.InvariantCulture)
                                       + " se descartaron: se eligió otro frente u otros fondos de la vista.";
                }

                _segmentKey = key;
                ReplaceSegmentRows(front.HasValue ? RowsOf(StoredSegments(front.Value, _editor.FondoScope)) : new List<SelectiveSegmentRow>());
                _segmentError = string.Empty;
                _segmentHint = string.Empty; // T4 ins
                RefreshDirty();
            }

            RaiseSegments();
        }

        // The rows of the inspector are replaced as a whole; a keystroke in one of them measures again whether they are pending.
        private void ReplaceSegmentRows(List<SelectiveSegmentRow> rows)
        {
            foreach (var row in _segmentRows) row.PropertyChanged -= OnSegmentRowChanged;
            _segmentRows = rows ?? new List<SelectiveSegmentRow>();
            foreach (var row in _segmentRows) row.PropertyChanged += OnSegmentRowChanged;
            UpdateRestText();
        }

        private void OnSegmentRowChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectiveSegmentRow.RestText)) return;
            UpdateRestText();
            RefreshDirty();
            OnPropertyChanged(nameof(HasPendingSegments));
            OnPropertyChanged(nameof(CanDiscardSegments));
            SegmentGesture(string.Empty); // T4 ins: a length or «con largueros» written; the strip and the fit follow
            RaiseSegmentsIns();
        }

        // «(resto) ≈ n in» of the last tramo (the approved draft): the width of the front less the lengths written above it; T4 ins (DG-10):
        // and less what the intermediate posts take, as the resolver draws it (RestTextOf).
        private void UpdateRestText()
        {
            var rest = _segmentRows.LastOrDefault(r => r.IsRest);
            if (rest == null) return;
            rest.RestText = RestTextOf();
        }

        private void RaiseSegments()
        {
            OnPropertyChanged(nameof(SegmentFront));
            OnPropertyChanged(nameof(SegmentRows));
            OnPropertyChanged(nameof(HasSegmentRows));
            OnPropertyChanged(nameof(CanEditSegments));
            OnPropertyChanged(nameof(CanStartSegments));
            OnPropertyChanged(nameof(CanChangeSegments));
            OnPropertyChanged(nameof(HasPendingSegments));
            OnPropertyChanged(nameof(CanDiscardSegments));
            OnPropertyChanged(nameof(SegmentNote));
            OnPropertyChanged(nameof(SegmentError));
            RaiseSegmentsIns(); // T4 ins
        }

        private void SetSegmentError(string text)
        {
            _segmentError = text ?? string.Empty;
            OnPropertyChanged(nameof(SegmentError));
        }

        // The tramos of the front in the first fondo of that scope that has it (when the fondos differ, the note says so).
        private IReadOnlyList<SelectiveSegment> StoredSegments(int front, int? scope)
            => (scope is int fondo ? new[] { fondo } : Enumerable.Range(0, _editor.FondoCount))
                   .Select(k => _editor.Front(k, front)).FirstOrDefault(f => f != null)?.Segments
               ?? (IReadOnlyList<SelectiveSegment>)Array.Empty<SelectiveSegment>();

        // The rows as the inspector shows them: a length with up to three decimals (or the text as written, when it is not one), and
        // whether the tramo carries largueros.
        private static string RowsKey(IEnumerable<SelectiveSegmentRow> rows)
            => string.Join(";", rows.Select(row =>
            {
                var text = (row.LengthText ?? string.Empty).Trim();
                var length = row.IsRest ? "resto" : UiSupport.TryNum(text, out var value) && value > 0.0 ? Num(value) : text;
                return length + (row.Loaded ? "+" : "-");
            }));

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

        // True when the list was replaced (its fields changed), false when its items were refreshed in place.
        private bool Merge(ref IReadOnlyList<object> current, IReadOnlyList<object> next, string propertyName)
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

                return false;
            }

            current = next;
            OnPropertyChanged(propertyName);
            return true;
        }

        // The rows of «Fondos» are refreshed in place while the fondos are the same; another count of fondos gives new rows.
        private void MergeFondoRows(IReadOnlyList<SelectiveFondoRow> next)
        {
            if (_fondoRows.Count == next.Count)
            {
                for (var i = 0; i < next.Count; i++) _fondoRows[i].Refresh(next[i]);
                return;
            }

            _fondoRows = next;
            OnPropertyChanged(nameof(FondoRows));
            OnPropertyChanged(nameof(SeparatorFields));
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

        // «Lo esencial» of the approved draft: the WHOLE rack, in every fondo, whatever the fondos of the view are (WholeRack: the same write
        // as «Todo» of the matrix with «Todos los fondos»). The holguras governed by a project variable or a formula are read only, with
        // their effective value and the reason, and with no «=» (decision 12).
        private IReadOnlyList<object> BuildEssentials()
        {
            var fondos = HasFondos;
            var fronts = _editor.WithinScope(null, () => _editor.ReadFondos(f => f.FrontCount));
            var depth = _editor.WithinScope(null, () => _editor.ReadFondos(f => f.PalletDepth));
            var all = _editor.WithinScope(null, _editor.ScopePositions);
            var allFronts = Enumerable.Range(0, _editor.WithinScope(null, () => _editor.ScopeFrontCount)).ToList();
            var levels = _editor.WithinScope(null, () => _editor.ReadFronts(allFronts, f => f.LevelCount));
            var width = _editor.WithinScope(null, () => _editor.ReadCells(all, c => c.Frente));
            var height = _editor.WithinScope(null, () => _editor.ReadCells(all, c => c.Alto));
            var count = _editor.WithinScope(null, () => _editor.ReadCells(all, c => c.PalletCount));
            var beam = _editor.WithinScope(null, () => BeamChoice("rBeam", all, onFloor: false, InWholeRack));
            var peralte = _editor.WithinScope(null, () => PeralteChoice("rPeralte", all, onFloor: false, "Los que admite el larguero.", InWholeRack));

            var list = new List<object>
            {
                WholeInt("fronts", "Frentes (bahías)", fronts, fondos ? "En todos los fondos." : "Columnas de la matriz.",
                    "La cantidad de frentes debe ser un entero mayor o igual que 1.", v => _editor.SetFrontCount(v), "los frentes"),
                WholeInt("rLevels", "Niveles", levels, "Cantidad; N1 es la tarima en el piso.",
                    "El número de niveles debe ser un entero mayor o igual que 1.", v => _editor.SetLevelCount(allFronts, v)),
                new DynamicFieldItem("fondos", "Fondos", string.Empty, _editor.FondoCount.ToString(CultureInfo.InvariantCulture), "1 = sencillo; de 2 a 4, doble profundidad.", text =>
                {
                    if (!UiSupport.TryInt(text, out var value) || value < 1 || value > SelectiveRackDefaults.MaxDepthCount)
                        return "El número de fondos debe ser un entero de 1 a " + SelectiveRackDefaults.MaxDepthCount.ToString(CultureInfo.InvariantCulture) + ".";
                    return Apply(() => _editor.SetFondoCount(value), "los fondos");
                }, !CanEdit),
                WholeInt("rCount", "Tarimas por nivel", count, "Lado a lado en cada nivel.",
                    "Las tarimas por nivel deben ser un entero mayor o igual que 1.", v => _editor.SetPalletCount(all, v)),
                WholeNumber("rWidth", "Ancho", width, "Hoy «Frente tarima».", "El ancho de tarima debe ser un número mayor que cero.", v => _editor.SetPalletFront(all, v)),
                WholeNumber("rHeight", "Alto", height, string.Empty, "El alto de tarima debe ser un número mayor que cero.", v => _editor.SetPalletHeight(all, v)),
                WholeNumber("depth", "Profundidad", depth, "Hoy «Fondo de tarima».", "La profundidad de tarima debe ser un número mayor que cero.", v => _editor.SetPalletDepth(v)),
            };

            // With one fondo there is no table «Fondos»: the depth of the cabecera stays here, so a rack of one fondo can still have its own.
            if (!fondos)
            {
                var fondo = _editor.Fondo(0);
                var own = fondo?.CabeceraDepthOverride;
                list.Add(new DynamicFieldItem("cabecera", "Profundidad de cabecera", "in", own.HasValue ? Num(own.Value) : string.Empty,
                    own.HasValue ? "Vacía = la de la tarima − 6 in." : "Auto: " + Num(fondo?.CabeceraDepth ?? 0.0) + " in (la de la tarima − 6 in).", text =>
                    {
                        if (string.IsNullOrWhiteSpace(text)) return Apply(() => _editor.SetCabeceraDepth(null));
                        if (!UiSupport.TryNum(text, out var value) || value <= 0.0) return "La profundidad de cabecera debe ser un número mayor que cero, o vacía.";
                        return Apply(() => _editor.SetCabeceraDepth(value));
                    }, !CanEdit));
            }

            list.Add(beam);
            list.Add(peralte);
            list.Add(Holgura("tolerance", "Holgura horizontal", "A los lados de cada tarima.", Tolerance, _editor.PalletTolerance, v => _editor.PalletTolerance = v));
            list.Add(Holgura("clearance", "Holgura vertical", "Sobre cada tarima.", Clearance, _editor.VerticalClearance, v => _editor.VerticalClearance = v));
            list.Add(RackPeralteEssential()); // T5 pp: «Poste» › «Peralte del poste», para todos los postes
            return list;
        }

        // The table «Fondos» (two or more fondos; the approved draft): each row writes in its own fondo only.
        private IReadOnlyList<SelectiveFondoRow> BuildFondoRows()
        {
            if (!HasFondos) return Array.Empty<SelectiveFondoRow>();
            var separators = _editor.Separators;
            var rows = new List<SelectiveFondoRow>();
            for (var k = 0; k < _editor.FondoCount; k++)
            {
                var fondo = k;
                var data = _editor.Fondo(fondo);
                var name = "Fondo " + (fondo + 1).ToString(CultureInfo.InvariantCulture);
                var fronts = new DynamicFieldItem("fondo" + fondo + "Fronts", name + ": frentes", string.Empty, data.FrontCount.ToString(CultureInfo.InvariantCulture), string.Empty, text =>
                {
                    if (!UiSupport.TryInt(text, out var value) || value < 1) return "La cantidad de frentes debe ser un entero mayor o igual que 1.";
                    return Apply(() => _editor.WithinScope(fondo, () => _editor.SetFrontCount(value)), "los frentes");
                }, !CanEdit);
                var depth = new DynamicFieldItem("fondo" + fondo + "Depth", name + ": profundidad de tarima", "in", Num(data.PalletDepth), string.Empty, text =>
                {
                    if (!UiSupport.TryNum(text, out var value) || value <= 0.0) return "La profundidad de tarima debe ser un número mayor que cero.";
                    return Apply(() => _editor.WithinScope(fondo, () => _editor.SetPalletDepth(value)));
                }, !CanEdit);
                var cabecera = new DynamicFieldItem("fondo" + fondo + "Cabecera", name + ": profundidad de cabecera", "in",
                    data.CabeceraDepthOverride.HasValue ? Num(data.CabeceraDepthOverride.Value) : string.Empty,
                    data.CabeceraDepthOverride.HasValue ? string.Empty : "Auto: " + Num(data.CabeceraDepth), text =>
                    {
                        if (string.IsNullOrWhiteSpace(text)) return Apply(() => _editor.WithinScope(fondo, () => _editor.SetCabeceraDepth(null)));
                        if (!UiSupport.TryNum(text, out var value) || value <= 0.0) return "La profundidad de cabecera debe ser un número mayor que cero, o vacía.";
                        return Apply(() => _editor.WithinScope(fondo, () => _editor.SetCabeceraDepth(value)));
                    }, !CanEdit);
                var separator = fondo < separators.Count
                    ? Number("sep" + fondo.ToString(CultureInfo.InvariantCulture), name + ": separación al fondo siguiente", "in", separators[fondo], v => v > 0.0,
                        "La separación debe ser un número mayor que cero.", v => _editor.SetSeparator(fondo, v))
                    : null;
                rows.Add(new SelectiveFondoRow(fondo, fronts, depth, cabecera, separator));
            }

            return rows;
        }

        private IReadOnlyList<object> BuildAnnotationFields() => new object[]
        {
            new SelectiveChoiceItem("post", "Poste", _editor.PostChoices.Select(p => new SelectiveChoiceOption(p.Id.Trim(), Label(p.Label, p.Id))).ToList(),
                _editor.PostId?.Trim(), string.Empty, value => Apply(() =>
                {
                    _editor.PostId = value;
                    return Done;
                }), !CanEdit),
            // T5 pp: «Peralte del poste» is in «Lo esencial» › «Poste» now (RackPeralteEssential), seen without unfolding this expander.
            Number("scale", "Escala de texto", string.Empty, _editor.AnnotationScale, v => v > 0.0, "La escala de texto debe ser mayor que cero.", v =>
            {
                _editor.AnnotationScale = v;
                return Done;
            }, "1 = texto de 6 in. Un valor no válido se marca; ya no pasa a 1 sin avisar."),
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
            // S-27 (M-10): the styles of the drawing, as the list of the classic editor: «(Automático)» first, and the style the rack was opened
            // with kept even when the drawing does not have it, so it is written back as it was read.
            new SelectiveChoiceItem("dimStyle", "Estilo de cota", StyleOptions(), NamedStyle(_editor.DimensionStyle) ?? AutomaticStyle,
                "O un estilo del dibujo: la ventana siempre los lee.",
                value => Apply(() =>
                {
                    _editor.DimensionStyle = value == AutomaticStyle ? null : value;
                    return Done;
                }), !CanEdit),
        };

        // «(Automático)», the styles of the drawing, then the style the rack opened with and the current one, when the drawing lacks them.
        private IReadOnlyList<SelectiveChoiceOption> StyleOptions()
        {
            var names = new List<string> { AutomaticStyle };
            foreach (var name in _drawingStyles.Concat(new[] { _loadedStyle, NamedStyle(_editor.DimensionStyle) }))
            {
                if (!string.IsNullOrEmpty(name) && !names.Contains(name, StringComparer.Ordinal)) names.Add(name);
            }

            return names.Select(name => new SelectiveChoiceOption(name, name)).ToList();
        }

        // A style with a name, trimmed; null for the automatic one (blank or «(Automático)»), as the draft writes it.
        private static string NamedStyle(string style)
        {
            var trimmed = style?.Trim();
            return string.IsNullOrEmpty(trimmed) || trimmed == AutomaticStyle ? null : trimmed;
        }

        // The inspector of the cells: the pallet, the larguero and its peralte (only those the catalog allows for the larguero of every cell,
        // as the list of the classic editor), and the manual values.
        private IReadOnlyList<object> BuildCellFields()
        {
            var cells = _selection.ToList();

            // S-23 (M-07): the whole selection is N1 on the floor, with no larguero in any fondo of the view; the larguero, its peralte and the
            // clear are not drawn there, so they are not offered, with the texts of the approved draft. What they hold is kept.
            var onFloor = cells.Count > 0 && cells.All(p => HasNoLarguero(p.FrontIndex, p.LevelIndex));

            // «Automático: …» (the approved draft): the length the larguero of those cells would have with their manual length emptied.
            var automatic = _automatic.BeamLengths;
            var length = _editor.ReadCells(cells, c => c.BeamLength);
            var lengthHint = length.Varies
                ? Varies
                : (length.HasValue && length.Value.HasValue
                      ? "Vacío = automático" + (automatic.Count == 1 ? " (" + Num(automatic[0]) + " in)" : string.Empty) + "."
                      : "Automático" + (automatic.Count == 1 ? ": " + Num(automatic[0]) + " in" : string.Empty) + ".")
                  + WidthRule(cells, length.HasValue && length.Value.HasValue);

            return new object[]
            {
                CellNumber("cFront", "Ancho de tarima", "in", cells, c => c.Frente, (p, v) => _editor.SetPalletFront(p, v), "El ancho de tarima debe ser un número mayor que cero."),
                CellNumber("cHeight", "Alto de tarima", "in", cells, c => c.Alto, (p, v) => _editor.SetPalletHeight(p, v), "El alto de tarima debe ser un número mayor que cero."),
                CellInt("cCount", "Tarimas por nivel", cells),
                BeamChoice("cBeam", cells, onFloor, write => write()),
                PeralteChoice("cPeralte", cells, onFloor, string.Empty, write => write()),
                CellOptional("cLength", "Largo de larguero", cells, c => c.BeamLength, (p, v) => _editor.SetBeamLengthOverride(p, v),
                    "El largo del larguero debe ser un número mayor que cero, o vacío para automático.", hint: lengthHint),
                CellOptional("cClear", "Claro debajo", cells, c => c.Clear, (p, v) => _editor.SetClearOverride(p, v),
                    "El claro debe ser un número mayor que cero, o vacío para automático.", onFloor ? NotInN1Text : null,
                    "Vacío = automático. Hasta el larguero de abajo (en N2, desde el piso); al troquel de 2 in."),
            };
        }

        // Who fixes the width of the fronts of those cells (M-02 of the review of the polish): the widest level of the front in the width master
        // (SelectiveDraftEditor.WidthMasterFondo, the fondo with the most fronts). In another fondo the master governs: a manual length there is
        // kept and written, but it does not change the drawing, and the hint says so instead of «El mayor del frente fija su ancho».
        private string WidthRule(IReadOnlyList<SelectiveMatrixPosition> cells, bool manual)
        {
            var master = _editor.WidthMasterFondo;
            var reached = _editor.ScopeFondos.Where(k => cells.Any(p => p.LevelIndex >= 0 && p.LevelIndex < _editor.LevelCount(k, p.FrontIndex))).ToList();
            var others = reached.Where(k => k != master).ToList();
            if (others.Count == 0) return " El mayor del frente fija su ancho.";
            var masterName = "fondo " + (master + 1).ToString(CultureInfo.InvariantCulture) + " (el maestro, el de más frentes)";
            if (!reached.Contains(master))
                return " En " + FondoList(others) + " manda el ancho del " + masterName + (manual ? ": este largo no cambia el dibujo." : ".");
            return " Manda el mayor del frente en el " + masterName + (manual ? "; en " + FondoList(others) + " el largo manual no cambia el dibujo." : ".");
        }

        // The larguero of those cells, over the fondos of the scope; the write goes through «through» (the scope of the field).
        private SelectiveChoiceItem BeamChoice(string key, IReadOnlyList<SelectiveMatrixPosition> cells, bool onFloor, Func<Func<SelectiveDraftEdit>, SelectiveDraftEdit> through)
        {
            var beam = _editor.ReadCells(cells, c => c.BeamId?.Trim(), StringComparer.OrdinalIgnoreCase);
            var beamOptions = _editor.BeamChoices.Select(b => new SelectiveChoiceOption(b.Id.Trim(), Label(b.Label, b.Id))).ToList();
            var known = beam.HasValue ? beamOptions.FirstOrDefault(o => string.Equals(o.Value, beam.Value, StringComparison.OrdinalIgnoreCase))?.Value : null;
            return new SelectiveChoiceItem(key, "Larguero", beamOptions, known, onFloor ? NoBeamText : beam.Varies ? Varies : string.Empty,
                value => Apply(() => through(() => _editor.SetBeam(cells, value))), !CanEdit || beam.Count == 0 || onFloor);
        }

        // The peralte of the larguero of those cells: only those the catalog allows for the larguero of every cell, as the classic list.
        private SelectiveChoiceItem PeralteChoice(string key, IReadOnlyList<SelectiveMatrixPosition> cells, bool onFloor, string hint,
            Func<Func<SelectiveDraftEdit>, SelectiveDraftEdit> through)
        {
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
            return new SelectiveChoiceItem(key, "Peralte del larguero", peralteOptions, peralte.HasValue ? Num(peralte.Value) : null,
                onFloor ? NoBeamText
                    : peralte.Varies ? Varies : peralteOptions.Count == 0 ? "El catálogo no tiene un peralte común para estos largueros." : hint,
                value =>
                {
                    if (!UiSupport.TryNum(value, out var chosen)) return "Elige un peralte del catálogo.";
                    return Apply(() => through(() => _editor.SetBeamPeralte(cells, chosen)));
                }, !CanEdit || peralte.Count == 0 || peralteOptions.Count == 0 || onFloor);
        }

        // A write of the «Rack» tab: the whole rack, in every fondo, whatever the fondos of the view are (the same write as «Todo» of the
        // matrix with «Todos los fondos»). Every cell or front that exists is written, so nothing is «omitted» to say.
        private SelectiveDraftEdit InWholeRack(Func<SelectiveDraftEdit> write)
        {
            var edit = _editor.WithinScope(null, write) ?? Done;
            return edit.Refused ? edit : edit with { Omitted = 0 };
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

            // «Calculada: …» (the approved draft): the height those fronts would have with their manual height emptied. T4 seg, SD-14 of the
            // Owner («Quitarlo»): no «≈», the value is the one of the resolver.
            var calculated = _automatic.Heights;
            var heightHint = height.Varies
                ? Varies
                : (height.HasValue && height.Value.HasValue
                      ? "Vacía = calculada" + (calculated.Count == 1 ? " (" + Num(calculated[0]) + " in)" : string.Empty) + "."
                      : "Calculada" + (calculated.Count == 1 ? ": " + Num(calculated[0]) + " in" : string.Empty) + ".")
                  + " Manda en los dos postes del frente.";
            return new[]
            {
                new DynamicFieldItem("fLevels", "Niveles", string.Empty, levels.HasValue ? levels.Value.ToString(CultureInfo.InvariantCulture) : string.Empty,
                    levels.Varies ? Varies : "Cantidad exacta; N1 es el piso.", text =>
                    {
                        if (!UiSupport.TryInt(text, out var count) || count < 1) return "El número de niveles debe ser un entero mayor o igual que 1.";
                        return Apply(() => _editor.SetLevelCount(fronts, count));
                    }, !CanEdit || levels.Count == 0),
                new DynamicFieldItem("fRise", "Elevación del larguero a piso", "in", rise.HasValue ? Num(rise.Value) : string.Empty,
                    rise.Varies ? Varies
                        : anyFloor ? "Sobre el primer troquel; un frente nuevo copia la del último."
                        : "Solo con «Larguero a piso».", text =>
                    {
                        if (!UiSupport.TryNum(text, out var value) || value < 0.0) return "La elevación del larguero a piso debe ser un número mayor o igual que cero.";
                        return Apply(() => _editor.SetFloorBeamRise(fronts, value));
                    }, !CanEdit || rise.Count == 0 || !anyFloor),
                new DynamicFieldItem("fHeight", "Altura manual de cabecera", "in", height.HasValue && height.Value.HasValue ? Num(height.Value.Value) : string.Empty,
                    heightHint, text =>
                    {
                        if (string.IsNullOrWhiteSpace(text)) return Apply(() => _editor.SetHeightOverride(fronts, null));
                        if (!UiSupport.TryNum(text, out var value) || value <= 0.0) return "La altura manual debe ser un número mayor que cero, o vacía para calculada.";
                        return Apply(() => _editor.SetHeightOverride(fronts, value));
                    }, !CanEdit || height.Count == 0),
            };
        }

        // «Avanzado»: the own peralte of each post (empty = the one of the rack, the rule of the classic box), its height and its cabecera by
        // fondo (the table of the approved draft). The name of each box carries its post, so a reader of the screen tells them apart.
        private IReadOnlyList<SelectivePostRow> BuildPostRows()
        {
            var rows = new List<SelectivePostRow>();
            for (var p = 0; p < _editor.PostCount; p++)
            {
                var post = p;
                var own = _editor.PostPeralteOverride(post);
                var name = "P" + (post + 1).ToString(CultureInfo.InvariantCulture);
                var field = new DynamicFieldItem("post" + post.ToString(CultureInfo.InvariantCulture), name + ": peralte propio", "in", own.HasValue ? Num(own.Value) : string.Empty,
                    own.HasValue ? string.Empty : "Del rack: " + Num(_editor.PostPeralte), text =>
                    {
                        if (string.IsNullOrWhiteSpace(text)) return Apply(() => _editor.SetPostPeralte(post, null));
                        if (!UiSupport.TryNum(text, out var value) || value <= 0.0) return "Peralte de poste inválido (deja vacío para usar el peralte del rack).";
                        return Apply(() => _editor.SetPostPeralte(post, value));
                    }, !CanEdit);
                var reach = Enumerable.Range(0, _editor.FondoCount).Where(k => post <= _editor.FrontCount(k)).ToList();
                var cabeceras = reach.Select(k => (_editor.FondoCount > 1 ? "Fondo " + (k + 1) + ": " : string.Empty) + (_editor.HasCustomCabecera(k, post) ? "personalizada" : "calculada"));
                var heights = string.Join(" / ", reach.Select(k => PostHeight(k, post)).Where(h => h > 0.0).Select(h => Num(h) + " in")); // T4 seg, SD-14: exact, no «≈»
                rows.Add(new SelectivePostRow(post, field, "Cabecera " + string.Join(" · ", cabeceras), heights));
            }

            return rows;
        }

        // The height of a post of a fondo in the last valid system, as the drawing gives it: its custom cabecera, or the tallest front it
        // touches (SelectivePostGeometry, the rule of the drawing and of the BOM). 0 when there is no system or the fondo does not reach it.
        private double PostHeight(int fondo, int post)
        {
            var system = _editor.System;
            if (system == null) return 0.0;
            var bays = SelectiveDepthLayout.BaysOfFondo(system, fondo);
            if (bays == null || post < 0 || post > bays.Count) return 0.0;
            var row = fondo == 0 ? system.PostCabeceras : fondo - 1 < system.ExtraFondoPostCabeceras.Count ? system.ExtraFondoPostCabeceras[fondo - 1] : null;
            var custom = row != null && post < row.Count ? row[post] : null;
            return custom != null && custom.Height > 0.0 ? custom.Height : SelectivePostGeometry.PostHeight(bays, post, SelectivePostGeometry.FallbackHeight(bays, system.Height));
        }

        // «Altura por poste» under the matrix (the approved draft): the posts of each fondo of the view, marked when a manual height of one of
        // the two fronts it touches governs it.
        private IReadOnlyList<SelectivePostHeightBlock> BuildPostHeights()
        {
            var blocks = new List<SelectivePostHeightBlock>();
            if (_editor.System == null) return blocks;
            foreach (var fondo in _editor.ScopeFondos)
            {
                var fronts = _editor.FrontCount(fondo);
                var of = HasFondos ? " del fondo " + (fondo + 1).ToString(CultureInfo.InvariantCulture) : string.Empty;
                var chips = new List<SelectivePostHeightChip>();
                for (var post = 0; post <= fronts; post++)
                {
                    var height = PostHeight(fondo, post);
                    var manual = new[] { post - 1, post }.Any(f => f >= 0 && f < fronts && _editor.Front(fondo, f)?.HeightOverride.HasValue == true);
                    var custom = _editor.HasCustomCabecera(fondo, post);
                    var id = "P" + (post + 1).ToString(CultureInfo.InvariantCulture);
                    var value = Num(height) + " in"; // T4 seg, SD-14: exact, no «≈»
                    chips.Add(new SelectivePostHeightChip(id, value, manual,
                        "Poste " + (post + 1).ToString(CultureInfo.InvariantCulture) + of + ": " + value
                        + (manual ? ", con una altura manual" : string.Empty) + (custom ? ", cabecera personalizada" : string.Empty)));
                }

                blocks.Add(new SelectivePostHeightBlock("Fondo " + (fondo + 1).ToString(CultureInfo.InvariantCulture), HasFondos, "Altura de cada poste" + of, chips));
            }

            return blocks;
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

        // «Calculado» (the approved draft), from the last valid recomposition, read only: the larguero length (one value or the range), the
        // height of the cabecera, the separation N2 → N3, the depth of the cabecera of fondo 1, and the counts. Cabeceras, largueros and
        // separators come from the BOM (SelectiveDraftEditor.BomCounts: no count is made again here); the pallet positions are the cells
        // times their pallets per level, in every fondo.
        private IReadOnlyList<SelectiveCalcItem> BuildCalculated()
        {
            var system = _editor.System;
            if (system == null) return new[] { new SelectiveCalcItem("Calculado", "—") };
            var items = new List<SelectiveCalcItem>();
            var fondos = SelectiveDepthLayout.Count(system);
            var several = fondos > 1;
            try
            {
                var master = SelectiveDepthLayout.BaysOfFondo(system, SelectiveDepthLayout.MasterFondoIndex(system)) ?? new List<SelectiveBay>();
                items.Add(new SelectiveCalcItem("Largo del larguero", Range(master.Select(b => b.BeamLength).Where(v => v > 0.0)),
                    "Ancho × tarimas + holgura × (tarimas + 1). En cada frente manda el nivel más ancho."));
                items.Add(new SelectiveCalcItem("Altura de cabecera", Num(system.Height) + " in", /* T4 seg, SD-14 */ "La del frente más alto, al pie entero."));

                // N2 → N3: the separation between the larguero of N2 and the one of N3 of every front that has them, in every fondo.
                var separations = new List<double>();
                for (var k = 0; k < fondos; k++)
                {
                    var bays = SelectiveDepthLayout.BaysOfFondo(system, k) ?? new List<SelectiveBay>();
                    for (var f = 0; f < bays.Count; f++)
                    {
                        var front = _editor.Front(k, f);
                        if (front == null || front.LevelCount < 3) continue;
                        var offset = front.FloorBeam ? 0 : 1; // with no «Larguero a piso», N1 has no larguero: the first one is N2
                        var levels = bays[f].Levels;
                        if (2 - offset < levels.Count) separations.Add(levels[2 - offset].Y - levels[1 - offset].Y);
                    }
                }

                items.Add(new SelectiveCalcItem("Separación N2 → N3", Range(separations), "Alto + holgura, al par, más el peralte; al troquel de 2 in."));
                var own = _editor.Fondo(0)?.CabeceraDepthOverride;
                items.Add(new SelectiveCalcItem("Profundidad de cabecera", Num(SelectiveDepthLayout.CabeceraDepthOfFondo(system, 0)) + " in",
                    (own.HasValue ? "Manual" : "Tarima − 6 in") + (several ? ", en el fondo 1." : ".")));

                // On a system of its own (I-01 of the review of the polish): the BOM imposes the depth of its fondo on the custom cabeceras it
                // reads; the system of the draft no longer shares them with its design (I-1 of the review of the entrega 2), and stays as it is.
                var counts = _editor.BomCounts();
                var split = Enumerable.Range(0, _editor.FondoCount).Any(k => Enumerable.Range(0, _editor.FrontCount(k)).Any(f => _editor.Front(k, f)?.IsSplit == true));
                items.Add(new SelectiveCalcItem("Cabeceras", counts.Cabeceras.ToString(CultureInfo.InvariantCulture),
                    (several ? "En los " + fondos.ToString(CultureInfo.InvariantCulture) + " fondos" : "Una por poste") + (split ? ", más las de medio frente." : ".")));
                items.Add(new SelectiveCalcItem("Largueros", counts.Largueros.ToString(CultureInfo.InvariantCulture), "Dos por nivel con larguero; N1 va en el piso."));
                var positions = 0;
                for (var k = 0; k < _editor.FondoCount; k++)
                    for (var f = 0; f < _editor.FrontCount(k); f++)
                        for (var l = 0; l < _editor.LevelCount(k, f); l++)
                            positions += Math.Max(1, _editor.Cell(k, f, l)?.PalletCount ?? 1);
                items.Add(new SelectiveCalcItem("Posiciones de tarima", positions.ToString(CultureInfo.InvariantCulture),
                    "Celdas × tarimas por nivel" + (several ? ", en todos los fondos." : ".")));
                if (several)
                    items.Add(new SelectiveCalcItem("Separadores", counts.Separadores.ToString(CultureInfo.InvariantCulture), "Entre fondos: 2 o más por poste, uno más cada 100 in de altura."));
            }
            catch (Exception ex)
            {
                items.Add(new SelectiveCalcItem("No se pudo calcular", ex.Message));
            }

            return items;
        }

        // One value, or the range «a–b», in inches; «—» for none.
        private static string Range(IEnumerable<double> values)
        {
            var distinct = values.Select(v => Math.Round(v, 2)).Distinct().OrderBy(v => v).ToList();
            if (distinct.Count == 0) return "—";
            return (distinct.Count == 1 ? Num(distinct[0]) : Num(distinct[0]) + "–" + Num(distinct[distinct.Count - 1])) + " in";
        }

        private DynamicFieldItem Number(string key, string label, string unit, double current, Func<double, bool> valid, string invalid, Func<double, SelectiveDraftEdit> set,
            string hint = "") =>
            new DynamicFieldItem(key, label, unit, Num(current), hint, text =>
            {
                if (!UiSupport.TryNum(text, out var value) || !valid(value)) return invalid;
                return Apply(() => set(value));
            }, !CanEdit);

        // A whole number of the WHOLE rack («Rack» › «Lo esencial»): what every fondo has, «Varía» when they differ.
        private DynamicFieldItem WholeInt(string key, string label, SelectiveScopedValue<int> current, string hint, string invalid, Func<int, SelectiveDraftEdit> set,
            string reduces = null) =>
            new DynamicFieldItem(key, label, string.Empty, current.HasValue ? current.Value.ToString(CultureInfo.InvariantCulture) : string.Empty,
                current.Varies ? Varies : hint, text =>
                {
                    if (!UiSupport.TryInt(text, out var value) || value < 1) return invalid;
                    return Apply(() => InWholeRack(() => set(value)), reduces);
                }, !CanEdit || current.Count == 0);

        // A measure of the WHOLE rack, in inches, greater than zero.
        private DynamicFieldItem WholeNumber(string key, string label, SelectiveScopedValue<double> current, string hint, string invalid, Func<double, SelectiveDraftEdit> set) =>
            new DynamicFieldItem(key, label, "in", current.HasValue ? Num(current.Value) : string.Empty, current.Varies ? Varies : hint, text =>
            {
                if (!UiSupport.TryNum(text, out var value) || !(value > 0.0)) return invalid;
                return Apply(() => InWholeRack(() => set(value)));
            }, !CanEdit || current.Count == 0);

        // A holgura: its value while it belongs to the rack; the effective value, read only, with the reason and the name of the variable while
        // a project variable or a formula governs it. No «=» anywhere (decision 12).
        private DynamicFieldItem Holgura(string key, string label, string hint, PropertyId property, double current, Action<double> set)
        {
            if (_editor.IsLinked(property))
                return new DynamicFieldItem(key, label, "in", Num(current), _editor.LinkedReadOnlyReason(property), null, isReadOnly: true);

            return Number(key, label, "in", current, v => v >= 0.0, label + ": debe ser un número mayor o igual que cero.", v =>
            {
                set(v);
                return Done;
            }, hint);
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

        // A manual value of the cells; with «locked», read only with that hint (S-23); «hint» says what an empty box means.
        private DynamicFieldItem CellOptional(string key, string label, IReadOnlyList<SelectiveMatrixPosition> cells, Func<SelectiveEditorCell, double?> read,
            Func<IReadOnlyList<SelectiveMatrixPosition>, double?, SelectiveDraftEdit> write, string invalid, string locked = null, string hint = null)
        {
            var value = _editor.ReadCells(cells, read);
            return new DynamicFieldItem(key, label, "in", value.HasValue && value.Value.HasValue ? Num(value.Value.Value) : string.Empty,
                locked ?? (value.Varies ? Varies : hint ?? string.Empty), text =>
                {
                    if (string.IsNullOrWhiteSpace(text)) return Apply(() => write(cells, null));
                    if (!UiSupport.TryNum(text, out var number) || number <= 0.0) return invalid;
                    return Apply(() => write(cells, number));
                }, !CanEdit || value.Count == 0 || locked != null);
        }

        // «Frente 1» / «Frentes 1, 2» (the approved draft).
        private static string ListText(IReadOnlyList<int> numbers, string one, string many)
            => (numbers.Count == 1 ? one : many) + " " + string.Join(", ", numbers.Select(n => n.ToString(CultureInfo.InvariantCulture)));

        // «1 y 2», «1, 2 y 3».
        private static string JoinAnd(IEnumerable<int> numbers)
        {
            var names = numbers.Select(n => n.ToString(CultureInfo.InvariantCulture)).ToList();
            return names.Count <= 1 ? string.Join(string.Empty, names) : string.Join(", ", names.Take(names.Count - 1)) + " y " + names[names.Count - 1];
        }

        // ------------------------------------------------------------------ internals: the matrix

        private bool Exists(int frontIndex, int levelIndex)
            => frontIndex >= 0 && levelIndex >= 0 && _editor.ScopeFondos.Any(k => levelIndex < _editor.LevelCount(k, frontIndex));

        // N1 of a front that some fondo of the scope has with no «Larguero a piso»: its pallet is on the floor, with no larguero.
        private bool IsOnFloor(int frontIndex, int levelIndex)
            => levelIndex == 0 && _editor.ScopeFondos.Select(k => _editor.Front(k, frontIndex)).Any(f => f != null && f.LevelCount > 0 && !f.FloorBeam);

        // N1 of a front that no fondo of the scope has with «Larguero a piso»: there is no larguero in that cell in any fondo of the view (S-23).
        private bool HasNoLarguero(int frontIndex, int levelIndex)
        {
            if (levelIndex != 0) return false;
            var fronts = _editor.ScopeFondos.Select(k => _editor.Front(k, frontIndex)).Where(f => f != null && f.LevelCount > 0).ToList();
            return fronts.Count > 0 && fronts.All(f => !f.FloorBeam);
        }

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
