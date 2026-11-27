using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Views;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// One cell of the matrix of the Push Back editor: front × level of the side (or sides) shown, with one datum (the pallet, width ×
    /// height), «varía» when the two sides differ, and its marks: «T» (rear tope, question 1 of the prototype), «M» (an own value: a
    /// manual beam length or an own fondo) and, in a two-way rack, «A» / «B» / «C» (the beds of the cell, question 3). Refreshed in place.
    /// </summary>
    public sealed class PushBackCellItem : ObservableObject
    {
        private string _text = string.Empty;
        private bool _exists;
        private bool _isSelected;
        private bool _isPrimary;
        private bool _isBlankFront;
        private bool _hasTope;
        private bool _hasOwnValue;
        private string _beds = string.Empty;
        private string _accessibleText = string.Empty;

        public PushBackCellItem(int frontIndex, int levelIndex)
        {
            FrontIndex = frontIndex;
            LevelIndex = levelIndex;
        }

        public int FrontIndex { get; }

        public int LevelIndex { get; }

        /// <summary>«F2 · N3»: unique and with its position, for UI Automation.</summary>
        public string AutomationName => "Celda F" + (FrontIndex + 1) + " · N" + (LevelIndex + 1);

        public string Text { get => _text; private set => SetProperty(ref _text, value); }

        public bool Exists { get => _exists; private set => SetProperty(ref _exists, value); }

        public bool IsSelected { get => _isSelected; private set => SetProperty(ref _isSelected, value); }

        public bool IsPrimary { get => _isPrimary; private set => SetProperty(ref _isPrimary, value); }

        public bool IsBlankFront { get => _isBlankFront; private set => SetProperty(ref _isBlankFront, value); }

        public bool HasTope { get => _hasTope; private set => SetProperty(ref _hasTope, value); }

        public bool HasOwnValue { get => _hasOwnValue; private set => SetProperty(ref _hasOwnValue, value); }

        /// <summary>«A», «B» or «C» in a two-way rack (one bed in A, one in B, a run); empty for two beds or a one-way rack.</summary>
        public string Beds { get => _beds; private set { if (SetProperty(ref _beds, value ?? string.Empty)) OnPropertyChanged(nameof(HasBeds)); } }

        public bool HasBeds => _beds.Length > 0;

        /// <summary>What a screen reader says of the cell: its datum and every mark.</summary>
        public string AccessibleText { get => _accessibleText; private set => SetProperty(ref _accessibleText, value); }

        internal void Set(string text, bool exists, bool isSelected, bool isPrimary, bool isBlankFront, bool hasTope, bool hasOwnValue, string beds, string bedsText)
        {
            Text = text;
            Exists = exists;
            IsSelected = isSelected;
            IsPrimary = isPrimary;
            IsBlankFront = isBlankFront;
            HasTope = hasTope;
            HasOwnValue = hasOwnValue;
            Beds = beds;
            var marks = new List<string>();
            if (hasTope) marks.Add("con tope posterior");
            if (hasOwnValue) marks.Add("con un valor propio");
            if (!string.IsNullOrEmpty(bedsText)) marks.Add(bedsText);
            AccessibleText = "F" + (FrontIndex + 1) + " · N" + (LevelIndex + 1) + ": " + text + (marks.Count == 0 ? string.Empty : ", " + string.Join(", ", marks));
        }
    }

    /// <summary>One row of the matrix: «N3» … «N1», top to bottom.</summary>
    public sealed class PushBackMatrixRow
    {
        public PushBackMatrixRow(int levelIndex, string label, IReadOnlyList<PushBackCellItem> cells)
        {
            LevelIndex = levelIndex;
            Label = label;
            Cells = cells;
        }

        public int LevelIndex { get; }

        public string Label { get; }

        public string AutomationName => "Seleccionar el nivel " + Label;

        public IReadOnlyList<PushBackCellItem> Cells { get; }
    }

    /// <summary>One module of the longitudinal sequence in «Avanzado» › «Módulos»: its length is a field when it can be edited (question 7).</summary>
    public sealed class PushBackModuleItem
    {
        internal PushBackModuleItem(PushBackModuleRow row, DynamicFieldItem length, string header)
        {
            ModuleId = row.ModuleId;
            Label = row.Label;
            Kind = row.Kind;
            Start = Num(row.Start);
            LengthField = length;
            LengthText = Num(row.Length) + (row.IsManualLength ? " (propia)" : string.Empty);
            Header = header;
        }

        public string ModuleId { get; }

        public string Label { get; }

        public string Kind { get; }

        public string Start { get; }

        /// <summary>The field of the length, or null when the module cannot be edited.</summary>
        public DynamicFieldItem LengthField { get; }

        public bool HasLengthField => LengthField != null;

        public string LengthText { get; }

        public string Header { get; }

        private static string Num(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);
    }

    /// <summary>One side of «Estructura longitudinal por lado»: the proposal, what is drawn and the manual adjustment (a field).</summary>
    public sealed record PushBackStructureItem(string Side, string Proposed, string Effective, DynamicFieldItem Override);

    /// <summary>One box of «Restaurar layout…» (question 6): marked by default, off when it has nothing to give back.</summary>
    public sealed class PushBackRestoreBox : ObservableObject
    {
        private bool _isChecked = true;

        internal PushBackRestoreBox(string key, string label)
        {
            Key = key;
            Label = label;
        }

        public string Key { get; }

        public string Label { get; }

        public bool IsChecked { get => _isChecked; set => SetProperty(ref _isChecked, value); }
    }

    /// <summary>
    /// The editor of ONE Push Back rack in the RackCad window (I-64, fifth batch, part «pb»; the prototype «pushback», all its questions
    /// accepted): the tabs Rack, Matriz (the first one), Extremos y seguridad, Avanzado and Propiedades, over the draft
    /// <see cref="PushBackDraftEditor"/>, which recomposes with the pass of the classic editor.
    ///
    /// One way to confirm: what the user writes in a field goes to the draft when the field is left (or with Enter) and the rack is
    /// recomposed; a list, a box or a button goes at once; leaving a field without changing its text changes nothing. One scope: the
    /// selection of the matrix (a click, Ctrl+click, Mayús+click from the anchor, a column, a row or «Todo»), and, in a two-way rack, the
    /// side over the matrix: «Lado A», «Lado B» or «Ambos lados» (question 2: what is seen is where it is written). A value that cannot be
    /// recomposed is refused in its field and the draft stays as it was. The draft is «Modificado» only while it differs from what was last
    /// read from the drawing or written to it. Nothing here writes the drawing: «Actualizar» hands the editor to the host.
    /// </summary>
    public sealed partial class PushBackEditorViewModel : ObservableObject
    {
        private const string Varies = "Varía";
        private const string WaitingWriteText = "AutoCAD está escribiendo el rack: espera a que termine o pulsa «Dejar de esperar».";
        private const string TakenWriteText = "AutoCAD ya tomó la escritura del rack: espera a que termine, o termina o cancela la orden en AutoCAD; la ventana recibirá su respuesta.";

        /// <summary>The tabs, in the order of the prototype; «Matriz» is the first one shown.</summary>
        public const int RackTabIndex = 0;
        public const int MatrixTabIndex = 1;
        public const int SafetyTabIndex = 2;
        public const int AdvancedTabIndex = 3;
        public const int PropertiesTabIndex = 4;

        private static readonly RackProjectStore Store = new();

        private readonly PushBackDraftEditor _draft;
        private readonly Action<PushBackEditorViewModel> _update;
        private readonly HashSet<(int Front, int Level)> _selected = new();
        private (int Front, int Level) _primary;
        private (int Front, int Level) _anchor;
        private int _selectedTab = MatrixTabIndex;
        private bool _dirty;
        private bool _writing;
        private bool _writeTaken;
        private bool _confirmingClose;
        private bool _isNewRack;
        private string _baselineJson;
        private string _baselineName = string.Empty;
        private string _message = string.Empty;
        private bool _messageIsError;

        private IReadOnlyList<DynamicFieldItem> _essentials = Array.Empty<DynamicFieldItem>();
        private IReadOnlyList<DynamicFieldItem> _twoWayFields = Array.Empty<DynamicFieldItem>();
        private IReadOnlyList<DynamicFieldItem> _postFields = Array.Empty<DynamicFieldItem>();
        private IReadOnlyList<SelectiveChoiceItem> _postChoices = Array.Empty<SelectiveChoiceItem>();
        private IReadOnlyList<DynamicFieldItem> _cellFields = Array.Empty<DynamicFieldItem>();
        private IReadOnlyList<DynamicFieldItem> _depthFields = Array.Empty<DynamicFieldItem>();
        private IReadOnlyList<SelectiveChoiceItem> _beamChoices = Array.Empty<SelectiveChoiceItem>();
        private IReadOnlyList<DynamicFieldItem> _beamFields = Array.Empty<DynamicFieldItem>();
        private IReadOnlyList<SelectiveChoiceItem> _bedsChoices = Array.Empty<SelectiveChoiceItem>();
        private IReadOnlyList<DynamicFieldItem> _frontFields = Array.Empty<DynamicFieldItem>();
        private IReadOnlyList<DynamicFieldItem> _advancedFields = Array.Empty<DynamicFieldItem>();
        private IReadOnlyList<DynamicFieldItem> _topeFields = Array.Empty<DynamicFieldItem>();
        private IReadOnlyList<SelectiveChoiceItem> _topeChoices = Array.Empty<SelectiveChoiceItem>();
        private IReadOnlyList<DynamicCalcItem> _calculated = Array.Empty<DynamicCalcItem>();
        private IReadOnlyList<PushBackMatrixRow> _rows = Array.Empty<PushBackMatrixRow>();
        private IReadOnlyList<DynamicColumnHead> _columns = Array.Empty<DynamicColumnHead>();
        private IReadOnlyList<PushBackModuleItem> _modules = Array.Empty<PushBackModuleItem>();
        private IReadOnlyList<PushBackStructureItem> _structure = Array.Empty<PushBackStructureItem>();
        private IReadOnlyList<string> _extremes = Array.Empty<string>();

        /// <param name="draft">The draft of the rack, as it was read (its design is the baseline of «Modificado» and of «Descartar»).</param>
        /// <param name="rackId">The identity of the rack in the drawing (or the one the window gave a new rack).</param>
        /// <param name="update">What «Actualizar» does with this editor (the host writes the drawing); null when it is not offered.</param>
        /// <param name="isNewRack">A new rack of the window: not in the drawing until «Insertar en el dibujo…» places it.</param>
        public PushBackEditorViewModel(PushBackDraftEditor draft, string rackId, Action<PushBackEditorViewModel> update = null, bool isNewRack = false)
        {
            _draft = draft ?? throw new ArgumentNullException(nameof(draft));
            RackId = rackId;
            _update = update;
            _isNewRack = isNewRack;
            _selected.Add((0, 0));
            _primary = (0, 0);
            _anchor = (0, 0);
            TakeBaseline();
            Rebuild();
            if (!string.IsNullOrWhiteSpace(draft.LastError)) Say(draft.LastError, error: true);
        }

        /// <summary>«Cerrar el editor» was confirmed (or the draft had nothing to lose): the host forgets this editor.</summary>
        public event Action<PushBackEditorViewModel> CloseRequested;

        // ------------------------------------------------------------------ what the editor shows

        public PushBackDraftEditor Draft => _draft;

        public string RackId { get; }

        public string Name
        {
            get => _draft.Name ?? string.Empty;
            set
            {
                var next = value ?? string.Empty;
                if (string.Equals(_draft.Name ?? string.Empty, next, StringComparison.Ordinal)) return;
                if (!CanEdit)
                {
                    OnPropertyChanged(nameof(Name));
                    return;
                }

                _draft.Name = next;
                OnPropertyChanged(nameof(Name));
                UpdateDirty();
            }
        }

        /// <summary>0 = Rack, 1 = Matriz (the first one), 2 = Extremos y seguridad, 3 = Avanzado, 4 = Propiedades.</summary>
        public int SelectedTab
        {
            get => _selectedTab;
            set => SetProperty(ref _selectedTab, value);
        }

        public bool IsDirty => _dirty;

        public bool IsWriting => _writing;

        public bool CanEdit => !_writing;

        public bool IsNameReadOnly => !CanEdit;

        public bool CanDiscard => _dirty && CanEdit;

        public string StatusText => _writing ? "Escribiendo" : _dirty ? "Modificado" : "Limpio";

        public string Message => _message;

        public bool MessageIsError => _messageIsError;

        public bool IsNewRack => _isNewRack;

        public bool CanUpdate => _update != null && !_isNewRack && _dirty && CanEdit && !PropertiesBlockUpdate && _draft.Design != null
                                 && string.IsNullOrEmpty(_draft.LastError) && !_draft.IsOutputBlocked;

        public string UpdateBlockedText
        {
            get
            {
                if (_isNewRack) return "Un rack nuevo se escribe en el dibujo con «Insertar en el dibujo…».";
                if (_update == null) return "Aquí no se puede escribir el dibujo.";
                if (_writing) return WritingText;
                if (!_dirty) return "Sin cambios que escribir.";
                if (!string.IsNullOrEmpty(_draft.LastError)) return _draft.LastError;
                if (_draft.IsOutputBlocked) return "No se puede escribir: " + _draft.OutputStatus;
                if (PropertiesBlockUpdate) return PropertiesTab.BlockedText;
                return string.Empty;
            }
        }

        /// <summary>«Cerrar el editor» of a draft with changes asks first, inline.</summary>
        public bool IsConfirmingClose => _confirmingClose;

        public string WritingText => _writeTaken ? TakenWriteText : WaitingWriteText;

        // ------------------------------------------------------------------ the two sides (questions 2 and 3)

        /// <summary>The rack has a side B: the matrix shows «Lado A / Lado B / Ambos lados», and the cells carry their beds.</summary>
        public bool IsTwoSided => _draft.IsTwoSided;

        public bool IsSideA
        {
            get => !IsTwoSided || _draft.Composite.ActiveSelection == PushBackSideSelection.A;
            set { if (value) SetSides(PushBackSideSelection.A); }
        }

        public bool IsSideB
        {
            get => IsTwoSided && _draft.Composite.ActiveSelection == PushBackSideSelection.B;
            set { if (value) SetSides(PushBackSideSelection.B); }
        }

        public bool IsBothSides
        {
            get => IsTwoSided && _draft.Composite.ActiveSelection == PushBackSideSelection.Both;
            set { if (value) SetSides(PushBackSideSelection.Both); }
        }

        /// <summary>What the side over the matrix means, in a two-way rack.</summary>
        public string SidesHelp => !IsTwoSided ? string.Empty
            : IsBothSides ? "Ambos lados: lo que escribas va al lado A y al lado B. «varía» donde difieren; un nivel de un solo lado se escribe solo en ese lado."
            : IsSideB ? "Lado B: lo que ves es lo que escribes. Los frentes son del rack; los niveles y las celdas, de cada lado."
            : "Lado A: lo que ves es lo que escribes. Los frentes son del rack; los niveles y las celdas, de cada lado.";

        /// <summary>«Rack de dos sentidos (lado B)»: removing it keeps the configuration of the side B dormant.</summary>
        public bool TwoWay
        {
            get => IsTwoSided;
            set
            {
                if (value == IsTwoSided) return;
                Apply(() => _draft.SetSideBPresent(value), null);
                OnPropertyChanged(nameof(TwoWay));
            }
        }

        public bool CentralSeparator
        {
            get => _draft.Composite.CentralSeparator;
            set
            {
                if (value == _draft.Composite.CentralSeparator) return;
                Apply(() => _draft.Composite.SetCentralSeparator(value), null);
                OnPropertyChanged(nameof(CentralSeparator));
            }
        }

        public string CentralSeparatorHint => _draft.Composite.CentralSeparatorWithoutGap
            ? "Con hueco 0 no hay dónde poner el separador central: escribe un hueco mayor que cero, o quítalo."
            : "El separador central va en el hueco entre los dos lados.";

        private void SetSides(PushBackSideSelection selection)
        {
            if (!IsTwoSided || _draft.Composite.ActiveSelection == selection) return;
            _draft.SetSelection(selection);
            NormalizeSelection();
            RebuildSelection();
            RaiseSides();
        }

        private void RaiseSides()
        {
            OnPropertyChanged(nameof(IsTwoSided));
            OnPropertyChanged(nameof(IsSideA));
            OnPropertyChanged(nameof(IsSideB));
            OnPropertyChanged(nameof(IsBothSides));
            OnPropertyChanged(nameof(SidesHelp));
            OnPropertyChanged(nameof(TwoWay));
            OnPropertyChanged(nameof(CentralSeparator));
            OnPropertyChanged(nameof(CentralSeparatorHint));
        }

        // The sides the matrix shows: in a two-way rack the one of the selector, or both.
        private IReadOnlyList<PushBackSide> ViewSides() => _draft.EditSides();

        private bool ExistsInView(int front, int level) => ViewSides().Any(side => _draft.CellExists(side, front, level));

        // ------------------------------------------------------------------ lists of the tabs

        public IReadOnlyList<DynamicFieldItem> Essentials => _essentials;

        public IReadOnlyList<DynamicFieldItem> TwoWayFields => _twoWayFields;

        public IReadOnlyList<DynamicFieldItem> PostFields => _postFields;

        public IReadOnlyList<SelectiveChoiceItem> PostChoices => _postChoices;

        public IReadOnlyList<DynamicCalcItem> Calculated => _calculated;

        public IReadOnlyList<DynamicColumnHead> Columns => _columns;

        public IReadOnlyList<PushBackMatrixRow> Rows => _rows;

        public IReadOnlyList<DynamicFieldItem> CellFields => _cellFields;

        public IReadOnlyList<DynamicFieldItem> DepthFields => _depthFields;

        public IReadOnlyList<SelectiveChoiceItem> BeamChoices => _beamChoices;

        public IReadOnlyList<DynamicFieldItem> BeamFields => _beamFields;

        public IReadOnlyList<SelectiveChoiceItem> BedsChoices => _bedsChoices;

        public IReadOnlyList<DynamicFieldItem> FrontFields => _frontFields;

        public IReadOnlyList<DynamicFieldItem> AdvancedFields => _advancedFields;

        public IReadOnlyList<PushBackModuleItem> Modules => _modules;

        public string ModulesSummary => _modules.Count == 0 ? "Sin módulos todavía."
            : _modules.Count.ToString(CultureInfo.InvariantCulture) + " módulos · " + Num(_draft.System?.TotalLength ?? 0.0) + " in de largo.";

        public IReadOnlyList<PushBackStructureItem> StructureRows => _structure;

        public IReadOnlyList<DynamicFieldItem> TopeFields => _topeFields;

        public IReadOnlyList<SelectiveChoiceItem> TopeChoices => _topeChoices;

        /// <summary>«Extremos»: what the drawing places in each end, counted as the BOM counts it.</summary>
        public IReadOnlyList<string> ExtremeLines => _extremes;

        // ------------------------------------------------------------------ annotations of «Poste y anotaciones»

        public bool NumberFronts
        {
            get => _draft.State.NumberFronts;
            set { if (_draft.State.NumberFronts != value) Apply(() => _draft.State.NumberFronts = value, null); }
        }

        public bool NumberLevels
        {
            get => _draft.State.NumberLevels;
            set { if (_draft.State.NumberLevels != value) Apply(() => _draft.State.NumberLevels = value, null); }
        }

        public bool DrawRackName
        {
            get => _draft.State.DrawRackName;
            set { if (_draft.State.DrawRackName != value) Apply(() => _draft.State.DrawRackName = value, null); }
        }

        public bool DimensionsFrontal
        {
            get => _draft.State.DimensionsFrontal;
            set { if (_draft.State.DimensionsFrontal != value) Apply(() => { _draft.State.DimensionsFrontal = value; _draft.State.DimensionViewsTouched = true; }, null); }
        }

        public bool DimensionsLateral
        {
            get => _draft.State.DimensionsLateral;
            set { if (_draft.State.DimensionsLateral != value) Apply(() => { _draft.State.DimensionsLateral = value; _draft.State.DimensionViewsTouched = true; }, null); }
        }

        public bool DimensionsPlanta
        {
            get => _draft.State.DimensionsPlanta;
            set { if (_draft.State.DimensionsPlanta != value) Apply(() => { _draft.State.DimensionsPlanta = value; _draft.State.DimensionViewsTouched = true; }, null); }
        }

        /// <summary>«Tarimas de referencia» (question 12): in how many cells «Dibujar tarima» is marked.</summary>
        public string ReferencePalletsSummary
        {
            get
            {
                var cells = _draft.AllCells().ToList();
                var on = cells.Count(c => _draft.Side(c.Side).Cell(c.Front, c.Level).DrawPallet);
                if (cells.Count == 0) return "El rack no tiene celdas.";
                if (on == 0) return "Tarimas dibujadas en ninguna celda.";
                if (on == cells.Count) return "Tarimas dibujadas en todas las celdas (" + on.ToString(CultureInfo.InvariantCulture) + ").";
                return "Tarimas dibujadas en " + on.ToString(CultureInfo.InvariantCulture) + " de " + cells.Count.ToString(CultureInfo.InvariantCulture) + " celdas.";
            }
        }

        public void DrawPalletsEverywhere() => Apply(() => _draft.SetDrawPalletEverywhere(true), null);

        public void DrawPalletsNowhere() => Apply(() => _draft.SetDrawPalletEverywhere(false), null);

        // ------------------------------------------------------------------ the rear tope (question 1)

        /// <summary>«Tope posterior» of the inspector: true when every cell of the selection has it, false when none, null when they differ.</summary>
        public bool? SelectionTope
        {
            get
            {
                var cells = SelectedTargets().ToList();
                if (cells.Count == 0) return false;
                var on = cells.Count(c => c.Push.RearTopeEnabled);
                return on == cells.Count ? true : on == 0 ? false : (bool?)null;
            }
            set
            {
                var shown = SelectionTope;
                if (!value.HasValue || !SelectedTargets().Any()) { OnPropertyChanged(nameof(SelectionTope)); return; }
                // A mixed selection: any click puts the tope in every cell (as the guide of Dinámico, I-01 of the review of Selectivo).
                var put = shown.HasValue ? value.Value : true;
                if (put == shown) { OnPropertyChanged(nameof(SelectionTope)); return; }
                var cells = SelectedCells().ToList();
                Apply(() => _draft.SetTope(cells, put), null);
            }
        }

        public string SelectionTopeLabel => SelectionTope.HasValue ? "Tope posterior" : "Tope posterior (varía; marcar lo pone en todas)";

        public string TopeNote => "En el extremo alto" + (IsTwoSided ? " de cada lado" : string.Empty)
                                  + ". El tipo y el saque están en «Extremos y seguridad». No se dibuja en un frente en blanco"
                                  + (IsTwoSided ? " ni en el lado que las camas de la celda no tienen." : ".");

        /// <summary>«Tope posterior» of «Extremos y seguridad»: in how many cells it is drawn.</summary>
        public string TopeSummary
        {
            get
            {
                var cells = _draft.AllCells().ToList();
                var drawn = cells.Count(c => _draft.TopeDrawnAt(c.Side, c.Front, c.Level));
                if (cells.Count == 0) return "Tope posterior: el rack no tiene celdas.";
                if (drawn == 0) return "Tope posterior: en ninguna celda.";
                if (drawn == cells.Count) return "Tope posterior: en todas las celdas (" + drawn.ToString(CultureInfo.InvariantCulture) + ").";
                return "Tope posterior: en " + drawn.ToString(CultureInfo.InvariantCulture) + " de " + cells.Count.ToString(CultureInfo.InvariantCulture) + " celdas.";
            }
        }

        public void TopeEverywhere() => Apply(() => _draft.SetTopeEverywhere(true), null);

        public void TopeNowhere() => Apply(() => _draft.SetTopeEverywhere(false), null);

        // ------------------------------------------------------------------ the reference pallet of a cell (question 12)

        public bool? SelectionDrawPallet
        {
            get
            {
                var cells = SelectedTargets().ToList();
                if (cells.Count == 0) return false;
                var on = cells.Count(c => c.Push.DrawPallet);
                return on == cells.Count ? true : on == 0 ? false : (bool?)null;
            }
            set
            {
                var shown = SelectionDrawPallet;
                if (!value.HasValue || !SelectedTargets().Any()) { OnPropertyChanged(nameof(SelectionDrawPallet)); return; }
                var put = shown.HasValue ? value.Value : true;
                if (put == shown) { OnPropertyChanged(nameof(SelectionDrawPallet)); return; }
                var cells = SelectedCells().ToList();
                Apply(() => _draft.SetDrawPallet(cells, put), null);
            }
        }

        public string SelectionDrawPalletLabel => SelectionDrawPallet.HasValue ? "Dibujar tarima" : "Dibujar tarima (varía; marcar la pone en todas)";

        public bool CanEditSelection => CanEdit && SelectedTargets().Any();

        // ------------------------------------------------------------------ the inspector

        public string InspectorTitle
        {
            get
            {
                var cells = _selected.Where(c => ExistsInView(c.Front, c.Level)).ToList();
                if (cells.Count <= 1) return "Celda F" + (_primary.Front + 1) + " · N" + (_primary.Level + 1);
                var fronts = cells.Select(c => c.Front).Distinct().Count();
                return cells.Count.ToString(CultureInfo.InvariantCulture) + " celdas en " + fronts.ToString(CultureInfo.InvariantCulture)
                       + (fronts == 1 ? " frente" : " frentes");
            }
        }

        /// <summary>Where the inspector writes: the sides of the selector.</summary>
        public string TargetText => !IsTwoSided ? string.Empty
            : IsBothSides ? "Se escribe en los dos lados." : IsSideB ? "Se escribe en el lado B." : "Se escribe en el lado A.";

        /// <summary>Why part of the selection is not edited: the fronts in blank that it touches; empty otherwise.</summary>
        public string InspectorNote
        {
            get
            {
                var blank = SelectedFrontIndices().Where(f => !ViewSides().Any(side => _draft.IsPresent(side, f))).ToList();
                if (blank.Count == 0) return string.Empty;
                var names = string.Join(", ", blank.Select(f => "F" + (f + 1)));
                return blank.Count == 1
                    ? names + " está en blanco: sus niveles y sus celdas no se editan. Actívalo para volver a editarlo."
                    : names + " están en blanco: sus niveles y sus celdas no se editan. Actívalos para volver a editarlos.";
            }
        }

        /// <summary>The primary front of the selection is in blank in every side shown.</summary>
        public bool IsPrimaryFrontBlank => !ViewSides().Any(side => _draft.IsPresent(side, _primary.Front));

        public string FrontTitle => "Frente F" + (_primary.Front + 1);

        // ------------------------------------------------------------------ the selection of the matrix (the one scope)

        /// <summary>A click on a cell: one cell; with Ctrl, add or remove it; with Mayús, the rectangle from the anchor.</summary>
        public void ClickCell(int frontIndex, int levelIndex, bool control, bool shift)
        {
            if (frontIndex < 0 || frontIndex >= _draft.SlotCount || !ExistsInView(frontIndex, levelIndex))
            {
                if (frontIndex >= 0 && frontIndex < _draft.SlotCount && !control && !shift) SelectFront(frontIndex);
                return;
            }

            if (shift)
            {
                _selected.Clear();
                for (var f = Math.Min(_anchor.Front, frontIndex); f <= Math.Max(_anchor.Front, frontIndex); f++)
                    for (var l = Math.Min(_anchor.Level, levelIndex); l <= Math.Max(_anchor.Level, levelIndex); l++)
                        if (ExistsInView(f, l)) _selected.Add((f, l));
                _primary = (frontIndex, levelIndex);
                _selected.Add(_primary);
            }
            else if (control)
            {
                if (_selected.Contains((frontIndex, levelIndex)) && _selected.Count > 1)
                {
                    _selected.Remove((frontIndex, levelIndex));
                    if (_primary == (frontIndex, levelIndex)) _primary = _selected.First();
                }
                else
                {
                    _selected.Add((frontIndex, levelIndex));
                    _primary = (frontIndex, levelIndex);
                }

                _anchor = (frontIndex, levelIndex);
            }
            else
            {
                _selected.Clear();
                _selected.Add((frontIndex, levelIndex));
                _primary = (frontIndex, levelIndex);
                _anchor = _primary;
            }

            RebuildSelection();
        }

        /// <summary>«F1»…: every cell of that front (a front in blank: the front alone, for its fields).</summary>
        public void SelectFront(int frontIndex)
        {
            if (frontIndex < 0 || frontIndex >= _draft.SlotCount) return;
            _selected.Clear();
            for (var l = 0; l < _draft.MaxLevels(); l++) if (ExistsInView(frontIndex, l)) _selected.Add((frontIndex, l));
            _primary = (frontIndex, 0);
            _selected.Add(_primary);
            _anchor = _primary;
            RebuildSelection();
        }

        /// <summary>«N1»…: that level in every front that has it.</summary>
        public void SelectLevel(int levelIndex)
        {
            var cells = Enumerable.Range(0, _draft.SlotCount).Where(f => ExistsInView(f, levelIndex)).Select(f => (f, levelIndex)).ToList();
            if (cells.Count == 0) return;
            _selected.Clear();
            foreach (var cell in cells) _selected.Add(cell);
            _primary = cells[0];
            _anchor = _primary;
            RebuildSelection();
        }

        /// <summary>«Todo»: every cell of the rack, of the sides shown.</summary>
        public void SelectAll()
        {
            _selected.Clear();
            for (var f = 0; f < _draft.SlotCount; f++)
                for (var l = 0; l < _draft.MaxLevels(); l++)
                    if (ExistsInView(f, l)) _selected.Add((f, l));
            _primary = _selected.Count > 0 ? _selected.OrderBy(c => c.Front).ThenBy(c => c.Level).First() : (0, 0);
            _selected.Add(_primary);
            _anchor = _primary;
            RebuildSelection();
        }

        // The selection keeps only what exists in the sides shown (a reduction, another side); it is never empty.
        private void NormalizeSelection()
        {
            _selected.RemoveWhere(c => c.Front >= _draft.SlotCount || (!ExistsInView(c.Front, c.Level) && c != _primary));
            if (_primary.Front >= _draft.SlotCount) _primary = (_draft.SlotCount - 1, 0);
            if (!ExistsInView(_primary.Front, _primary.Level))
            {
                var level = Math.Max(0, Enumerable.Range(0, _draft.MaxLevels()).Where(l => ExistsInView(_primary.Front, l)).DefaultIfEmpty(0).Max());
                _selected.Remove(_primary);
                _primary = (_primary.Front, Math.Min(_primary.Level, level));
            }

            _selected.Add(_primary);
            if (_anchor.Front >= _draft.SlotCount) _anchor = _primary;
        }

        // ------------------------------------------------------------------ the gestures of the rack

        /// <summary>«Frentes»: the slots of the rack; new fronts copy the selected one, as in the classic editor.</summary>
        public string SetFrontCount(string text)
        {
            if (!UiSupport.TryInt(text, out var count) || count < 1) return "La cantidad de frentes debe ser un entero mayor o igual que 1.";
            return Apply(() =>
            {
                // The template of a new front is the front selected, as the matrix of the classic editor clones it.
                foreach (var side in _draft.Sides())
                {
                    var matrix = _draft.Side(side).Structure;
                    if (_primary.Front < matrix.Count) matrix.ToggleCell(_primary.Front, 0, extendSelection: false);
                }

                _draft.SetSlotCount(count);
            }, null);
        }

        /// <summary>«En blanco» of the primary front in the sides the selection writes («conserva claro y estructura, sin niveles»).</summary>
        public void SetFrontBlank(bool blank)
        {
            if (!CanEdit)
            {
                Say(WritingText, error: true);
                return;
            }

            string refusal = null;
            var front = _primary.Front;
            Apply(() =>
            {
                foreach (var side in _draft.EditSides())
                {
                    if (_draft.IsPresent(side, front) == !blank) continue;
                    refusal ??= _draft.SetBlank(side, front, blank);
                }

                if (refusal != null) throw new InvalidOperationException(refusal);
            }, null, refusalOnThrow: true);
        }

        /// <summary>«Descartar»: the draft goes back to what was last read from the drawing or written to it in this editor.</summary>
        public void Discard()
        {
            if (!CanEdit)
            {
                Say(WritingText, error: true);
                return;
            }

            PushBackDesign baseline;
            try
            {
                baseline = _baselineJson == null ? null : Store.Deserialize(_baselineJson)?.PushBackDesign;
            }
            catch (Exception ex)
            {
                Say("No se pudo volver a lo leído del dibujo: " + ex.Message + " El borrador sigue como está.", error: true);
                return;
            }

            if (baseline != null) _draft.LoadFrom(baseline, _baselineName);
            TakeBaseline();
            _propertiesTab?.Reset();
            _confirmingClose = false;
            CloseRestore();
            Say(RackId == null || _isNewRack
                ? "Borrador descartado."
                : "Borrador descartado: se muestra lo último que se leyó del dibujo o se escribió en él desde este editor.", error: false);
            NormalizeSelection();
            Rebuild();
            RaiseSides();
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

        public void ConfirmClose()
        {
            if (!_confirmingClose || !CanEdit) return;
            SetConfirmingClose(false);
            CloseRequested?.Invoke(this);
        }

        public void CancelClose() => SetConfirmingClose(false);

        /// <summary>The host handed this draft to AutoCAD: nothing can be edited until it answers or the window stops waiting.</summary>
        public void BeginWriting()
        {
            if (_writing) return;
            _writing = true;
            _writeTaken = false;
            SetConfirmingClose(false);
            _propertiesTab?.Lock(true);
            Say("Escribiendo el rack en el dibujo…", error: false);
            Rebuild();
        }

        /// <summary>The command of AutoCAD took the write: «Dejar de esperar» is no longer offered.</summary>
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
            _propertiesTab?.Lock(false);
            Rebuild();
        }

        /// <summary>The host wrote what was sent: that design and that name are the drawing now, and «Descartar» goes back to them.</summary>
        public void MarkWritten(string message, string writtenJson, string writtenName)
        {
            _baselineJson = writtenJson;
            _baselineName = (writtenName ?? string.Empty).Trim();
            Say(message, error: false);
            UpdateDirty();
            RaiseState();
        }

        /// <summary>AutoCAD placed the new rack: from now on it is a rack of the drawing, with the name the drawing gave it.</summary>
        public void MarkInserted(string name = null)
        {
            if (!_isNewRack) return;
            _isNewRack = false;
            if (!string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(_draft.Name))
            {
                _draft.Name = name.Trim();
                _baselineName = _draft.Name;
                OnPropertyChanged(nameof(Name));
                UpdateDirty();
            }

            OnPropertyChanged(nameof(IsNewRack));
            OnPropertyChanged(nameof(InsertViewLabel));
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

        // ------------------------------------------------------------------ internals: baseline, dirty, apply

        private string DesignJson() => _draft.DesignJson();

        private void TakeBaseline()
        {
            _baselineJson = DesignJson();
            _baselineName = (_draft.Name ?? string.Empty).Trim();
            _dirty = false;
            RaiseState();
        }

        // «Modificado» is a difference with what the drawing has (as last read or written), never a gesture.
        private void UpdateDirty()
        {
            var dirty = !string.Equals(DesignJson(), _baselineJson, StringComparison.Ordinal)
                        || !string.Equals((_draft.Name ?? string.Empty).Trim(), _baselineName, StringComparison.Ordinal)
                        || PropertiesDirty;
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

        // Every change of the draft: a copy of the state, the change, the recomposition; a change that cannot be recomposed is undone
        // and its reason goes back to the field. With refusalOnThrow an exception of the change is its refusal (said, nothing written).
        private string Apply(Action change, string fieldError, bool refusalOnThrow = false)
        {
            if (fieldError != null) return fieldError;
            if (!CanEdit)
            {
                Say(WritingText, error: true);
                return WritingText;
            }

            var copy = _draft.Capture();
            try
            {
                change();
            }
            catch (InvalidOperationException ex) when (refusalOnThrow)
            {
                _draft.Restore(copy);
                Say(ex.Message, error: true);
                Rebuild();
                return ex.Message;
            }

            if (!_draft.Recompose())
            {
                var reason = _draft.LastError;
                _draft.Restore(copy);
                // The draft is back to its last valid recomposition: the refused value does not block «Actualizar» (I-09 of Dinámico).
                _draft.InputsRestoredToLastValid();
                Say(reason, error: true);
                NormalizeSelection();
                Rebuild();
                return reason;
            }

            Say(_draft.OutputStatus ?? string.Empty, error: _draft.IsOutputBlocked);
            NormalizeSelection();
            Rebuild();
            UpdateDirty();
            return null;
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
            OnPropertyChanged(nameof(CanEditSelection));
            OnPropertyChanged(nameof(CanInsertView));
            OnPropertyChanged(nameof(InsertViewBlockedText));
            RaiseStateIns();
            RaiseRestore();
        }

        // The lists keep their items: a recomposition refreshes them in place, so the focus and the button under the mouse survive it.
        private void Rebuild()
        {
            Merge(ref _essentials, BuildEssentials(), nameof(Essentials));
            Merge(ref _twoWayFields, BuildTwoWayFields(), nameof(TwoWayFields));
            Merge(ref _postFields, BuildPostFields(), nameof(PostFields));
            MergeChoices(ref _postChoices, BuildPostChoices(), nameof(PostChoices));
            Merge(ref _advancedFields, BuildAdvancedFields(), nameof(AdvancedFields));
            Merge(ref _topeFields, BuildTopeFields(), nameof(TopeFields));
            MergeChoices(ref _topeChoices, BuildTopeChoices(), nameof(TopeChoices));
            _calculated = BuildCalculated();
            _modules = BuildModules();
            _structure = BuildStructure();
            _extremes = BuildExtremes();
            OnPropertyChanged(nameof(Calculated));
            OnPropertyChanged(nameof(Modules));
            OnPropertyChanged(nameof(ModulesSummary));
            OnPropertyChanged(nameof(StructureRows));
            OnPropertyChanged(nameof(ExtremeLines));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(NumberFronts));
            OnPropertyChanged(nameof(NumberLevels));
            OnPropertyChanged(nameof(DrawRackName));
            OnPropertyChanged(nameof(DimensionsFrontal));
            OnPropertyChanged(nameof(DimensionsLateral));
            OnPropertyChanged(nameof(DimensionsPlanta));
            OnPropertyChanged(nameof(DerivedPostReinforced));
            OnPropertyChanged(nameof(ReferencePalletsSummary));
            OnPropertyChanged(nameof(TopeSummary));
            OnPropertyChanged(nameof(TopeNote));
            RaiseSides();
            RebuildSelection();
            RefreshInsertPanel();
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

        private void MergeChoices(ref IReadOnlyList<SelectiveChoiceItem> current, IReadOnlyList<SelectiveChoiceItem> next, string propertyName)
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
            var slots = _draft.SlotCount;
            var maxLevels = _draft.MaxLevels();
            if (_columns.Count != slots || _rows.Count != maxLevels)
            {
                _columns = Enumerable.Range(0, slots).Select(f => new DynamicColumnHead(f, "F" + (f + 1))).ToList();
                _rows = Enumerable.Range(0, maxLevels).Reverse()
                    .Select(l => new PushBackMatrixRow(l, "N" + (l + 1), Enumerable.Range(0, slots).Select(f => new PushBackCellItem(f, l)).ToList()))
                    .ToList();
                OnPropertyChanged(nameof(Columns));
                OnPropertyChanged(nameof(Rows));
            }

            var sides = ViewSides();
            foreach (var column in _columns) column.IsBlank = !sides.Any(side => _draft.IsPresent(side, column.FrontIndex));
            foreach (var row in _rows)
            {
                foreach (var item in row.Cells)
                {
                    var f = item.FrontIndex;
                    var l = item.LevelIndex;
                    var having = sides.Where(side => _draft.CellExists(side, f, l)).ToList();
                    var exists = having.Count > 0;
                    var blank = !sides.Any(side => _draft.IsPresent(side, f));
                    var text = "—";
                    var tope = false;
                    var own = false;
                    if (exists)
                    {
                        var cells = having.Select(side => _draft.Side(side).Structure.Fronts[f].Cells[l]).ToList();
                        text = cells.Select(c => Num(c.PalletFront) + "×" + Num(c.PalletHeight)).Distinct().Count() > 1
                            ? "varía"
                            : Num(cells[0].PalletFront) + "×" + Num(cells[0].PalletHeight);
                        if (IsBothSides && having.Count == 1) text += having[0] == PushBackSide.A ? " (A)" : " (B)";
                        tope = having.Any(side => _draft.TopeDrawnAt(side, f, l));
                        own = having.Any(side => _draft.Side(side).Structure.Fronts[f].Cells[l].BeamLengthOverride.HasValue
                                                 || _draft.CellFondoAt(side, f, l).HasValue);
                    }

                    var beds = exists && IsTwoSided ? BedsMark(_draft.BedsAt(f, l)) : (Mark: string.Empty, Text: string.Empty);
                    item.Set(text, exists, exists && _selected.Contains((f, l)), exists && _primary == (f, l), blank, tope, own, beds.Mark, beds.Text);
                }
            }

            Merge(ref _cellFields, BuildCellFields(), nameof(CellFields));
            Merge(ref _depthFields, BuildDepthFields(), nameof(DepthFields));
            MergeChoices(ref _beamChoices, BuildBeamChoices(), nameof(BeamChoices));
            Merge(ref _beamFields, BuildBeamFields(), nameof(BeamFields));
            MergeChoices(ref _bedsChoices, BuildBedsChoices(), nameof(BedsChoices));
            Merge(ref _frontFields, BuildFrontFields(), nameof(FrontFields));
            OnPropertyChanged(nameof(SelectionTope));
            OnPropertyChanged(nameof(SelectionTopeLabel));
            OnPropertyChanged(nameof(SelectionDrawPallet));
            OnPropertyChanged(nameof(SelectionDrawPalletLabel));
            OnPropertyChanged(nameof(CanEditSelection));
            OnPropertyChanged(nameof(InspectorTitle));
            OnPropertyChanged(nameof(InspectorNote));
            OnPropertyChanged(nameof(TargetText));
            OnPropertyChanged(nameof(IsPrimaryFrontBlank));
            OnPropertyChanged(nameof(FrontTitle));
        }

        private static (string Mark, string Text) BedsMark(PushBackCellBeds beds)
        {
            switch (beds)
            {
                case PushBackCellBeds.SoloA: return ("A", "una cama, solo en el lado A");
                case PushBackCellBeds.SoloB: return ("B", "una cama, solo en el lado B");
                case PushBackCellBeds.CorridaAToB: return ("C", "cama corrida de A a B");
                case PushBackCellBeds.CorridaBToA: return ("C", "cama corrida de B a A");
                default: return (string.Empty, string.Empty);
            }
        }

        // ------------------------------------------------------------------ targets

        private sealed record CellTarget(PushBackSide Side, int Front, int Level, DynamicEditorCell Cell, PushBackEditorCell Push);

        private sealed record FrontTarget(PushBackSide Side, int Front, DynamicEditorFront Row, bool IsPresent);

        // The cells of the selection in the sides it writes (only those that exist there).
        private IEnumerable<CellTarget> SelectedTargets() => Targets(_selected, _draft.EditSides());

        // Every cell of the rack, of every side («Lo esencial»).
        private IEnumerable<CellTarget> AllTargets() => _draft.AllCells().Select(c => Target(c.Side, c.Front, c.Level));

        private IEnumerable<CellTarget> Targets(IEnumerable<(int Front, int Level)> cells, IEnumerable<PushBackSide> sides)
        {
            foreach (var side in sides)
                foreach (var (front, level) in cells.OrderBy(c => c.Front).ThenBy(c => c.Level))
                    if (_draft.CellExists(side, front, level)) yield return Target(side, front, level);
        }

        private CellTarget Target(PushBackSide side, int front, int level)
        {
            var row = _draft.Side(side).Structure.Fronts[front];
            row.EnsureCellCount(row.LoadLevels);
            return new CellTarget(side, front, level, row.Cells[level], _draft.Side(side).Cell(front, level));
        }

        private IEnumerable<(int Front, int Level)> SelectedCells() => _selected.Where(c => ExistsInView(c.Front, c.Level));

        private IEnumerable<int> SelectedFrontIndices() => _selected.Select(c => c.Front).Concat(new[] { _primary.Front }).Distinct().OrderBy(f => f);

        private IEnumerable<FrontTarget> FrontTargets(IEnumerable<int> fronts, IEnumerable<PushBackSide> sides)
        {
            foreach (var side in sides)
            {
                var matrix = _draft.Side(side).Structure;
                foreach (var front in fronts)
                    if (front >= 0 && front < matrix.Count) yield return new FrontTarget(side, front, matrix.Fronts[front], _draft.IsPresent(side, front));
            }
        }

        // ------------------------------------------------------------------ «Lo esencial» and the rest of «Rack»

        // «Lo esencial»: the whole rack, both sides. A front in blank keeps its levels and cells dormant (I-33).
        private IReadOnlyList<DynamicFieldItem> BuildEssentials()
        {
            var cells = AllTargets().ToList();
            var allFronts = Enumerable.Range(0, _draft.SlotCount).ToList();
            var fronts = FrontTargets(allFronts, _draft.Sides()).ToList();
            var present = fronts.Where(f => f.IsPresent).ToList();
            var pallet = _draft.State.Pallet;
            return new[]
            {
                Number("depth", "Profundidad de tarima", "in", new[] { pallet.Depth }, v => v > 0.0, "Fondo inválido.",
                    v => _draft.State.Pallet = new PalletSpecification(pallet.Front, v, pallet.Height, pallet.Weight, pallet.WeightUnit)),
                CellNumber("front", "Ancho de tarima", "in", cells, c => c.PalletFront, (c, v) => c.PalletFront = v, v => v > 0.0, allSides: true),
                CellNumber("height", "Alto de tarima", "in", cells, c => c.PalletHeight, (c, v) => c.PalletHeight = v, v => v > 0.0, allSides: true),
                CellNumber("weight", "Peso de tarima", pallet.WeightUnit ?? "kg", cells, c => c.PalletWeight, (c, v) => c.PalletWeight = v, v => v >= 0.0, allSides: true),
                new DynamicFieldItem("fronts", "Frentes", string.Empty, _draft.SlotCount.ToString(CultureInfo.InvariantCulture), string.Empty, SetFrontCount, !CanEdit),
                FrontInt("positions", "Carriles por frente", fronts, f => f.Row.PalletCount, 1, (targets, v) => _draft.SetPositions(targets, v, allSides: true)),
                FrontInt("levels", "Niveles", present, f => f.Row.LoadLevels, 1, (targets, v) => _draft.SetLevels(targets, v, allSides: true)),
                FrontInt("deep", "Posiciones de fondo", fronts, f => f.Row.PalletsDeep, 2,
                    (targets, v) => _draft.WriteFronts(targets, (_, state, front) => state.Structure.Fronts[front].PalletsDeep = v, allSides: true)),
                FrontNumber("first", "Alto del 1er nivel", "in", present, f => f.Row.FirstLevelHeight, v => v >= 0.0,
                    "El inicio del primer larguero debe ser mayor o igual que cero.",
                    (targets, v) => _draft.WriteFronts(targets, (side, state, front) => { if (_draft.IsPresent(side, front)) state.Structure.Fronts[front].FirstLevelHeight = v; }, allSides: true)),
                CellNumber("clear", "Claro sobre la tarima", "in", cells, c => c.ClearHeight, (c, v) => c.ClearHeight = v, v => v >= 0.0, allSides: true),
                Number("tolerance", "Tolerancia", "in", new[] { _draft.State.PalletTolerance }, v => v > 0.0, "La tolerancia debe ser un número mayor que cero.",
                    v => _draft.State.PalletTolerance = v),
            };
        }

        // «Dos sentidos (lado B)»: the gap between the two sides.
        private IReadOnlyList<DynamicFieldItem> BuildTwoWayFields() => !IsTwoSided
            ? Array.Empty<DynamicFieldItem>()
            : new[]
            {
                Number("gap", "Hueco entre lados", "in", new[] { _draft.Composite.Gap }, v => v >= 0.0, "El hueco debe ser un número mayor o igual que cero.",
                    v => _draft.Composite.SetGap(v)),
            };

        private IReadOnlyList<DynamicFieldItem> BuildPostFields() => new[]
        {
            Number("peralte", "Peralte de poste (0 = el de la cabecera)", "in", new[] { _draft.State.PostPeralte }, v => v >= 0.0,
                "El peralte de poste debe ser un número mayor o igual que cero.", v => _draft.State.PostPeralte = v),
            Number("scale", "Escala de texto (1 = 6\")", string.Empty, new[] { _draft.State.AnnotationScale }, v => v > 0.0,
                "La escala de texto debe ser mayor que cero.", v => _draft.State.AnnotationScale = v),
        };

        private IReadOnlyList<SelectiveChoiceItem> BuildPostChoices()
        {
            var posts = (_draft.Catalog.PostProfiles ?? Array.Empty<RackCad.Application.Catalogs.ProfileCatalogEntry>())
                .Where(p => p != null && !string.IsNullOrWhiteSpace(p.Id))
                .GroupBy(p => p.Id.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => new SelectiveChoiceOption(g.First().Id.Trim(), g.First().Label))
                .ToList();
            var dims = new[] { "Ninguna", "Mínima", "Estándar", "Detallada" }
                .Select((label, index) => new SelectiveChoiceOption(index.ToString(CultureInfo.InvariantCulture), label)).ToList();
            var units = new[] { new SelectiveChoiceOption("kg", "kg"), new SelectiveChoiceOption("lb", "lb") };
            var pallet = _draft.State.Pallet;
            return new[]
            {
                new SelectiveChoiceItem("post", "Poste", posts, _draft.State.PostCatalogId, string.Empty,
                    id => Apply(() => _draft.State.PostCatalogId = id, null), !CanEdit),
                new SelectiveChoiceItem("dims", "Cotas", dims, ((int)_draft.State.Dimensions).ToString(CultureInfo.InvariantCulture), string.Empty,
                    value => Apply(() => _draft.State.Dimensions = (DimensionDetail)int.Parse(value, CultureInfo.InvariantCulture), null), !CanEdit),
                new SelectiveChoiceItem("unit", "Unidad del peso", units, pallet.WeightUnit ?? "kg", string.Empty,
                    unit => Apply(() => _draft.State.Pallet = new PalletSpecification(pallet.Front, pallet.Depth, pallet.Height, pallet.Weight, unit), null), !CanEdit),
            };
        }

        private IReadOnlyList<DynamicCalcItem> BuildCalculated()
        {
            var system = _draft.System;
            var structure = system?.Structure;
            var items = new List<DynamicCalcItem>
            {
                new("Altura de cabecera", structure == null ? "—"
                    : Num(DynamicFrontGeometry.Height(structure)) + " in" + (_draft.State.Advanced.ManualHeaderHeightOverride.HasValue ? " (manual)" : " (la más alta)")),
                new("Frentes", _draft.SlotCount.ToString(CultureInfo.InvariantCulture)),
                new("Niveles máximos", _draft.MaxLevels().ToString(CultureInfo.InvariantCulture)),
                new("Largo del rack", system == null ? "—" : Num(system.TotalLength) + " in"),
                new("Módulos", structure == null ? "—" : structure.Modules.Count.ToString(CultureInfo.InvariantCulture)),
            };
            if (IsTwoSided) items.Add(new DynamicCalcItem("Sentidos", "Dos (lado A y lado B)"));
            var status = _draft.OutputStatus;
            if (!string.IsNullOrWhiteSpace(status)) items.Add(new DynamicCalcItem("Aviso", status));
            return items;
        }

        // ------------------------------------------------------------------ the inspector

        private IReadOnlyList<DynamicFieldItem> BuildCellFields()
        {
            var cells = SelectedTargets().ToList();
            return new[]
            {
                CellNumber("cFront", "Ancho de tarima", "in", cells, c => c.PalletFront, (c, v) => c.PalletFront = v, v => v > 0.0),
                CellNumber("cHeight", "Alto de tarima", "in", cells, c => c.PalletHeight, (c, v) => c.PalletHeight = v, v => v > 0.0),
                CellNumber("cWeight", "Peso de tarima", _draft.State.Pallet.WeightUnit ?? "kg", cells, c => c.PalletWeight, (c, v) => c.PalletWeight = v, v => v >= 0.0),
                CellNumber("cClear", "Claro sobre la tarima", "in", cells, c => c.ClearHeight, (c, v) => c.ClearHeight = v, v => v >= 0.0),
            };
        }

        // «Fondo de la celda»: the own positions of fondo of the cells (a run cell: of its bed); empty = the ones of its front.
        private IReadOnlyList<DynamicFieldItem> BuildDepthFields()
        {
            var cells = SelectedTargets().ToList();
            var values = cells.Select(c => _draft.CellFondoAt(c.Side, c.Front, c.Level)).ToList();
            var same = values.Distinct().Count() <= 1;
            var text = same && values.Count > 0 && values[0].HasValue ? values[0].Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
            var inherited = cells.Select(c => _draft.Side(c.Side).Structure.Fronts[c.Front].PalletsDeep).Distinct().ToList();
            var hint = !same ? Varies : inherited.Count == 1 ? "Vacío = las del frente (" + inherited[0].ToString(CultureInfo.InvariantCulture) + ")." : "Vacío = las de su frente.";
            var addresses = SelectedCells().ToList();
            return new[]
            {
                new DynamicFieldItem("cFondo", "Posiciones de fondo de la celda", string.Empty, text, hint, input =>
                {
                    if (string.IsNullOrWhiteSpace(input)) return Apply(() => _draft.SetCellFondo(addresses, null), null);
                    if (!UiSupport.TryInt(input, out var deep) || deep < PushBackCellDepth.MinimumPalletsDeep)
                        return "Debe ser un entero mayor o igual que " + PushBackCellDepth.MinimumPalletsDeep.ToString(CultureInfo.InvariantCulture) + ", o vacío.";
                    return Apply(() => _draft.SetCellFondo(addresses, deep), null);
                }, !CanEdit || cells.Count == 0),
            };
        }

        // «Largueros»: the beams of the cells and their peraltes, from the catalog, as the lists of the classic editor.
        private IReadOnlyList<SelectiveChoiceItem> BuildBeamChoices()
        {
            var cells = SelectedTargets().ToList();
            var readOnly = !CanEdit || cells.Count == 0;
            var inOut = DynamicRackLevelGeometry.CompatibleInOutBeams(_draft.Catalog).Select(b => new SelectiveChoiceOption(b.Id, b.Label)).ToList();
            var inter = DynamicRackLevelGeometry.CompatibleIntermediateBeams(_draft.Catalog).Select(b => new SelectiveChoiceOption(b.Id, b.Label)).ToList();
            var rear = _draft.AllowedRearPeraltes().Select(p => new SelectiveChoiceOption(Num(p), Num(p) + " in")).ToList();
            return new[]
            {
                Choice("cInOutBeam", "Larguero IN/OUT", inOut, cells, c => c.Cell.InOutBeamCatalogId, readOnly, id => (c =>
                {
                    c.Cell.InOutBeamCatalogId = id;
                    c.Cell.InOutBeamDepth = KeepAllowed(id, c.Cell.InOutBeamDepth, DynamicRackDefaults.DefaultBeamDepth);
                })),
                PeralteChoice("cInOutPeralte", "Peralte IN/OUT", cells, c => c.Cell.InOutBeamCatalogId, DynamicRackDefaults.DefaultBeamDepth,
                    c => c.Cell.InOutBeamDepth, (c, v) => c.Cell.InOutBeamDepth = v, readOnly),
                Choice("cInterBeam", "Larguero intermedio", inter, cells, c => c.Cell.IntermediateBeamCatalogId, readOnly, id => (c =>
                {
                    c.Cell.IntermediateBeamCatalogId = id;
                    c.Cell.IntermediateBeamDepth = KeepAllowed(id, c.Cell.IntermediateBeamDepth, DynamicRackDefaults.DefaultIntermediateBeamDepth);
                })),
                PeralteChoice("cInterPeralte", "Peralte intermedio", cells, c => c.Cell.IntermediateBeamCatalogId, DynamicRackDefaults.DefaultIntermediateBeamDepth,
                    c => c.Cell.IntermediateBeamDepth, (c, v) => c.Cell.IntermediateBeamDepth = v, readOnly),
                Choice("cRearPeralte", "Peralte posterior", rear, cells, c => Num(c.Push.HighEndBeamPeralte), readOnly,
                    value => (c => c.Push.HighEndBeamPeralte = double.Parse(value, CultureInfo.InvariantCulture))),
            };
        }

        // A new beam keeps its peralte when the catalog offers it for that beam, and takes the first one it offers otherwise, as the list of
        // the classic editor does when the beam changes (SetPeralteOptions).
        private double KeepAllowed(string beamId, double peralte, double fallback)
        {
            var allowed = DynamicRackLevelGeometry.AllowedPeraltes(_draft.Catalog, beamId);
            var options = allowed.Count > 0 ? allowed : new[] { fallback };
            return options.Any(a => Math.Abs(a - peralte) < 1e-6) ? peralte : options[0];
        }

        private IReadOnlyList<DynamicFieldItem> BuildBeamFields()
        {
            var cells = SelectedTargets().ToList();
            var values = cells.Select(c => c.Cell.BeamLengthOverride).ToList();
            var same = values.Distinct().Count() <= 1;
            var text = same && values.Count > 0 && values[0].HasValue ? Num(values[0].Value) : string.Empty;
            return new[]
            {
                new DynamicFieldItem("cLength", "Largo manual (vacío = calculado)", "in", text, same ? string.Empty : Varies, input =>
                {
                    if (string.IsNullOrWhiteSpace(input)) return Apply(() => WriteSelected((c, _) => c.BeamLengthOverride = null), null);
                    if (!UiSupport.TryNum(input, out var value) || value <= 0.0) return "Revisa los valores de tarima, claro y largueros de cada celda.";
                    return Apply(() => WriteSelected((c, _) => c.BeamLengthOverride = value), null);
                }, !CanEdit || cells.Count == 0),
            };
        }

        // «Camas de la celda» (question 3), only in a two-way rack: one list that joins topology and direction.
        private IReadOnlyList<SelectiveChoiceItem> BuildBedsChoices()
        {
            if (!IsTwoSided) return Array.Empty<SelectiveChoiceItem>();
            var addresses = SelectedCells().ToList();
            var options = new[]
            {
                new SelectiveChoiceOption(nameof(PushBackCellBeds.Encontradas), "Encontradas (dos camas)"),
                new SelectiveChoiceOption(nameof(PushBackCellBeds.SoloA), "Solo A"),
                new SelectiveChoiceOption(nameof(PushBackCellBeds.SoloB), "Solo B"),
                new SelectiveChoiceOption(nameof(PushBackCellBeds.CorridaAToB), "Corrida A → B"),
                new SelectiveChoiceOption(nameof(PushBackCellBeds.CorridaBToA), "Corrida B → A"),
            };
            var values = addresses.Select(c => _draft.BedsAt(c.Front, c.Level).ToString()).Distinct().ToList();
            return new[]
            {
                new SelectiveChoiceItem("cBeds", "Camas de la celda", options, values.Count == 1 ? values[0] : null, values.Count > 1 ? Varies : string.Empty,
                    value => Enum.TryParse<PushBackCellBeds>(value, out var beds) ? Apply(() => _draft.SetBeds(addresses, beds), null) : "Elige una de la lista.",
                    !CanEdit || addresses.Count == 0),
            };
        }

        public string BedsHint => !IsTwoSided ? string.Empty
            : "Encontradas: una cama en cada lado, con sus extremos altos al centro. Una corrida cruza el rack, de un lado al otro. Un frente sin uno de los lados solo admite el otro.";

        // «Frente»: the fronts that the selection touches, in the sides it writes; the levels and the first beam only of those present.
        private IReadOnlyList<DynamicFieldItem> BuildFrontFields()
        {
            var fronts = FrontTargets(SelectedFrontIndices(), _draft.EditSides()).ToList();
            var present = fronts.Where(f => f.IsPresent).ToList();
            var indices = SelectedFrontIndices().ToList();
            return new[]
            {
                FrontInt("fPositions", "Carriles por frente", fronts, f => f.Row.PalletCount, 1, (_, v) => _draft.SetPositions(indices, v)),
                FrontInt("fLevels", "Niveles", present, f => f.Row.LoadLevels, 1, (_, v) => _draft.SetLevels(indices, v)),
                FrontInt("fDeep", "Posiciones de fondo", fronts, f => f.Row.PalletsDeep, 2,
                    (_, v) => _draft.WriteFronts(indices, (__, state, front) => state.Structure.Fronts[front].PalletsDeep = v)),
                FrontInt("fStart", "Empieza en la posición", fronts, f => f.Row.DepthStartPosition, 1,
                    (_, v) => _draft.WriteFronts(indices, (__, state, front) => state.Structure.Fronts[front].DepthStartPosition = v)),
                FrontNumber("fFirst", "Alto del 1er nivel", "in", present, f => f.Row.FirstLevelHeight, v => v >= 0.0,
                    "El inicio del primer larguero debe ser mayor o igual que cero.",
                    (_, v) => _draft.WriteFronts(indices, (side, state, front) => { if (_draft.IsPresent(side, front)) state.Structure.Fronts[front].FirstLevelHeight = v; })),
            };
        }

        // ------------------------------------------------------------------ «Avanzado»

        public bool DerivedPostReinforced
        {
            get => _draft.State.Advanced.DerivedPostReinforced;
            set { if (_draft.State.Advanced.DerivedPostReinforced != value) Apply(() => _draft.State.Advanced.DerivedPostReinforced = value, null); }
        }

        private IReadOnlyList<DynamicFieldItem> BuildAdvancedFields()
        {
            var a = _draft.State.Advanced;
            return new[]
            {
                OptionalNumber("manual", "Altura de cabecera (vacío = calculada)", "in", a.ManualHeaderHeightOverride, "Altura manual inválida (debe ser mayor que cero).", v => a.ManualHeaderHeightOverride = v),
                OptionalNumber("derived", "Altura del poste derivado (vacío = la de la cabecera)", "in", a.DerivedPostHeight, "La altura del poste derivado debe ser mayor que cero, o vacía.", v => a.DerivedPostHeight = v),
                OptionalNumber("reinforcement", "Altura del refuerzo (vacío = toda la altura)", "in", a.DerivedPostReinforcementHeight, "Altura de refuerzo del poste derivado inválida (deja vacío para altura completa).", v => a.DerivedPostReinforcementHeight = v),
                OptionalInt("sepCount", "Separadores (cantidad; vacío = cálculo)", a.SeparatorCountOverride, "Cantidad de separadores inválida (entero >= 1, o vacío para el cálculo).", v => a.SeparatorCountOverride = v),
                OptionalNumber("sepSpacing", "Separación de separadores (vacío = cálculo)", "in", a.SeparatorSpacingOverride, "Separación de separadores inválida (deja vacío para el cálculo).", v => a.SeparatorSpacingOverride = v),
            };
        }

        private IReadOnlyList<PushBackModuleItem> BuildModules()
        {
            var rows = _draft.ModuleRows();
            var items = new List<PushBackModuleItem>(rows.Count);
            foreach (var row in rows)
            {
                var id = row.ModuleId;
                var field = row.CanEditLength
                    ? new DynamicFieldItem("mod:" + id, row.Label + ": longitud", "in", row.IsManualLength ? Num(row.Length) : string.Empty,
                        row.IsManualLength ? string.Empty : "Calculada: " + Num(row.Length), text =>
                        {
                            if (string.IsNullOrWhiteSpace(text)) return Apply(() => Refuse(_draft.SetModuleLength(id, null)), null, refusalOnThrow: true);
                            if (!UiSupport.TryNum(text, out var length) || length <= 0.0) return "La longitud debe ser un número mayor que cero, o vacía para la calculada.";
                            return Apply(() => Refuse(_draft.SetModuleLength(id, length)), null, refusalOnThrow: true);
                        }, !CanEdit)
                    : null;
                var header = !row.IsHeader ? "—" : row.IsCustomHeader ? "Personalizada" : "Calculada";
                items.Add(new PushBackModuleItem(row, field, header));
            }

            return items;
        }

        private static void Refuse(string refusal)
        {
            if (refusal != null) throw new InvalidOperationException(refusal);
        }

        private IReadOnlyList<PushBackStructureItem> BuildStructure()
        {
            if (!IsTwoSided) return Array.Empty<PushBackStructureItem>();
            var rows = new List<PushBackStructureItem>();
            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                var resolved = _draft.System?.Composite?.Of(side);
                var present = resolved != null && resolved.IsPresent;
                var stored = _draft.Composite.StructureOverride(side);
                var label = side == PushBackSide.A ? "A" : "B";
                var field = new DynamicFieldItem("str" + label, "Lado " + label + ": ajuste manual", string.Empty,
                    stored.HasValue ? stored.Value.ToString(CultureInfo.InvariantCulture) : string.Empty, string.Empty, text =>
                    {
                        if (string.IsNullOrWhiteSpace(text)) return Apply(() => _draft.Composite.RestoreStructure(side), null);
                        if (!UiSupport.TryInt(text, out var positions) || positions < PushBackCellDepth.MinimumPalletsDeep) return "Entero de 2 o más, o vacío para la propuesta.";
                        return Apply(() => _draft.Composite.SetStructureOverride(side, positions), null);
                    }, !CanEdit);
                rows.Add(new PushBackStructureItem(label,
                    present ? resolved.ProposedStructure.ToString(CultureInfo.InvariantCulture) + " posiciones" : "—",
                    present ? resolved.EffectiveStructure.ToString(CultureInfo.InvariantCulture) + " posiciones" : "—",
                    field));
            }

            return rows;
        }

        // ------------------------------------------------------------------ «Extremos y seguridad»

        // The type and the stick-out of the rear tope, by side (the dialog of the classic editor had a section by side).
        private IReadOnlyList<SelectiveChoiceItem> BuildTopeChoices()
        {
            var variants = SelectiveSafetyFamilies.VariantsOfType(_draft.Catalog.SafetyElements, SelectiveSafetyDefaults.TopeType)
                .Select(v => new SelectiveChoiceOption(v.Id, v.Label)).ToList();
            variants.Add(new SelectiveChoiceOption(PushBackRearTopeConfig.NonePieceId, "Ninguno (no se dibuja)"));
            return _draft.Sides().Select(side =>
            {
                var state = _draft.Side(side);
                var config = state.RearTopeConfig();
                var current = config.IsNone ? PushBackRearTopeConfig.NonePieceId : PushBackRearTopeBuilder.ResolvePieceId(_draft.Catalog, config);
                var label = IsTwoSided ? "Tipo de tope · lado " + SideLetter(side) : "Tipo de tope";
                return new SelectiveChoiceItem("topeType" + SideLetter(side), label, variants, current, string.Empty,
                    id => Apply(() => state.RearTopePieceId = id, null), !CanEdit);
            }).ToList();
        }

        private IReadOnlyList<DynamicFieldItem> BuildTopeFields() => _draft.Sides().Select(side =>
        {
            var state = _draft.Side(side);
            var label = IsTwoSided ? "Saque del tope · lado " + SideLetter(side) : "Saque del tope";
            return Number("topeSaque" + SideLetter(side), label, "in", new[] { state.RearTopeSaque }, v => v > 0.0, "El saque debe ser un número mayor que cero.",
                v => state.RearTopeSaque = v);
        }).ToList();

        // «Extremos»: what each family places, counted as the BOM of the draft counts it.
        private IReadOnlyList<string> BuildExtremes()
        {
            var bom = _draft.Computation?.Bom;
            if (bom == null) return new[] { "Todavía no hay un sistema válido que contar." };
            int Count(string type) => bom.Components
                .Where(c => c != null && string.Equals(c.Category, SelectiveBomBuilder.Safety, StringComparison.Ordinal))
                .Where(c => SelectiveSafetyDefaults.IsType(_draft.Catalog.SafetyElements?.FirstOrDefault(e => string.Equals(e?.Id, c.ProfileId, StringComparison.OrdinalIgnoreCase))?.Type, type))
                .Sum(c => c.Quantity);
            var topes = bom.Components.Where(c => c != null && string.Equals(c.Category, PushBackBomBuilder.RearTope, StringComparison.Ordinal)).Sum(c => c.Quantity);
            string Line(string family, int pieces) => family + ": " + (pieces == 0 ? "no lleva" : pieces.ToString(CultureInfo.InvariantCulture) + (pieces == 1 ? " pieza" : " piezas"));
            return new[]
            {
                Line("Botas (base de poste)", Count(SelectiveSafetyDefaults.BotaType)),
                Line("Protectores laterales", Count(SelectiveSafetyDefaults.LateralType)),
                Line("Desviadores", Count(SelectiveSafetyDefaults.DesviadorType)),
                Line("Defensa de montacargas", Count(SelectiveSafetyDefaults.DefensaType)),
                Line("Topes posteriores", topes),
            };
        }

        /// <summary>
        /// What the window does not edit yet of the safety (declared, PB-S03): the boots, the side guards, the deflectors and the forklift
        /// defense keep what the drawing has; they are edited in RACKEDITAR until the table of the fourth batch reaches Push Back.
        /// </summary>
        public string SafetyPendingText => "Botas, protectores laterales, desviadores y defensa de montacargas se conservan como están en el dibujo. "
                                          + "En la ventana todavía no se editan: usa RACKEDITAR › «Configurar seguridad…» (Entrada/Salida · Posterior, por lado).";

        private static string SideLetter(PushBackSide side) => side == PushBackSide.B ? "B" : "A";

        // ------------------------------------------------------------------ field builders

        private void WriteSelected(Action<DynamicEditorCell, PushBackEditorCell> write)
            => _draft.WriteCells(SelectedCells().ToList(), (_, cell, push) => write(cell, push));

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

        private DynamicFieldItem CellNumber(string key, string label, string unit, IReadOnlyList<CellTarget> cells, Func<DynamicEditorCell, double> get,
            Action<DynamicEditorCell, double> set, Func<double, bool> valid, bool allSides = false)
        {
            var values = cells.Select(c => get(c.Cell)).ToList();
            var addresses = cells.Select(c => (c.Front, c.Level)).Distinct().ToList();
            return new DynamicFieldItem(key, label, unit, Same(values), Hint(values.Cast<object>()), text =>
            {
                if (!UiSupport.TryNum(text, out var value) || !valid(value)) return "Revisa los valores de tarima, claro y largueros de cada celda.";
                return Apply(() => _draft.WriteCells(addresses, (_, cell, __) => set(cell, value), allSides), null);
            }, !CanEdit || cells.Count == 0);
        }

        private DynamicFieldItem FrontInt(string key, string label, IReadOnlyList<FrontTarget> fronts, Func<FrontTarget, int> get, int minimum, Action<IReadOnlyList<int>, int> set)
        {
            var values = fronts.Select(get).ToList();
            var same = values.Distinct().Count() <= 1;
            var text0 = same && values.Count > 0 ? values[0].ToString(CultureInfo.InvariantCulture) : string.Empty;
            var indices = fronts.Select(f => f.Front).Distinct().ToList();
            return new DynamicFieldItem(key, label, string.Empty, text0, same ? string.Empty : Varies, text =>
            {
                if (!UiSupport.TryInt(text, out var value) || value < minimum)
                    return "Debe ser un entero mayor o igual que " + minimum.ToString(CultureInfo.InvariantCulture) + ".";
                return Apply(() => set(indices, value), null);
            }, !CanEdit || fronts.Count == 0);
        }

        private DynamicFieldItem FrontNumber(string key, string label, string unit, IReadOnlyList<FrontTarget> fronts, Func<FrontTarget, double> get,
            Func<double, bool> valid, string invalid, Action<IReadOnlyList<int>, double> set)
        {
            var values = fronts.Select(get).ToList();
            var indices = fronts.Select(f => f.Front).Distinct().ToList();
            return new DynamicFieldItem(key, label, unit, Same(values), Hint(values.Cast<object>()), text =>
            {
                if (!UiSupport.TryNum(text, out var value) || !valid(value)) return invalid;
                return Apply(() => set(indices, value), null);
            }, !CanEdit || fronts.Count == 0);
        }

        // A list of the inspector over the cells of the selection: «Varía» when they differ; a choice writes every cell, in every side written.
        private SelectiveChoiceItem Choice(string key, string label, IReadOnlyList<SelectiveChoiceOption> options, IReadOnlyList<CellTarget> cells,
            Func<CellTarget, string> get, bool readOnly, Func<string, Action<CellTarget>> write)
        {
            var values = cells.Select(get).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var shown = values.Count == 1 ? options.FirstOrDefault(o => string.Equals(o.Value, values[0], StringComparison.OrdinalIgnoreCase))?.Value ?? values[0] : null;
            var addresses = cells.Select(c => (c.Front, c.Level)).Distinct().ToList();
            return new SelectiveChoiceItem(key, label, options, shown, values.Count > 1 ? Varies : string.Empty, value =>
            {
                var set = write(value);
                return Apply(() => _draft.WriteCells(addresses, (side, cell, push) => set(new CellTarget(side, 0, 0, cell, push))), null);
            }, readOnly);
        }

        // A peralte of a beam: only one that the catalog offers for the beam of every cell written, as the list of the classic editor.
        private SelectiveChoiceItem PeralteChoice(string key, string label, IReadOnlyList<CellTarget> cells, Func<CellTarget, string> beamOf, double fallback,
            Func<CellTarget, double> get, Action<CellTarget, double> set, bool readOnly)
        {
            IReadOnlyList<double> AllowedFor(CellTarget c)
            {
                var allowed = DynamicRackLevelGeometry.AllowedPeraltes(_draft.Catalog, beamOf(c));
                return allowed.Count > 0 ? allowed : new[] { fallback };
            }

            var common = cells.Count == 0 ? new List<double> { fallback }
                : cells.Select(AllowedFor).Aggregate((IEnumerable<double>)AllowedFor(cells[0]), (acc, next) => acc.Where(a => next.Any(n => Math.Abs(n - a) < 1e-6))).ToList();
            var options = common.Select(p => new SelectiveChoiceOption(Num(p), Num(p) + " in")).ToList();
            var values = cells.Select(c => Num(get(c))).Distinct(StringComparer.Ordinal).ToList();
            var addresses = cells.Select(c => (c.Front, c.Level)).Distinct().ToList();
            return new SelectiveChoiceItem(key, label, options, values.Count == 1 ? values[0] : null, values.Count > 1 ? Varies : string.Empty, value =>
            {
                if (!UiSupport.TryNum(value, out var peralte)) return "Elige uno de la lista.";
                return Apply(() => _draft.WriteCells(addresses, (side, cell, push) => set(new CellTarget(side, 0, 0, cell, push), peralte)), null);
            }, readOnly);
        }

        private static string Same(IReadOnlyList<double> values) =>
            values.Count > 0 && values.All(v => Math.Abs(v - values[0]) < 1e-9) ? Num(values[0]) : string.Empty;

        private static string Hint(IEnumerable<object> values) => values.Distinct().Count() > 1 ? Varies : string.Empty;

        private static string Num(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);
    }
}
