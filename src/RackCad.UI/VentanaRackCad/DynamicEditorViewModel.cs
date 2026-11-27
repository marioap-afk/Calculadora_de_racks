using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Views;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// One editable value of the Dinámico editor: its label, unit and text, the hint when the selection has several values, and its error.
    /// The item lives as long as the editor: a recomposition refreshes it in place, so the box that has the focus is never replaced.
    /// </summary>
    public sealed class DynamicFieldItem : ObservableObject
    {
        private Func<string, string> _commit;
        private string _text;
        private string _shownText;
        private string _hint;
        private string _error = string.Empty;
        private bool _isReadOnly;
        private bool _committing;

        public DynamicFieldItem(string key, string label, string unit, string text, string hint, Func<string, string> commit, bool isReadOnly = false)
        {
            Key = key;
            Label = label;
            Unit = unit ?? string.Empty;
            _text = text ?? string.Empty;
            _shownText = _text;
            _hint = hint ?? string.Empty;
            _commit = commit;
            _isReadOnly = isReadOnly;
        }

        public string Key { get; }

        public string Label { get; }

        public string Unit { get; }

        /// <summary>«Varía» when the values of the selection differ and the box is empty; empty otherwise.</summary>
        public string Hint
        {
            get => _hint;
            private set => SetProperty(ref _hint, value ?? string.Empty);
        }

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value ?? string.Empty);
        }

        public string Error
        {
            get => _error;
            private set => SetProperty(ref _error, value ?? string.Empty);
        }

        /// <summary>The field cannot be written now: AutoCAD is writing the rack, or the selection has nothing that this field edits.</summary>
        public bool IsReadOnly
        {
            get => _isReadOnly;
            private set => SetProperty(ref _isReadOnly, value);
        }

        /// <summary>
        /// The gesture of the field (Enter or leaving it): the text goes to the draft, or the field says why not. Leaving a field whose text
        /// is the one it showed changes nothing (also an empty «Varía»): only a value that the user wrote reaches the draft.
        /// </summary>
        public void Commit()
        {
            if (_committing || _commit == null || _isReadOnly) return;
            if (string.Equals(_text.Trim(), _shownText.Trim(), StringComparison.Ordinal))
            {
                Error = string.Empty;
                return;
            }

            _committing = true;
            try
            {
                Error = _commit(_text) ?? string.Empty;
            }
            finally
            {
                _committing = false;
            }
        }

        // A recomposition refreshed the field: what it shows, what it writes, and whether it can be written. The error is cleared; the
        // commit that is running sets its own once it returns.
        internal void Refresh(DynamicFieldItem next)
        {
            _commit = next._commit;
            _shownText = next._text;
            Hint = next._hint;
            IsReadOnly = next._isReadOnly;
            Text = next._text;
            Error = string.Empty;
        }
    }

    /// <summary>One cell of the matrix: front × level, with one datum (the pallet, width × height) and its marks; refreshed in place.</summary>
    public sealed class DynamicCellItem : ObservableObject
    {
        private string _text = string.Empty;
        private bool _exists;
        private bool _isSelected;
        private bool _isPrimary;
        private bool _isBlankFront;
        private bool _hasGuide;
        private string _accessibleText = string.Empty;

        public DynamicCellItem(int frontIndex, int levelIndex)
        {
            FrontIndex = frontIndex;
            LevelIndex = levelIndex;
        }

        public int FrontIndex { get; }

        public int LevelIndex { get; }

        public string Text { get => _text; private set => SetProperty(ref _text, value); }

        public bool Exists { get => _exists; private set => SetProperty(ref _exists, value); }

        public bool IsSelected { get => _isSelected; private set => SetProperty(ref _isSelected, value); }

        public bool IsPrimary { get => _isPrimary; private set => SetProperty(ref _isPrimary, value); }

        public bool IsBlankFront { get => _isBlankFront; private set => SetProperty(ref _isBlankFront, value); }

        /// <summary>The cell carries an entrance guide in the drawing (decision 6 of the prototype: «guía de entrada marcada en la matriz»).</summary>
        public bool HasGuide { get => _hasGuide; private set => SetProperty(ref _hasGuide, value); }

        /// <summary>What a screen reader says of the cell: its datum and its guide.</summary>
        public string AccessibleText { get => _accessibleText; private set => SetProperty(ref _accessibleText, value); }

        internal void Set(string text, bool exists, bool isSelected, bool isPrimary, bool isBlankFront, bool hasGuide = false)
        {
            Text = text;
            Exists = exists;
            IsSelected = isSelected;
            IsPrimary = isPrimary;
            IsBlankFront = isBlankFront;
            HasGuide = hasGuide;
            AccessibleText = "F" + (FrontIndex + 1) + " · N" + (LevelIndex + 1) + ": " + text + (hasGuide ? ", con guía de entrada" : string.Empty);
        }
    }

    /// <summary>One post of «Seguridad por poste»: what the drawing places there, family by family, as the BOM counts it.</summary>
    public sealed class DynamicPostSafetyItem
    {
        internal DynamicPostSafetyItem(DynamicPostSafetyRow row)
        {
            Post = "P" + (row.PostIndex + 1);
            Boots = Count(row.Boots);
            SideGuards = Count(row.SideGuards);
            Deflectors = row.DeflectorsAtExit + row.DeflectorsAtEntrance == 0
                ? "—"
                : "salida " + row.DeflectorsAtExit.ToString(CultureInfo.InvariantCulture) + " · entrada " + row.DeflectorsAtEntrance.ToString(CultureInfo.InvariantCulture);
            Defenses = Count(row.Defenses);
            Guides = Count(row.EntranceGuides);
        }

        public string Post { get; }

        public string Boots { get; }

        public string SideGuards { get; }

        public string Deflectors { get; }

        public string Defenses { get; }

        public string Guides { get; }

        private static string Count(int value) => value == 0 ? "—" : value.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>One row of the matrix: «N3» … «N1», top to bottom.</summary>
    public sealed class DynamicMatrixRow
    {
        public DynamicMatrixRow(int levelIndex, string label, IReadOnlyList<DynamicCellItem> cells)
        {
            LevelIndex = levelIndex;
            Label = label;
            Cells = cells;
        }

        public int LevelIndex { get; }

        public string Label { get; }

        public IReadOnlyList<DynamicCellItem> Cells { get; }
    }

    /// <summary>One column head of the matrix: «F1» …, with its blank mark; refreshed in place.</summary>
    public sealed class DynamicColumnHead : ObservableObject
    {
        private bool _isBlank;

        public DynamicColumnHead(int frontIndex, string title)
        {
            FrontIndex = frontIndex;
            Title = title;
        }

        public int FrontIndex { get; }

        public string Title { get; }

        public bool IsBlank { get => _isBlank; internal set => SetProperty(ref _isBlank, value); }
    }

    /// <summary>One computed value of the rack.</summary>
    public sealed record DynamicCalcItem(string Label, string Value);

    /// <summary>One module of the sequence, with Spanish labels.</summary>
    public sealed record DynamicModuleItem(string Id, string Kind, string Length, string Start, string Manual);

    /// <summary>
    /// The editor of ONE Dinámico rack in the RackCad window (I-64 A-14, line D): the design the Owner approved («versión despejada»), with
    /// the tabs Rack, Matriz and Avanzado, over the same recomposition as the classic editor (<see cref="DynamicDraftEditor"/>).
    ///
    /// One way to confirm: what the user writes in a field goes to the draft when the field is left (or with Enter) and the rack is
    /// recomposed; leaving a field without changing its text changes nothing. One scope: the selection of the matrix (a click, Ctrl+click,
    /// Mayús+click from the anchor, a column, a row or «Todo»). A value that cannot be recomposed is refused in its field and the draft,
    /// its matrix and its selection stay as they were. The rack-wide fields of «Lo esencial» write every active cell or front of the rack
    /// and say «Varía» when they differ; a front in blank keeps its levels and cells untouched until it is active again, as in the
    /// classic editor. The draft is «Modificado» only while it differs from what was last read from the drawing or written to it.
    /// Nothing here writes the drawing: «Actualizar» hands the editor to the host, and while AutoCAD writes nothing can be edited.
    /// </summary>
    public sealed partial class DynamicEditorViewModel : ObservableObject // T4: partial (.Ins, .Library, .Properties, .Drafts, .Seg)
    {
        private const string Varies = "Varía";
        private const string WaitingWriteText = "AutoCAD está escribiendo el rack: espera a que termine o pulsa «Dejar de esperar».";

        // M-11: once the command took the write the window no longer offers «Dejar de esperar» (I-02 of the fifth review), so the editor
        // no longer says to press it; the same text as S-28 of the Selectivo.
        private const string TakenWriteText = "AutoCAD ya tomó la escritura del rack: espera a que termine, o termina o cancela la orden en AutoCAD; la ventana recibirá su respuesta.";

        private static readonly RackProjectStore Store = new();

        private readonly DynamicDraftEditor _editor;
        private readonly Action<DynamicEditorViewModel> _update;
        private int _selectedTab = 1;
        private bool _dirty;
        private bool _writing;
        private bool _writeTaken;
        private bool _confirmingClose;
        private string _baselineJson;
        private string _baselineName = string.Empty;
        private string _message = string.Empty;
        private bool _messageIsError;
        private int _anchorFront;
        private int _anchorLevel;

        private IReadOnlyList<DynamicFieldItem> _essentials = Array.Empty<DynamicFieldItem>();
        private IReadOnlyList<DynamicFieldItem> _postFields = Array.Empty<DynamicFieldItem>();
        private IReadOnlyList<DynamicFieldItem> _cellFields = Array.Empty<DynamicFieldItem>();
        private IReadOnlyList<DynamicFieldItem> _frontFields = Array.Empty<DynamicFieldItem>();
        private IReadOnlyList<DynamicFieldItem> _advancedFields = Array.Empty<DynamicFieldItem>();
        private IReadOnlyList<DynamicFieldItem> _bedFields = Array.Empty<DynamicFieldItem>();
        private IReadOnlyList<DynamicCalcItem> _calculated = Array.Empty<DynamicCalcItem>();
        private IReadOnlyList<DynamicMatrixRow> _rows = Array.Empty<DynamicMatrixRow>();
        private IReadOnlyList<DynamicColumnHead> _columns = Array.Empty<DynamicColumnHead>();
        private IReadOnlyList<DynamicModuleItem> _modules = Array.Empty<DynamicModuleItem>();
        private IReadOnlyList<DynamicPostSafetyItem> _postSafety = Array.Empty<DynamicPostSafetyItem>();
        private string _safetySummary = string.Empty;
        private Func<DynamicDraftEditor, IReadOnlyList<RackCad.Domain.Systems.Selective.SelectiveSafetySelection>> _safetyEditor;
        private Action<DynamicEditorViewModel, IReadOnlyList<RackViewAddress>> _insertViews;
        private Action<DynamicEditorViewModel, IReadOnlyList<RackViewAddress>> _insertNew;
        private bool _isNewRack;

        /// <param name="editor">The draft of the rack, as it was read (its design is the baseline of «Modificado» and of «Descartar»).</param>
        /// <param name="rackId">The identity of the rack in the drawing; null for a new rack.</param>
        /// <param name="update">What «Actualizar» does with this editor (the host writes the drawing); null when it is not offered.</param>
        /// <param name="isNewRack">A new rack made in the window (step 4): its identity is given, but it is not in the drawing until
        /// «Insertar en el dibujo…» places it; until then «Actualizar» is not offered.</param>
        public DynamicEditorViewModel(DynamicDraftEditor editor, string rackId, Action<DynamicEditorViewModel> update = null, bool isNewRack = false)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            RackId = rackId;
            _update = update;
            _isNewRack = isNewRack;
            _anchorFront = editor.Matrix.SelectedFrontIndex;
            _anchorLevel = editor.Matrix.SelectedLevelIndex;
            TakeBaseline();
            Rebuild();
            if (!string.IsNullOrWhiteSpace(editor.LoadNotice)) Say(editor.LoadNotice, error: false);
        }

        /// <summary>«Cerrar el editor» was confirmed (or the draft had nothing to lose): the host forgets this editor.</summary>
        public event Action<DynamicEditorViewModel> CloseRequested;

        // ------------------------------------------------------------------ what the editor shows

        public DynamicDraftEditor Draft => _editor;

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

        /// <summary>0 = Rack, 1 = Matriz (the first tab, as the Owner decided), 2 = Extremos y seguridad, 3 = Avanzado.</summary>
        public int SelectedTab
        {
            get => _selectedTab;
            set => SetProperty(ref _selectedTab, value);
        }

        public bool IsDirty => _dirty;

        /// <summary>AutoCAD is writing this draft: nothing can be edited until it answers or the window stops waiting.</summary>
        public bool IsWriting => _writing;

        public bool CanEdit => !_writing;

        public bool IsNameReadOnly => !CanEdit;

        public bool CanDiscard => _dirty && CanEdit;

        public string StatusText => _writing ? "Escribiendo" : _dirty ? "Modificado" : "Limpio";

        public string Message => _message;

        public bool MessageIsError => _messageIsError;

        public bool CanUpdate => _update != null && !_isNewRack && _dirty && CanEdit && !PropertiesBlockUpdate && _editor.Design != null && string.IsNullOrEmpty(_editor.LastError);

        /// <summary>A new rack of the window that is not in the drawing yet («Insertar en el dibujo…» places it).</summary>
        public bool IsNewRack => _isNewRack;

        public string UpdateBlockedText
        {
            get
            {
                if (_isNewRack) return "Un rack nuevo se escribe en el dibujo con «Insertar en el dibujo…».";
                if (_update == null) return "Aquí no se puede escribir el dibujo.";
                if (_writing) return WritingText;
                if (!_dirty) return "Sin cambios que escribir.";
                if (!string.IsNullOrEmpty(_editor.LastError)) return _editor.LastError;
                if (PropertiesBlockUpdate) return PropertiesTab.BlockedText; // T4 va (VA-07)
                return string.Empty;
            }
        }

        /// <summary>«Cerrar el editor» of a draft with changes asks first, inline: discard them and close, or cancel.</summary>
        public bool IsConfirmingClose => _confirmingClose;

        public IReadOnlyList<DynamicFieldItem> Essentials => _essentials;

        public IReadOnlyList<DynamicFieldItem> PostFields => _postFields;

        public IReadOnlyList<DynamicCalcItem> Calculated => _calculated;

        public IReadOnlyList<DynamicColumnHead> Columns => _columns;

        public IReadOnlyList<DynamicMatrixRow> Rows => _rows;

        public IReadOnlyList<DynamicFieldItem> CellFields => _cellFields;

        public IReadOnlyList<DynamicFieldItem> FrontFields => _frontFields;

        public IReadOnlyList<DynamicFieldItem> AdvancedFields => _advancedFields;

        /// <summary>
        /// «Cama y pendiente» (step 3 of Dinámico; the Owner: «Debe de ser todo modificable», by front, in «Avanzado»): the roller, its
        /// pitch, a brake every N rollers and the slope of the active fronts of the selection. Empty values keep the bed as it always was.
        /// </summary>
        public IReadOnlyList<DynamicFieldItem> BedFields => _bedFields;

        /// <summary>Which fronts «Cama y pendiente» writes: the active fronts of the selection of the matrix.</summary>
        public string BedScopeText
        {
            get
            {
                var fronts = SelectedFronts().Where(f => _editor.Matrix.IsActive(IndexOf(f))).Select(f => "F" + (IndexOf(f) + 1)).ToList();
                return fronts.Count == 0
                    ? "La selección no tiene frentes activos: elige una celda de un frente activo en «Matriz»."
                    : "Frentes de la selección: " + string.Join(", ", fronts) + ". Para elegir otros, selecciónalos en «Matriz».";
            }
        }

        public IReadOnlyList<DynamicModuleItem> Modules => _modules;

        // ------------------------------------------------------------------ «Extremos y seguridad» (step 4; decision 6 of the prototype)

        /// <summary>
        /// What «Editar seguridad…» opens: the safety dialog of the classic editor over copies of the safety of the draft (its result, or
        /// null when it was cancelled). The window sets it when it can show a modal over AutoCAD; without it the button is not offered.
        /// </summary>
        public Func<DynamicDraftEditor, IReadOnlyList<RackCad.Domain.Systems.Selective.SelectiveSafetySelection>> SafetyEditor
        {
            get => _safetyEditor;
            set
            {
                _safetyEditor = value;
                OnPropertyChanged(nameof(CanEditSafety));
                OnPropertyChanged(nameof(SafetyEditText));
            }
        }

        public bool CanEditSafety => CanEdit && _safetyEditor != null;

        public string SafetyEditText => _safetyEditor == null
            ? "El diálogo de seguridad no está disponible aquí: usa «Abrir en editor clásico»."
            : "Las botas, los protectores laterales, los desviadores, las defensas y la guía se eligen en el diálogo de seguridad del editor clásico.";

        /// <summary>«Seguridad por poste»: what the drawing places at each post, family by family, as the BOM counts it.</summary>
        public IReadOnlyList<DynamicPostSafetyItem> PostSafety => _postSafety;

        public string SafetySummary => _safetySummary;

        /// <summary>The rack has an entrance guide that the window can put in or take out of cells.</summary>
        public bool CanEditGuide => CanEdit && _editor.GuideSelection != null;

        /// <summary>Where the entrance guide is: in all the active cells, in some, in none; or that the rack has no guide.</summary>
        public string GuideSummary
        {
            get
            {
                if (_editor.GuideSelection == null)
                    return "Este rack no lleva guía de entrada. Para añadirla, elige su variante en «Variante y medidas»."; // T4 seg, SG-01
                var cells = ActiveCells().ToList();
                if (cells.Count == 0) return "Guía de entrada: el rack no tiene celdas activas.";
                var guides = new HashSet<(int, int)>(_editor.GuideCells);
                var on = cells.Count(guides.Contains);
                if (on == cells.Count) return "Guía de entrada: en todas las celdas (" + on.ToString(CultureInfo.InvariantCulture) + ").";
                if (on == 0) return "Guía de entrada: en ninguna celda.";
                return "Guía de entrada: en " + on.ToString(CultureInfo.InvariantCulture) + " de " + cells.Count.ToString(CultureInfo.InvariantCulture) + " celdas.";
            }
        }

        /// <summary>
        /// «Guía de entrada» of the inspector: true when every active cell of the selection has it, false when none has it, null when they
        /// differ. Writing it puts the guide in, or takes it out of, the active cells of the selection. When it says «varía» (null), any
        /// write puts the guide in every active cell of the selection and never takes it out (I-01 of the review of Selectivo; the approved
        /// prototype: «varía; marcar la pone en todas»): WPF turns a click on an indeterminate box into false, not into true.
        /// </summary>
        public bool? SelectionGuide
        {
            get
            {
                var cells = SelectedActiveCells().ToList();
                if (cells.Count == 0) return false;
                var guides = new HashSet<(int, int)>(_editor.GuideCells);
                var on = cells.Count(guides.Contains);
                return on == cells.Count ? true : on == 0 ? false : (bool?)null;
            }
            set
            {
                var shown = SelectionGuide;
                if (!value.HasValue) { OnPropertyChanged(nameof(SelectionGuide)); return; }
                // M-01 of the review of the fix: with no active cell in the selection there is nothing to write (the box is disabled too).
                if (!SelectedActiveCells().Any()) { OnPropertyChanged(nameof(SelectionGuide)); return; }
                var put = shown.HasValue ? value.Value : true;
                if (put == shown) { OnPropertyChanged(nameof(SelectionGuide)); return; }
                if (!CanEditGuide) { Say(_editor.GuideSelection == null ? GuideSummary : WritingText, error: true); OnPropertyChanged(nameof(SelectionGuide)); return; }
                var cells = SelectedActiveCells().ToList();
                Apply(() => _editor.SetGuide(cells, put), null);
            }
        }

        /// <summary>
        /// «Guía de entrada» of the inspector can be written: the rack has a guide, nothing is being written, and the selection has an
        /// active cell (a selection only in a blank front has none: the box is disabled, M-01 of the review of the fix).
        /// </summary>
        public bool CanEditSelectionGuide => CanEditGuide && SelectedActiveCells().Any();

        /// <summary>The label of «Guía de entrada»: with a mixed selection it says that marking it puts the guide in all the cells.</summary>
        public string SelectionGuideLabel => SelectionGuide.HasValue ? "Guía de entrada" : "Guía de entrada (varía; marcar la pone en todas)";

        /// <summary>«En todas las celdas»: the entrance guide in every active cell.</summary>
        public void GuideEverywhere() => SetGuideOnActiveCells(true);

        /// <summary>«En ninguna»: the entrance guide out of every active cell.</summary>
        public void GuideNowhere() => SetGuideOnActiveCells(false);

        /// <summary>«Editar seguridad…»: the classic dialog; its result replaces the safety of the draft, which is recomposed.</summary>
        public void EditSafety()
        {
            if (!CanEditSafety)
            {
                Say(_safetyEditor == null ? SafetyEditText : WritingText, error: true);
                return;
            }

            var result = _safetyEditor(_editor);
            if (result == null) return;
            Apply(() => _editor.ReplaceSafety(result), null);
        }

        private void SetGuideOnActiveCells(bool on)
        {
            if (!CanEditGuide)
            {
                Say(_editor.GuideSelection == null ? GuideSummary : WritingText, error: true);
                return;
            }

            var cells = ActiveCells().ToList();
            Apply(() => _editor.SetGuide(cells, on), null);
        }

        // The cells that exist in the active fronts (a front in blank keeps its cells dormant: they are not edited, as in the classic).
        private IEnumerable<(int Front, int Level)> ActiveCells()
        {
            var matrix = _editor.Matrix;
            for (var f = 0; f < matrix.Count; f++)
            {
                if (!matrix.IsActive(f)) continue;
                for (var l = 0; l < matrix.Fronts[f].LoadLevels; l++) yield return (f, l);
            }
        }

        private IEnumerable<(int Front, int Level)> SelectedActiveCells()
            => _editor.Matrix.SelectedCells()
                .Where(c => _editor.Matrix.IsActive(c.FrontIndex) && c.LevelIndex < _editor.Matrix.Fronts[c.FrontIndex].LoadLevels)
                .Select(c => (c.FrontIndex, c.LevelIndex));

        private void RebuildSafety()
        {
            var rows = _editor.PostSafety();
            _postSafety = rows.Select(row => new DynamicPostSafetyItem(row)).ToList();
            int Posts(Func<DynamicPostSafetyRow, int> count) => rows.Count(row => count(row) > 0);
            _safetySummary = rows.Count == 0
                ? string.Empty
                : "Botas en " + Posts(r => r.Boots) + " · protectores laterales en " + Posts(r => r.SideGuards)
                  + " · desviadores en " + Posts(r => r.DeflectorsAtExit + r.DeflectorsAtEntrance) + " · defensas en " + Posts(r => r.Defenses)
                  + " · guías en " + Posts(r => r.EntranceGuides) + ", de " + rows.Count.ToString(CultureInfo.InvariantCulture)
                  + (rows.Count == 1 ? " poste." : " postes.");
            OnPropertyChanged(nameof(PostSafety));
            OnPropertyChanged(nameof(SafetySummary));
            OnPropertyChanged(nameof(GuideSummary));
            RebuildSafetyPanel(rows); // T4 seg: the editable table and the inspector (DynamicEditorViewModel.Seg.cs)
        }

        public string InspectorTitle
        {
            get
            {
                var cells = _editor.Matrix.SelectedCells();
                if (cells.Count == 1) return "Celda F" + (cells[0].FrontIndex + 1) + " · N" + (cells[0].LevelIndex + 1);
                var fronts = cells.Select(c => c.FrontIndex).Distinct().Count();
                return cells.Count.ToString(CultureInfo.InvariantCulture) + " celdas en " + fronts.ToString(CultureInfo.InvariantCulture)
                    + (fronts == 1 ? " frente" : " frentes");
            }
        }

        /// <summary>Why part of the selection is not edited: the fronts in blank that it touches; empty otherwise.</summary>
        public string InspectorNote
        {
            get
            {
                var blank = _editor.Matrix.SelectedFrontIndices.Distinct().Where(f => !_editor.Matrix.IsActive(f)).OrderBy(f => f).ToList();
                if (blank.Count == 0) return string.Empty;
                var names = string.Join(", ", blank.Select(f => "F" + (f + 1)));
                return blank.Count == 1
                    ? names + " está en blanco: sus niveles y sus celdas no se editan. Actívalo para volver a editarlo."
                    : names + " están en blanco: sus niveles y sus celdas no se editan. Actívalos para volver a editarlos.";
            }
        }

        public bool NumberFronts
        {
            get => _editor.Annotations.NumberFronts;
            set { if (_editor.Annotations.NumberFronts != value) SetAnnotation(() => _editor.Annotations.NumberFronts = value); }
        }

        public bool NumberLevels
        {
            get => _editor.Annotations.NumberLevels;
            set { if (_editor.Annotations.NumberLevels != value) SetAnnotation(() => _editor.Annotations.NumberLevels = value); }
        }

        public bool DrawRackName
        {
            get => _editor.Annotations.DrawRackName;
            set { if (_editor.Annotations.DrawRackName != value) SetAnnotation(() => _editor.Annotations.DrawRackName = value); }
        }

        public bool DerivedPostReinforced
        {
            get => _editor.DerivedPostReinforced;
            set
            {
                if (_editor.DerivedPostReinforced == value) return;
                Apply(() => _editor.DerivedPostReinforced = value, null);
            }
        }

        // ------------------------------------------------------------------ the selection of the matrix (the one scope)

        /// <summary>A click on a cell: one cell; with Ctrl, add or remove it; with Mayús, the rectangle from the anchor (the last plain click).</summary>
        public void ClickCell(int frontIndex, int levelIndex, bool control, bool shift)
        {
            var matrix = _editor.Matrix;
            if (frontIndex < 0 || frontIndex >= matrix.Count) return;

            if (shift)
            {
                var fromFront = _anchorFront >= 0 && _anchorFront < matrix.Count ? _anchorFront : matrix.SelectedFrontIndex;
                var fromLevel = _anchorFront >= 0 && _anchorFront < matrix.Count ? _anchorLevel : matrix.SelectedLevelIndex;
                matrix.ToggleCell(fromFront, fromLevel, extendSelection: false);
                for (var f = Math.Min(fromFront, frontIndex); f <= Math.Max(fromFront, frontIndex); f++)
                    for (var l = Math.Min(fromLevel, levelIndex); l <= Math.Max(fromLevel, levelIndex); l++)
                        Add(f, l);
                // The clicked cell becomes the primary one, still selected; the anchor stays where it was.
                if (Exists(frontIndex, levelIndex) && matrix.IsSelected(frontIndex, levelIndex)) { matrix.ToggleCell(frontIndex, levelIndex, true); matrix.ToggleCell(frontIndex, levelIndex, true); }
            }
            else
            {
                matrix.ToggleCell(frontIndex, levelIndex, control);
                _anchorFront = frontIndex;
                _anchorLevel = levelIndex;
            }

            matrix.NormalizeSelection();
            RebuildSelection();
        }

        /// <summary>«F1»…: every cell of that front.</summary>
        public void SelectFront(int frontIndex)
        {
            var matrix = _editor.Matrix;
            if (frontIndex < 0 || frontIndex >= matrix.Count) return;
            matrix.ToggleCell(frontIndex, 0, extendSelection: false);
            for (var l = 1; l < matrix.Fronts[frontIndex].LoadLevels; l++) Add(frontIndex, l);
            matrix.NormalizeSelection();
            SetAnchor(frontIndex, 0);
            RebuildSelection();
        }

        /// <summary>«N1»…: that level in every front that has it.</summary>
        public void SelectLevel(int levelIndex)
        {
            var matrix = _editor.Matrix;
            var first = -1;
            for (var f = 0; f < matrix.Count; f++)
            {
                if (!Exists(f, levelIndex)) continue;
                if (first < 0) { matrix.ToggleCell(f, levelIndex, extendSelection: false); first = f; }
                else Add(f, levelIndex);
            }

            matrix.NormalizeSelection();
            if (first >= 0) SetAnchor(first, levelIndex);
            RebuildSelection();
        }

        /// <summary>«Todo»: every cell of the rack.</summary>
        public void SelectAll()
        {
            var matrix = _editor.Matrix;
            matrix.ToggleCell(0, 0, extendSelection: false);
            for (var f = 0; f < matrix.Count; f++)
                for (var l = 0; l < matrix.Fronts[f].LoadLevels; l++)
                    Add(f, l);
            matrix.NormalizeSelection();
            SetAnchor(0, 0);
            RebuildSelection();
        }

        // ------------------------------------------------------------------ the gestures of the rack

        /// <summary>«Frentes»: the count of fronts; new fronts copy the selected one, as in the classic editor.</summary>
        public string SetFrontCount(string text)
        {
            if (!UiSupport.TryInt(text, out var count) || count < 1) return "La cantidad de frentes debe ser un entero mayor o igual que 1.";
            return Apply(() => _editor.Matrix.SetFrontCount(count), null);
        }

        /// <summary>«En blanco» of a front: it keeps its structure and carries nothing. The last active front cannot be blank.</summary>
        public void SetFrontBlank(int frontIndex, bool blank)
        {
            if (!CanEdit)
            {
                Say(WritingText, error: true);
                return;
            }

            if (_editor.Matrix.IsActive(frontIndex) == !blank) return;

            string refusal = null;
            Apply(() =>
            {
                if (!_editor.Matrix.SetActive(frontIndex, !blank)) refusal = "Al menos un frente debe permanecer activo.";
            }, null);
            if (refusal != null) Say(refusal, error: true);
        }

        /// <summary>«Descartar»: the draft goes back to what was last read from the drawing or written to it in this editor.</summary>
        public void Discard()
        {
            if (!CanEdit)
            {
                Say(WritingText, error: true);
                return;
            }

            DynamicRackDesign baseline;
            try
            {
                baseline = _baselineJson == null ? null : Store.Deserialize(_baselineJson)?.DynamicDesign;
            }
            catch (Exception ex)
            {
                Say("No se pudo volver a lo leído del dibujo: " + ex.Message + " El borrador sigue como está.", error: true);
                return;
            }

            if (baseline != null) _editor.LoadFrom(baseline, _baselineName);
            TakeBaseline();
            _propertiesTab?.Reset(); // T4 va: «Descartar» drops the properties written in the tab too (VA-07)
            _confirmingClose = false;
            Say(RackId == null || _isNewRack
                ? "Borrador descartado."
                : "Borrador descartado: se muestra lo último que se leyó del dibujo o se escribió en él desde este editor.", error: false);
            Rebuild();
        }

        /// <summary>«Restaurar layout»: rebuilds the module sequence from scratch; the window asks first, because custom cabeceras are lost.</summary>
        public void RestoreStandard()
        {
            if (!CanEdit)
            {
                Say(WritingText, error: true);
                return;
            }

            if (_editor.Recompose(restoreStandard: true))
            {
                Say(string.IsNullOrWhiteSpace(_editor.LastRebuildReport) ? "Layout restaurado." : _editor.LastRebuildReport, error: false);
            }
            else
            {
                // Nothing changed: the last valid recomposition stays, and so does what can be written (I-09).
                Say(_editor.LastError, error: true);
                _editor.InputsRestoredToLastValid();
            }

            Rebuild();
            UpdateDirty();
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

        // ------------------------------------------------------------------ «Insertar vista…» (step 4; decision 10 of the prototype)

        /// <summary>What «Insertar vista…» does with the views chosen: the window first writes the draft in the views that exist, then inserts them.</summary>
        public Action<DynamicEditorViewModel, IReadOnlyList<RackViewAddress>> InsertViewsAction
        {
            get => _insertViews;
            set { _insertViews = value; RaiseState(); }
        }

        /// <summary>What «Insertar en el dibujo…» of a new rack does with the views chosen: the window inserts the rack with them.</summary>
        public Action<DynamicEditorViewModel, IReadOnlyList<RackViewAddress>> InsertNewAction
        {
            get => _insertNew;
            set { _insertNew = value; RaiseState(); }
        }

        /// <summary>«Insertar vista…» for a rack of the drawing; «Insertar en el dibujo…» for a new one.</summary>
        public string InsertViewLabel => _isNewRack ? "Insertar en el dibujo…" : "Insertar vista…";

        /// <summary>The views can be inserted now (T4 ins: the panel enables «Insertar N vistas» with this and at least one view marked).</summary>
        public bool CanInsertView => string.IsNullOrEmpty(InsertViewBlockedText);

        /// <summary>
        /// Why the views cannot be inserted now; empty when they can. T4 ins (DG-05): the panel opens anyway and says it next to «Insertar N
        /// vistas», with another request of the window that waits for AutoCAD (dolor 9).
        /// </summary>
        public string InsertViewBlockedText
        {
            get
            {
                var action = _isNewRack ? _insertNew : _insertViews;
                if (action == null || string.IsNullOrWhiteSpace(RackId))
                    return _isNewRack ? "Aquí no se puede insertar el rack nuevo." : "Aquí no se pueden insertar vistas del rack.";
                if (_writing) return WritingText;
                var drawing = OtherDrawingBlockedText;
                if (!string.IsNullOrEmpty(drawing)) return drawing;
                var other = OtherRequestBlockedText;
                if (!string.IsNullOrEmpty(other)) return other;
                if (_editor.Design == null) return "No hay un borrador válido que escribir.";
                if (!string.IsNullOrEmpty(_editor.LastError)) return _editor.LastError;
                return string.Empty;
            }
        }

        /// <summary>
        /// «Insertar vista…»: the views to insert, chosen in the panel of the editor (T4 ins, DG-01: <see cref="InsertPanel"/>) as in «Insertar
        /// varias…» of the classic editor; AutoCAD first writes the draft in the views that exist (also with no change: they are redrawn as
        /// they are) and then places the new ones, linked to the rack. The button only opens the panel; «Insertar N vistas» writes.
        /// </summary>
        public void InsertView()
        {
            if (!CanOpenInsertPanel)
            {
                Say(InsertPanelBlockedText, error: true);
                return;
            }

            OpenInsertPanel();
        }

        /// <summary>
        /// AutoCAD placed the new rack: from now on it is a rack of the drawing, written with «Actualizar» and «Insertar vista…». The name the
        /// drawing gave it (the automatic one of I-60) becomes its name and the name of «Limpio».
        /// </summary>
        public void MarkInserted(string name = null)
        {
            if (!_isNewRack) return;
            _isNewRack = false;
            if (!string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(_editor.Name))
            {
                _editor.Name = name.Trim();
                _baselineName = _editor.Name;
                OnPropertyChanged(nameof(Name));
                UpdateDirty();
            }

            OnPropertyChanged(nameof(IsNewRack));
            OnPropertyChanged(nameof(InsertViewLabel));
            RaiseState();
        }

        /// <summary>«Cerrar el editor»: closes it at once when the draft has nothing to lose, and asks first when it has changes.</summary>
        public void RequestClose()
        {
            if (!CanEdit)
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
            if (!_confirmingClose || !CanEdit) return;
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

        /// <summary>What the editor says while AutoCAD writes it: «Dejar de esperar» only while the command has not taken the write (M-11).</summary>
        public string WritingText => _writeTaken ? TakenWriteText : WaitingWriteText;

        /// <summary>
        /// The command of AutoCAD took the write (<c>RequestStarted</c>): its answer will come, and the window no longer offers «Dejar de
        /// esperar», so the editor no longer says to press it (M-11, as S-28 of the Selectivo).
        /// </summary>
        public void WriteTaken()
        {
            if (!_writing || _writeTaken) return;
            _writeTaken = true;
            OnPropertyChanged(nameof(WritingText));
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

        // ------------------------------------------------------------------ internals

        private void SetAnchor(int frontIndex, int levelIndex)
        {
            _anchorFront = frontIndex;
            _anchorLevel = levelIndex;
        }

        private void SetConfirmingClose(bool value)
        {
            if (_confirmingClose == value) return;
            _confirmingClose = value;
            OnPropertyChanged(nameof(IsConfirmingClose));
        }

        private bool Exists(int frontIndex, int levelIndex)
        {
            var matrix = _editor.Matrix;
            return frontIndex >= 0 && frontIndex < matrix.Count && levelIndex >= 0 && levelIndex < matrix.Fronts[frontIndex].LoadLevels;
        }

        private void Add(int frontIndex, int levelIndex)
        {
            if (!Exists(frontIndex, levelIndex) || _editor.Matrix.IsSelected(frontIndex, levelIndex)) return;
            _editor.Matrix.ToggleCell(frontIndex, levelIndex, extendSelection: true);
        }

        private void SetAnnotation(Action change)
        {
            Apply(change, null);
        }

        private string DesignJson() => _editor.Design == null ? null : Store.Serialize(RackProject.ForDynamic(_editor.Design));

        private void TakeBaseline()
        {
            _baselineJson = DesignJson();
            _baselineName = (_editor.Name ?? string.Empty).Trim();
            _dirty = false;
            RaiseState();
        }

        // «Modificado» is a difference with what the drawing has (as last read or written), never a gesture: leaving a field, a value
        // refused, or a recomposition that gives the same design leave the draft clean.
        private void UpdateDirty()
        {
            var dirty = !string.Equals(DesignJson(), _baselineJson, StringComparison.Ordinal)
                        || !string.Equals((_editor.Name ?? string.Empty).Trim(), _baselineName, StringComparison.Ordinal)
                        || PropertiesDirty; // T4 va: the tab «Propiedades» (VA-07)
            if (dirty == _dirty) return;
            _dirty = dirty;
            if (!dirty) SetConfirmingClose(false);
            RaiseState();
        }

        // Every change of the draft: a snapshot of the matrix, of its selection and of the scalars, the change, the recomposition; a change
        // that cannot be recomposed is undone (the matrix, the selection and the values) and its reason goes back to the field.
        private string Apply(Action change, string fieldError)
        {
            if (fieldError != null) return fieldError;
            if (!CanEdit)
            {
                Say(WritingText, error: true);
                return WritingText;
            }

            var snapshot = _editor.Matrix.Snapshot();
            var selection = CaptureSelection();
            var state = CaptureScalars();
            var safety = _editor.CopySafety();
            change();
            if (!_editor.Recompose())
            {
                var reason = _editor.LastError;
                _editor.Matrix.Restore(snapshot);
                RestoreSelection(selection);
                RestoreScalars(state);
                _editor.ReplaceSafety(safety);
                // The draft is back to its last valid recomposition: the refused value does not block «Actualizar» (I-09).
                _editor.InputsRestoredToLastValid();
                Say(reason, error: true);
                Rebuild();
                return reason;
            }

            Say(string.IsNullOrWhiteSpace(_editor.LastRebuildReport) ? string.Empty : _editor.LastRebuildReport, error: false);
            Rebuild();
            UpdateDirty();
            return null;
        }

        private (List<(int Front, int Level)> Cells, int Front, int Level) CaptureSelection()
        {
            var matrix = _editor.Matrix;
            return (matrix.SelectedCells().Select(c => (c.FrontIndex, c.LevelIndex)).ToList(), matrix.SelectedFrontIndex, matrix.SelectedLevelIndex);
        }

        private void RestoreSelection((List<(int Front, int Level)> Cells, int Front, int Level) selection)
        {
            var matrix = _editor.Matrix;
            if (matrix.Count == 0) return;

            var order = selection.Cells.Where(c => (c.Front != selection.Front || c.Level != selection.Level) && Exists(c.Front, c.Level)).ToList();
            if (Exists(selection.Front, selection.Level)) order.Add((selection.Front, selection.Level));
            if (order.Count > 0)
            {
                matrix.ToggleCell(order[0].Front, order[0].Level, extendSelection: false);
                foreach (var cell in order.Skip(1)) matrix.ToggleCell(cell.Front, cell.Level, extendSelection: true);
            }

            matrix.NormalizeSelection();
        }

        private (double Depth, double Peralte, string Post, double? Manual, int? SepCount, double? SepSpacing, bool Reinforced, double? Reinforcement, bool NumF, bool NumL, bool RackName, double Scale, DimensionDetail Dims) CaptureScalars()
        {
            var a = _editor.Annotations;
            return (_editor.PalletDepth, _editor.PostPeralte, _editor.PostCatalogId, _editor.ManualHeaderHeight, _editor.SeparatorCountOverride,
                _editor.SeparatorSpacingOverride, _editor.DerivedPostReinforced, _editor.DerivedPostReinforcementHeight,
                a.NumberFronts, a.NumberLevels, a.DrawRackName, a.AnnotationScale, a.Dimensions);
        }

        private void RestoreScalars((double Depth, double Peralte, string Post, double? Manual, int? SepCount, double? SepSpacing, bool Reinforced, double? Reinforcement, bool NumF, bool NumL, bool RackName, double Scale, DimensionDetail Dims) s)
        {
            _editor.PalletDepth = s.Depth;
            _editor.PostPeralte = s.Peralte;
            _editor.PostCatalogId = s.Post;
            _editor.ManualHeaderHeight = s.Manual;
            _editor.SeparatorCountOverride = s.SepCount;
            _editor.SeparatorSpacingOverride = s.SepSpacing;
            _editor.DerivedPostReinforced = s.Reinforced;
            _editor.DerivedPostReinforcementHeight = s.Reinforcement;
            _editor.Annotations.NumberFronts = s.NumF;
            _editor.Annotations.NumberLevels = s.NumL;
            _editor.Annotations.DrawRackName = s.RackName;
            _editor.Annotations.AnnotationScale = s.Scale;
            _editor.Annotations.Dimensions = s.Dims;
        }

        private void RaiseState()
        {
            OnPropertyChanged(nameof(IsDirty));
            OnPropertyChanged(nameof(IsWriting));
            OnPropertyChanged(nameof(CanEdit));
            OnPropertyChanged(nameof(IsNameReadOnly));
            OnPropertyChanged(nameof(CanDiscard));
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(CanUpdate));
            OnPropertyChanged(nameof(UpdateBlockedText));
            OnPropertyChanged(nameof(CanEditGuide));
            OnPropertyChanged(nameof(CanEditSelectionGuide));
            OnPropertyChanged(nameof(CanEditSafety));
            OnPropertyChanged(nameof(CanInsertView));
            OnPropertyChanged(nameof(InsertViewBlockedText));
            RaiseStateIns(); // T4 ins: the button and the panel «Insertar vista…»
            RefreshSafetyPanel(); // T4 seg: what can be edited, in the table and the inspector
        }

        // The lists keep their items: a recomposition refreshes them in place, so the focus, the box being written and the button under
        // the mouse survive it (I-05 of the second review). Only a list whose fields change is replaced.
        private void Rebuild()
        {
            Merge(ref _essentials, BuildEssentials(), nameof(Essentials));
            Merge(ref _postFields, BuildPostFields(), nameof(PostFields));
            Merge(ref _advancedFields, BuildAdvancedFields(), nameof(AdvancedFields));
            _calculated = BuildCalculated();
            _modules = BuildModules();
            OnPropertyChanged(nameof(Calculated));
            OnPropertyChanged(nameof(Modules));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(NumberFronts));
            OnPropertyChanged(nameof(NumberLevels));
            OnPropertyChanged(nameof(DrawRackName));
            OnPropertyChanged(nameof(DerivedPostReinforced));
            RebuildSafety();
            RebuildSelection();
            RefreshInsertPanel(); // T4 ins: the open panel of «Insertar vista…» follows the draft (DG-08)
            RaiseState();
        }

        private void Merge(ref IReadOnlyList<DynamicFieldItem> current, IReadOnlyList<DynamicFieldItem> next, string propertyName)
        {
            if (current.Count == next.Count && current.Select(f => f.Key).SequenceEqual(next.Select(f => f.Key), StringComparer.Ordinal))
            {
                for (var i = 0; i < current.Count; i++) current[i].Refresh(next[i]);
                return;
            }

            current = next;
            OnPropertyChanged(propertyName);
        }

        private void RebuildSelection()
        {
            var matrix = _editor.Matrix;
            var maxLevels = Math.Max(1, matrix.Count == 0 ? 1 : matrix.Fronts.Max(f => f.LoadLevels));

            // The shape (fronts × levels) changed: new rows and columns. Otherwise the same items are refreshed in place.
            if (_columns.Count != matrix.Count || _rows.Count != maxLevels)
            {
                _columns = Enumerable.Range(0, matrix.Count).Select(f => new DynamicColumnHead(f, "F" + (f + 1))).ToList();
                _rows = Enumerable.Range(0, maxLevels).Reverse()
                    .Select(l => new DynamicMatrixRow(l, "N" + (l + 1), Enumerable.Range(0, matrix.Count).Select(f => new DynamicCellItem(f, l)).ToList()))
                    .ToList();
                OnPropertyChanged(nameof(Columns));
                OnPropertyChanged(nameof(Rows));
            }

            foreach (var column in _columns) column.IsBlank = !matrix.IsActive(column.FrontIndex);
            var guides = new HashSet<(int, int)>(_editor.GuideCells);
            foreach (var row in _rows)
            {
                foreach (var item in row.Cells)
                {
                    var front = matrix.Fronts[item.FrontIndex];
                    var exists = item.LevelIndex < front.LoadLevels;
                    var active = matrix.IsActive(item.FrontIndex);
                    var text = "—";
                    if (exists && active)
                    {
                        front.EnsureCellCount(front.LoadLevels);
                        var cell = front.Cells[item.LevelIndex];
                        text = Num(cell.PalletFront) + "×" + Num(cell.PalletHeight);
                    }

                    item.Set(
                        text,
                        exists,
                        exists && matrix.IsSelected(item.FrontIndex, item.LevelIndex),
                        exists && matrix.SelectedFrontIndex == item.FrontIndex && matrix.SelectedLevelIndex == item.LevelIndex,
                        !active,
                        exists && active && guides.Contains((item.FrontIndex, item.LevelIndex)));
                }
            }

            OnPropertyChanged(nameof(SelectionGuide));
            OnPropertyChanged(nameof(SelectionGuideLabel));
            OnPropertyChanged(nameof(CanEditSelectionGuide));
            OnPropertyChanged(nameof(GuideSummary));
            Merge(ref _cellFields, BuildCellFields(), nameof(CellFields));
            Merge(ref _frontFields, BuildFrontFields(), nameof(FrontFields));
            Merge(ref _bedFields, BuildBedFields(), nameof(BedFields));
            OnPropertyChanged(nameof(BedScopeText));
            OnPropertyChanged(nameof(InspectorTitle));
            OnPropertyChanged(nameof(InspectorNote));
        }

        // «Lo esencial»: rack-wide values. A cell value writes every cell of the active fronts; the levels and the first beam of a front in
        // blank are not edited (its structure, positions and depths, still is), as in the classic editor.
        private IReadOnlyList<DynamicFieldItem> BuildEssentials()
        {
            var cells = AllCells().ToList();
            var fronts = _editor.Matrix.Fronts.ToList();
            var active = ActiveFronts().ToList();
            return new[]
            {
                Number("depth", "Profundidad de tarima", "in", new[] { _editor.PalletDepth }, v => v > 0.0, "Fondo inválido.", v => _editor.PalletDepth = v),
                CellNumber("front", "Ancho de tarima", "in", cells, c => c.PalletFront, (c, v) => c.PalletFront = v, v => v > 0.0),
                CellNumber("height", "Alto de tarima", "in", cells, c => c.PalletHeight, (c, v) => c.PalletHeight = v, v => v > 0.0),
                CellNumber("weight", "Peso de tarima", "kg", cells, c => c.PalletWeight, (c, v) => c.PalletWeight = v, v => v >= 0.0),
                new DynamicFieldItem("fronts", "Frentes", string.Empty, _editor.Matrix.Count.ToString(CultureInfo.InvariantCulture), string.Empty, SetFrontCount, !CanEdit),
                FrontInt("positions", "Carriles por frente", fronts, f => f.PalletCount, (f, v) => f.PalletCount = v, 1),
                FrontInt("levels", "Niveles", active, f => f.LoadLevels, (f, v) => { f.LoadLevels = v; f.EnsureCellCount(v); }, 1),
                FrontInt("deep", "Posiciones de fondo", fronts, f => f.PalletsDeep, (f, v) => f.PalletsDeep = v, 2),
                FrontNumber("first", "Inicio del 1er larguero", "in", active, f => f.FirstLevelHeight, (f, v) => f.FirstLevelHeight = v, v => v >= 0.0),
                CellNumber("clear", "Claro libre", "in", cells, c => c.ClearHeight, (c, v) => c.ClearHeight = v, v => v >= 0.0),
                PostPeralteEssential(), // T5 pp: «Peralte de poste», para todos los postes, seen without unfolding «Poste y anotaciones»
            };
        }

        private IReadOnlyList<DynamicFieldItem> BuildPostFields() => new[]
        {
            // T5 pp: «Peralte de poste» is in «Lo esencial» now (PostPeralteEssential).
            Number("scale", "Escala de texto (1 = 6\")", string.Empty, new[] { _editor.Annotations.AnnotationScale }, v => v > 0.0, "La escala de texto debe ser mayor que cero.", v => _editor.Annotations.AnnotationScale = v),
        };

        private IReadOnlyList<DynamicFieldItem> BuildAdvancedFields() => new[]
        {
            OptionalNumber("manual", "Altura manual de cabecera (vacío = calculada)", "in", _editor.ManualHeaderHeight, "Altura manual inválida (debe ser mayor que cero).", v => _editor.ManualHeaderHeight = v),
            OptionalInt("sepCount", "Separadores (cantidad; vacío = estándar)", _editor.SeparatorCountOverride, "Cantidad de separadores inválida (entero >= 1, o vacío para estándar).", v => _editor.SeparatorCountOverride = v),
            OptionalNumber("sepSpacing", "Separación de separadores (vacío = estándar)", "in", _editor.SeparatorSpacingOverride, "Separación de separadores inválida (deja vacío para estándar).", v => _editor.SeparatorSpacingOverride = v),
            OptionalNumber("reinforcement", "Longitud del refuerzo del poste derivado (vacío = altura completa)", "in", _editor.DerivedPostReinforcementHeight, "Altura de refuerzo del poste derivado inválida (deja vacío para altura completa).", v => _editor.DerivedPostReinforcementHeight = v),
        };

        // The inspector: the cell values of the selected cells of the active fronts. The peraltes are the ones of the catalog for the beam
        // of each cell, as the lists of the classic editor offer them.
        private IReadOnlyList<DynamicFieldItem> BuildCellFields()
        {
            var cells = SelectedCells().ToList();
            return new[]
            {
                CellNumber("cFront", "Ancho de tarima", "in", cells, c => c.PalletFront, (c, v) => c.PalletFront = v, v => v > 0.0),
                CellNumber("cHeight", "Alto de tarima", "in", cells, c => c.PalletHeight, (c, v) => c.PalletHeight = v, v => v > 0.0),
                CellNumber("cWeight", "Peso de tarima", "kg", cells, c => c.PalletWeight, (c, v) => c.PalletWeight = v, v => v >= 0.0),
                CellNumber("cClear", "Claro libre", "in", cells, c => c.ClearHeight, (c, v) => c.ClearHeight = v, v => v >= 0.0),
                CellPeralte("cInOut", "Peralte IN/OUT", cells, c => c.InOutBeamCatalogId, DynamicRackDefaults.DefaultBeamDepth, c => c.InOutBeamDepth, (c, v) => c.InOutBeamDepth = v),
                CellPeralte("cInter", "Peralte intermedio", cells, c => c.IntermediateBeamCatalogId, DynamicRackDefaults.DefaultIntermediateBeamDepth, c => c.IntermediateBeamDepth, (c, v) => c.IntermediateBeamDepth = v),
            };
        }

        // The inspector: the front values of the fronts that the selection touches; the levels and the first beam only of the active ones.
        private IReadOnlyList<DynamicFieldItem> BuildFrontFields()
        {
            var fronts = SelectedFronts().ToList();
            var active = fronts.Where(f => _editor.Matrix.IsActive(IndexOf(f))).ToList();
            return new[]
            {
                FrontInt("fPositions", "Carriles", fronts, f => f.PalletCount, (f, v) => f.PalletCount = v, 1),
                FrontInt("fLevels", "Niveles", active, f => f.LoadLevels, (f, v) => { f.LoadLevels = v; f.EnsureCellCount(v); }, 1),
                FrontInt("fDeep", "Posiciones de fondo", fronts, f => f.PalletsDeep, (f, v) => f.PalletsDeep = v, 2),
                FrontInt("fStart", "Inicio en fondo (1 = salida)", fronts, f => f.DepthStartPosition, (f, v) => f.DepthStartPosition = v, 1),
                FrontNumber("fFirst", "Inicio del 1er larguero", "in", active, f => f.FirstLevelHeight, (f, v) => f.FirstLevelHeight = v, v => v >= 0.0),
                FrontLength("fLength", "Largo manual IN/OUT (vacío = calculado)", active),
            };
        }

        // «Largo manual IN/OUT» is a value of the FRONT (decision 3 of the Owner on the prototype: «Sí, es por frente»): it writes every
        // level of the fronts written. The drawing keeps it per level, as the classic editor does, so a front whose levels differ (an
        // edit of the classic editor) says «Varía» until a value is written for the whole front.
        private DynamicFieldItem FrontLength(string key, string label, IReadOnlyList<DynamicEditorFront> fronts)
        {
            var cells = fronts.SelectMany(front =>
            {
                front.EnsureCellCount(front.LoadLevels);
                return front.Cells.Take(front.LoadLevels);
            }).ToList();
            return CellOptional(key, label, "in", cells, c => c.BeamLengthOverride, (c, v) => c.BeamLengthOverride = v);
        }

        // «Cama y pendiente» of the active fronts of the selection. The width of the bed is not here: it follows the pallet (the Owner).
        private IReadOnlyList<DynamicFieldItem> BuildBedFields()
        {
            var fronts = SelectedFronts().Where(f => _editor.Matrix.IsActive(IndexOf(f))).ToList();
            return new[]
            {
                FrontValue("bRoller", "Rodillo (diámetro; vacío = 1.9)", "in", fronts, f => RollerText(f.RollerId), text =>
                {
                    if (string.IsNullOrWhiteSpace(text)) return (null, f => f.RollerId = null);
                    if (!UiSupport.TryNum(text, out var diameter)) return (RollerChoices(), null);
                    var roller = Rollers().FirstOrDefault(r => Math.Abs(r.Diameter - diameter) < 0.01);
                    if (roller == null) return (RollerChoices(), null);
                    // The default roller is written as nothing, so a bed as before stays as it was in the drawing and in the BOM.
                    var id = string.Equals(roller.Id, RackCad.Domain.Systems.FlowBed.FlowBedDefaults.RollerId, StringComparison.OrdinalIgnoreCase) ? null : roller.Id;
                    foreach (var front in fronts)
                        if (front.RollerPitch.HasValue && front.RollerPitch.Value < MinimumPitch(id))
                            return ("El paso de F" + (IndexOf(front) + 1) + " (" + Num(front.RollerPitch.Value) + " in) es menor que este rodillo: cambia antes el paso.", null);
                    return (null, f => f.RollerId = id);
                }),
                FrontValue("bPitch", "Paso de rodillos (vacío = según el rodillo)", "in", fronts, f => f.RollerPitch.HasValue ? Num(f.RollerPitch.Value) : string.Empty, text =>
                {
                    if (string.IsNullOrWhiteSpace(text)) return (null, f => f.RollerPitch = null);
                    if (!UiSupport.TryNum(text, out var pitch) || pitch <= 0.0) return ("El paso debe ser un número mayor que cero.", null);
                    if (Math.Abs(pitch - Math.Round(pitch)) > 1e-9)
                        return ("Los rodillos se cuelgan en los troqueles: el paso debe ser un número entero de pulgadas.", null);
                    foreach (var front in fronts)
                    {
                        var minimum = MinimumPitch(front.RollerId);
                        if (pitch < minimum)
                            return ("El paso no puede ser menor que el rodillo de F" + (IndexOf(front) + 1) + ": mínimo " + Num(minimum) + " in.", null);
                    }

                    // The pitch of the roller is the default: it is written as nothing, so the bed and its line of the BOM stay as before.
                    return (null, f => f.RollerPitch = Math.Abs(pitch - MinimumPitch(f.RollerId)) < 1e-9 ? (double?)null : pitch);
                }),
                FrontValue("bBrakes", "Freno cada N rodillos (vacío = según el fondo de tarima)", string.Empty, fronts,
                    f => f.BrakeEveryRollers.HasValue ? f.BrakeEveryRollers.Value.ToString(CultureInfo.InvariantCulture) : string.Empty, text =>
                {
                    if (string.IsNullOrWhiteSpace(text)) return (null, f => f.BrakeEveryRollers = null);
                    if (!UiSupport.TryInt(text, out var every) || every < 1) return ("Debe ser un entero mayor o igual que 1.", null);
                    return (null, f => f.BrakeEveryRollers = every);
                }),
                FrontValue("bSlope", "Pendiente (in por pie; vacío = 7/16)", "in/pie", fronts, f => f.SlopePerFoot.HasValue ? Num(f.SlopePerFoot.Value) : string.Empty, text =>
                {
                    if (string.IsNullOrWhiteSpace(text)) return (null, f => f.SlopePerFoot = null);
                    if (!UiSupport.TryNum(text, out var perFoot) || perFoot < 0.0) return ("La pendiente debe ser un número mayor o igual que cero.", null);
                    return (null, f => f.SlopePerFoot = perFoot);
                }),
            };
        }

        // One value of the fronts written: «Varía» when they differ; the parser says why a text is refused or how to write it.
        private DynamicFieldItem FrontValue(string key, string label, string unit, IReadOnlyList<DynamicEditorFront> fronts, Func<DynamicEditorFront, string> show,
            Func<string, (string Error, Action<DynamicEditorFront> Set)> parse)
        {
            var values = fronts.Select(show).ToList();
            var same = values.Distinct(StringComparer.Ordinal).Count() <= 1;
            var text0 = same && values.Count > 0 ? values[0] ?? string.Empty : string.Empty;
            return new DynamicFieldItem(key, label, unit, text0, same ? string.Empty : Varies, text =>
            {
                var (error, set) = parse(text);
                if (error != null) return error;
                return Apply(() => { foreach (var front in fronts) set(front); }, null);
            }, !CanEdit || fronts.Count == 0);
        }

        private IEnumerable<RackCad.Application.Catalogs.FlowBedComponentCatalogEntry> Rollers() =>
            (_editor.Catalog.FlowBedProfiles ?? Array.Empty<RackCad.Application.Catalogs.FlowBedComponentCatalogEntry>())
            .Where(p => p != null && p.Diameter > 0.0 && string.Equals(p.Role, "RODILLO", StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p.Diameter);

        private string RollerChoices() => "El rodillo debe ser uno del catálogo: " + string.Join(" o ", Rollers().Select(r => Num(r.Diameter))) + " in.";

        private string RollerText(string rollerId)
        {
            if (string.IsNullOrWhiteSpace(rollerId)) return string.Empty;
            var roller = Rollers().FirstOrDefault(r => string.Equals(r.Id, rollerId, StringComparison.OrdinalIgnoreCase));
            return roller == null ? rollerId : Num(roller.Diameter);
        }

        // The smallest pitch a roller allows: its diameter rounded up to the inch, as the recipe of the bed does by default.
        private double MinimumPitch(string rollerId)
        {
            var id = string.IsNullOrWhiteSpace(rollerId) ? RackCad.Domain.Systems.FlowBed.FlowBedDefaults.RollerId : rollerId;
            var diameter = Rollers().FirstOrDefault(r => string.Equals(r.Id, id, StringComparison.OrdinalIgnoreCase))?.Diameter ?? 0.0;
            return Math.Max(1.0, Math.Ceiling(diameter));
        }

        private int IndexOf(DynamicEditorFront front)
        {
            var fronts = _editor.Matrix.Fronts;
            for (var i = 0; i < fronts.Count; i++) if (ReferenceEquals(fronts[i], front)) return i;
            return -1;
        }

        private IReadOnlyList<DynamicCalcItem> BuildCalculated()
        {
            var system = _editor.System;
            // The heights by post and the rise by front are the ones of the resolved system (rule 1 of the Owner), not one nominal value
            // for the whole rack: «Pendiente» showed the nominal slope after a recomposition and the real one of the highest front after a
            // load (the map of the code of step 3).
            var posts = _editor.PostHeights;
            var rises = _editor.FrontRises;
            var items = new List<DynamicCalcItem>
            {
                new("Altura de cabecera", Num(_editor.HeaderHeight) + " in" + (_editor.ManualHeaderHeight.HasValue ? " (manual)" : " (la más alta)")),
                new("Altura por poste", posts.Count == 0 ? "—" : string.Join(" · ", posts.Select((h, i) => "P" + (i + 1) + " " + Num(h))) + " in"),
                new("Subida de la cama", rises.Count == 0 ? "—" : string.Join(" · ", rises.Select((r, i) => "F" + (i + 1) + " " + Num(r))) + " in"),
                new("Posiciones de fondo del rack", _editor.PalletsDeep.ToString(CultureInfo.InvariantCulture)),
                new("Niveles máximos", _editor.Matrix.MaxLoadLevels().ToString(CultureInfo.InvariantCulture)),
                new("Módulos", system == null ? "—" : system.Modules.Count.ToString(CultureInfo.InvariantCulture)),
            };
            return items;
        }

        private IReadOnlyList<DynamicModuleItem> BuildModules()
        {
            var system = _editor.System;
            if (system == null) return Array.Empty<DynamicModuleItem>();
            var items = new List<DynamicModuleItem>();
            var x = 0.0;
            for (var i = 0; i < system.Modules.Count; i++)
            {
                var module = system.Modules[i];
                items.Add(new DynamicModuleItem(
                    "M" + (i + 1),
                    module.IsHeader ? "Cabecera" : "Separador",
                    Num(module.Length),
                    Num(x),
                    module.IsManualOverride ? "Sí" : "No"));
                x += module.Length;
            }

            return items;
        }

        private IEnumerable<DynamicEditorFront> ActiveFronts()
        {
            var matrix = _editor.Matrix;
            for (var f = 0; f < matrix.Count; f++) if (matrix.IsActive(f)) yield return matrix.Fronts[f];
        }

        // The cells of the active fronts: the cells of a front in blank keep the configuration it will have again when it is active.
        private IEnumerable<DynamicEditorCell> AllCells()
        {
            foreach (var front in ActiveFronts())
            {
                front.EnsureCellCount(front.LoadLevels);
                for (var l = 0; l < front.LoadLevels; l++) yield return front.Cells[l];
            }
        }

        private IEnumerable<DynamicEditorCell> SelectedCells()
        {
            foreach (var address in _editor.Matrix.SelectedCells())
            {
                if (!_editor.Matrix.IsActive(address.FrontIndex)) continue;
                var front = _editor.Matrix.Fronts[address.FrontIndex];
                front.EnsureCellCount(front.LoadLevels);
                if (address.LevelIndex < front.Cells.Count) yield return front.Cells[address.LevelIndex];
            }
        }

        private IEnumerable<DynamicEditorFront> SelectedFronts() =>
            _editor.Matrix.SelectedCells().Select(c => c.FrontIndex).Distinct().OrderBy(i => i).Select(i => _editor.Matrix.Fronts[i]);

        private DynamicFieldItem Number(string key, string label, string unit, IEnumerable<double> values, Func<double, bool> valid, string invalid, Action<double> set)
        {
            var list = values.ToList();
            return new DynamicFieldItem(key, label, unit, Same(list), Hint(list.Cast<object>()), text =>
            {
                if (!UiSupport.TryNum(text, out var value) || !valid(value)) return invalid;
                return Apply(() => set(value), null);
            }, !CanEdit);
        }

        private DynamicFieldItem OptionalNumber(string key, string label, string unit, double? current, string invalid, Action<double?> set) =>
            new(key, label, unit, current.HasValue ? Num(current.Value) : string.Empty, string.Empty, text =>
            {
                if (string.IsNullOrWhiteSpace(text)) return Apply(() => set(null), null);
                if (!UiSupport.TryNum(text, out var value) || value <= 0.0) return invalid;
                return Apply(() => set(value), null);
            }, !CanEdit);

        private DynamicFieldItem OptionalInt(string key, string label, int? current, string invalid, Action<int?> set) =>
            new(key, label, string.Empty, current.HasValue ? current.Value.ToString(CultureInfo.InvariantCulture) : string.Empty, string.Empty, text =>
            {
                if (string.IsNullOrWhiteSpace(text)) return Apply(() => set(null), null);
                if (!UiSupport.TryInt(text, out var value) || value < 1) return invalid;
                return Apply(() => set(value), null);
            }, !CanEdit);

        private DynamicFieldItem CellNumber(string key, string label, string unit, IReadOnlyList<DynamicEditorCell> cells, Func<DynamicEditorCell, double> get, Action<DynamicEditorCell, double> set, Func<double, bool> valid)
        {
            var values = cells.Select(get).ToList();
            return new DynamicFieldItem(key, label, unit, Same(values), Hint(values.Cast<object>()), text =>
            {
                if (!UiSupport.TryNum(text, out var value) || !valid(value)) return "Revisa los valores de tarima, claro y largueros de cada celda.";
                return Apply(() => { foreach (var cell in cells) set(cell, value); }, null);
            }, !CanEdit || cells.Count == 0);
        }

        private DynamicFieldItem CellOptional(string key, string label, string unit, IReadOnlyList<DynamicEditorCell> cells, Func<DynamicEditorCell, double?> get, Action<DynamicEditorCell, double?> set)
        {
            var values = cells.Select(get).ToList();
            var same = values.Distinct().Count() <= 1;
            var text0 = same && values.Count > 0 && values[0].HasValue ? Num(values[0].Value) : string.Empty;
            return new DynamicFieldItem(key, label, unit, text0, same ? string.Empty : Varies, text =>
            {
                if (string.IsNullOrWhiteSpace(text)) return Apply(() => { foreach (var cell in cells) set(cell, null); }, null);
                if (!UiSupport.TryNum(text, out var value) || value <= 0.0) return "Revisa los valores de tarima, claro y largueros de cada celda.";
                return Apply(() => { foreach (var cell in cells) set(cell, value); }, null);
            }, !CanEdit || cells.Count == 0);
        }

        // A peralte of a beam: only one that the catalog offers for the beam of every cell written (M-11 of the second review), as the list
        // of the classic editor. A value that the catalog does not offer is refused in its field, with the values it offers.
        private DynamicFieldItem CellPeralte(string key, string label, IReadOnlyList<DynamicEditorCell> cells, Func<DynamicEditorCell, string> beamOf, double fallback, Func<DynamicEditorCell, double> get, Action<DynamicEditorCell, double> set)
        {
            var values = cells.Select(get).ToList();
            return new DynamicFieldItem(key, label, "in", Same(values), Hint(values.Cast<object>()), text =>
            {
                if (!UiSupport.TryNum(text, out var value) || value <= 0.0) return "Revisa los valores de tarima, claro y largueros de cada celda.";
                foreach (var cell in cells)
                {
                    var allowed = DynamicRackLevelGeometry.AllowedPeraltes(_editor.Catalog, beamOf(cell));
                    if (allowed.Count == 0) allowed = new[] { fallback };
                    if (!allowed.Any(a => Math.Abs(a - value) < 1e-6))
                        return "El peralte debe ser uno del catálogo: " + string.Join(", ", allowed.Select(Num)) + ".";
                }

                return Apply(() => { foreach (var cell in cells) set(cell, value); }, null);
            }, !CanEdit || cells.Count == 0);
        }

        private DynamicFieldItem FrontInt(string key, string label, IReadOnlyList<DynamicEditorFront> fronts, Func<DynamicEditorFront, int> get, Action<DynamicEditorFront, int> set, int minimum)
        {
            var values = fronts.Select(get).ToList();
            var same = values.Distinct().Count() <= 1;
            var text0 = same && values.Count > 0 ? values[0].ToString(CultureInfo.InvariantCulture) : string.Empty;
            return new DynamicFieldItem(key, label, string.Empty, text0, same ? string.Empty : Varies, text =>
            {
                if (!UiSupport.TryInt(text, out var value) || value < minimum)
                    return "Debe ser un entero mayor o igual que " + minimum.ToString(CultureInfo.InvariantCulture) + ".";
                return Apply(() => { foreach (var front in fronts) set(front, value); _editor.Matrix.NormalizeSelection(); }, null);
            }, !CanEdit || fronts.Count == 0);
        }

        private DynamicFieldItem FrontNumber(string key, string label, string unit, IReadOnlyList<DynamicEditorFront> fronts, Func<DynamicEditorFront, double> get, Action<DynamicEditorFront, double> set, Func<double, bool> valid)
        {
            var values = fronts.Select(get).ToList();
            return new DynamicFieldItem(key, label, unit, Same(values), Hint(values.Cast<object>()), text =>
            {
                if (!UiSupport.TryNum(text, out var value) || !valid(value)) return "El inicio del primer larguero debe ser mayor o igual que cero.";
                return Apply(() => { foreach (var front in fronts) set(front, value); }, null);
            }, !CanEdit || fronts.Count == 0);
        }

        private static string Same(IReadOnlyList<double> values) =>
            values.Count > 0 && values.All(v => Math.Abs(v - values[0]) < 1e-9) ? Num(values[0]) : string.Empty;

        private static string Hint(IEnumerable<object> values) => values.Distinct().Count() > 1 ? Varies : string.Empty;

        private static string Num(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);
    }
}
