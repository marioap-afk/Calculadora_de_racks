using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.FlowBed;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// One value of the editor of the cama written in a box: its label, its unit, its text, the line under it and its error. The item lives
    /// as long as the editor and is refreshed in place, so the box that has the focus is never replaced. A value the editor refuses stays in
    /// its box, marked, until it is written again or the draft is discarded (dolor 5 of the inventory: every value marked at once).
    /// </summary>
    public sealed class CamaFieldItem : ObservableObject
    {
        private readonly Func<string, string> _commit;
        private string _text = string.Empty;
        private string _error = string.Empty;
        private string _hint = string.Empty;
        private bool _isReadOnly;

        internal CamaFieldItem(string key, string label, string unit, Func<string, string> commit)
        {
            Key = key;
            Label = label;
            Unit = unit ?? string.Empty;
            _commit = commit;
        }

        public string Key { get; }

        public string Label { get; }

        public string Unit { get; }

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value ?? string.Empty);
        }

        public string Error
        {
            get => _error;
            private set
            {
                if (SetProperty(ref _error, value ?? string.Empty)) OnPropertyChanged(nameof(HasError));
            }
        }

        public bool HasError => _error.Length > 0;

        /// <summary>What the field means (and, for an optional one, what empty means).</summary>
        public string Hint
        {
            get => _hint;
            private set => SetProperty(ref _hint, value ?? string.Empty);
        }

        /// <summary>The field cannot be written now: AutoCAD writes the cama, or the value does not apply (the pallet of a Pushback).</summary>
        public bool IsReadOnly
        {
            get => _isReadOnly;
            private set => SetProperty(ref _isReadOnly, value);
        }

        /// <summary>The gesture of the field (Enter or leaving it): the editor takes the text, or marks the field with why not.</summary>
        public void Commit()
        {
            if (_isReadOnly) return;
            _commit?.Invoke(_text);
        }

        internal void Show(string text, string error, string hint, bool readOnly)
        {
            Text = text;
            Error = error;
            Hint = hint;
            IsReadOnly = readOnly;
        }
    }

    /// <summary>One option of a list of the editor of the cama.</summary>
    public sealed record CamaChoiceOption(string Value, string Label);

    /// <summary>One value of the editor of the cama chosen from a list («Tipo de cama», «Rodillo»): a choice goes to the draft at once.</summary>
    public sealed class CamaChoiceItem : ObservableObject
    {
        private readonly Action<string> _commit;
        private IReadOnlyList<CamaChoiceOption> _options = Array.Empty<CamaChoiceOption>();
        private string _selectedValue;
        private string _error = string.Empty;
        private string _hint = string.Empty;
        private bool _isReadOnly;
        private bool _showing;

        internal CamaChoiceItem(string key, string label, Action<string> commit)
        {
            Key = key;
            Label = label;
            _commit = commit;
        }

        public string Key { get; }

        public string Label { get; }

        public IReadOnlyList<CamaChoiceOption> Options
        {
            get => _options;
            private set => SetProperty(ref _options, value ?? Array.Empty<CamaChoiceOption>());
        }

        /// <summary>The value chosen; a choice of the user goes to the draft at once, the value it shows changes nothing.</summary>
        public string SelectedValue
        {
            get => _selectedValue;
            set
            {
                if (_showing || string.IsNullOrEmpty(value) || string.Equals(value, _selectedValue, StringComparison.Ordinal)) return;
                if (_isReadOnly)
                {
                    OnPropertyChanged(nameof(SelectedValue));
                    return;
                }

                _selectedValue = value;
                OnPropertyChanged(nameof(SelectedValue));
                _commit?.Invoke(value);
            }
        }

        public string Error
        {
            get => _error;
            private set
            {
                if (SetProperty(ref _error, value ?? string.Empty)) OnPropertyChanged(nameof(HasError));
            }
        }

        public bool HasError => _error.Length > 0;

        public string Hint
        {
            get => _hint;
            private set => SetProperty(ref _hint, value ?? string.Empty);
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

        internal void Show(IReadOnlyList<CamaChoiceOption> options, string value, string error, string hint, bool readOnly)
        {
            _showing = true;
            try
            {
                if (!options.SequenceEqual(_options)) Options = options;
                if (!string.Equals(_selectedValue, value, StringComparison.Ordinal))
                {
                    _selectedValue = value;
                    OnPropertyChanged(nameof(SelectedValue));
                }
            }
            finally
            {
                _showing = false;
            }

            Error = error;
            Hint = hint;
            IsReadOnly = readOnly;
        }
    }

    /// <summary>One value of «Calculado»: what it is, its value, a line under it, and whether it was written by hand.</summary>
    public sealed record CamaCalcItem(string Label, string Value, string Hint, bool IsManual);

    /// <summary>
    /// The editor of ONE cama de rodamiento in the RackCad window (I-64, quinta tanda, part cm; prototype «camas», every question accepted):
    /// one page with «Lo esencial», «Calculado», «Esquema de la cama» (question 2) and «Avanzado» folded, and the tab «Propiedades» at the
    /// end (VA-07). The rules of the window apply as in Dinámico and Selectivo: one way to confirm (leaving a field or Enter; a list at
    /// once), one «Actualizar» (the one that writes the drawing), «Modificado» measured against what was last read from the drawing or
    /// written to it, «Descartar», «Restaurar layout…» with its two boxes (question 6) and «Cerrar el editor» asking inline when there are
    /// changes. A value that is refused stays marked in its field and «Actualizar» is off with the reason; a cama read with a pitch the
    /// window does not accept opens marked (question 4). A new cama of «Nuevo» is written with «Insertar en el dibujo…»: AutoCAD asks for
    /// the point (question 5: a cama has one view, so there is no «Insertar vista…»). Nothing here writes the drawing.
    /// </summary>
    public sealed class CamaEditorViewModel : ObservableObject
    {
        private const string WaitingWriteText = "AutoCAD está escribiendo la cama: espera a que termine o pulsa «Dejar de esperar».";
        private const string TakenWriteText = "AutoCAD ya tomó la escritura de la cama: espera a que termine, o termina o cancela la orden en AutoCAD; la ventana recibirá su respuesta.";
        private const string NothingManualText = "No hay nada manual que quitar: la cama ya es la que sale del rodillo y de la tarima.";

        private static readonly FlowBedConfigurationStore Store = new();

        private readonly CamaDraftEditor _draft;
        private readonly Action<CamaEditorViewModel> _update;
        private readonly Dictionary<string, (string Text, string Error)> _refused = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _choiceErrors = new(StringComparer.Ordinal);
        private Action<CamaEditorViewModel> _insertNew;
        private Action<CamaEditorViewModel> _saveToLibrary;
        private RackPropertiesTabViewModel _propertiesTab;
        private bool _isNewRack;
        private bool _dirty;
        private bool _writing;
        private bool _writeTaken;
        private bool _confirmingClose;
        private bool _restoreOpen;
        private bool _restorePitch;
        private bool _restoreBrakes;
        private bool _schemaOpen = true;
        private bool _advancedOpen;
        private bool _moreOpen;
        private int _selectedTab;
        private string _baselineJson;
        private FlowBedConfiguration _baselineBed;
        private string _baselineName = string.Empty;
        private string _message = string.Empty;
        private bool _messageIsError;
        private IReadOnlyList<CamaCalcItem> _calculated = Array.Empty<CamaCalcItem>();
        private IReadOnlyList<string> _positionLines = Array.Empty<string>();
        private CamaLayout _layout;

        /// <param name="draft">The draft of the cama, as it was read (its bed is the base of «Modificado» and of «Descartar»).</param>
        /// <param name="rackId">The identity of the cama in the drawing (for a new one, the identity the window gave it).</param>
        /// <param name="update">What «Actualizar» does with this editor (the window writes the drawing); null when it is not offered.</param>
        /// <param name="isNewRack">A new cama of «Nuevo», not in the drawing until «Insertar en el dibujo…» places it.</param>
        public CamaEditorViewModel(CamaDraftEditor draft, string rackId, Action<CamaEditorViewModel> update = null, bool isNewRack = false)
        {
            _draft = draft ?? throw new ArgumentNullException(nameof(draft));
            RackId = rackId;
            _update = update;
            _isNewRack = isNewRack;

            TypeField = new CamaChoiceItem(CamaDraftRules.TypeKey, "Tipo de cama", value => ChooseType(value));
            RollerField = new CamaChoiceItem(CamaDraftRules.RollerKey, "Rodillo", value => ChooseRoller(value));
            LaneField = new CamaFieldItem(CamaDraftRules.LaneKey, "Profundidad de la cama", "in", text => CommitField(CamaDraftRules.LaneKey, text));
            PalletField = new CamaFieldItem(CamaDraftRules.PalletKey, "Profundidad de tarima", "in", text => CommitField(CamaDraftRules.PalletKey, text));
            PitchField = new CamaFieldItem(CamaDraftRules.PitchKey, "Paso de rodillos", "in", text => CommitField(CamaDraftRules.PitchKey, text));
            BrakesField = new CamaFieldItem(CamaDraftRules.BrakesKey, "Freno cada N rodillos", string.Empty, text => CommitField(CamaDraftRules.BrakesKey, text));
            Essentials = new object[] { TypeField, RollerField, LaneField, PalletField };
            AdvancedFields = new[] { PitchField, BrakesField };

            TakeBaseline();
            // Question 4: a cama read with a value the window does not accept opens marked; a problem of «Avanzado» opens it.
            _advancedOpen = AdvancedHasProblem;
            Rebuild();
            if (_draft.RollerOptions.Count == 0) Say("No se encontró el catálogo de rodillos; revisa los CSV del catálogo.", error: true);
        }

        /// <summary>«Cerrar el editor» was confirmed (or the draft had nothing to lose): the window forgets this editor.</summary>
        public event Action<CamaEditorViewModel> CloseRequested;

        // ------------------------------------------------------------------ what the editor shows

        public CamaDraftEditor Draft => _draft;

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

        /// <summary>What an empty name means: the automatic one at insertion (I-60), or the one of the drawing (I-61).</summary>
        public string NameHint => _isNewRack
            ? "Vacío = el nombre automático al insertarla («Cama N», I-60)."
            : "Vacío = conserva el nombre que tiene en el dibujo.";

        /// <summary>A new cama of «Nuevo», not in the drawing yet.</summary>
        public bool IsNewRack => _isNewRack;

        /// <summary>0 = Cama (the one page), 1 = Propiedades (VA-07).</summary>
        public int SelectedTab
        {
            get => _selectedTab;
            set => SetProperty(ref _selectedTab, value);
        }

        public bool IsDirty => _dirty;

        public bool IsWriting => _writing;

        public bool CanEdit => !_writing;

        public bool IsNameReadOnly => !CanEdit;

        /// <summary>«Descartar» also takes away a value refused in its field, with no change in the draft (point 3 of the review).</summary>
        public bool CanDiscard => (_dirty || _refused.Count > 0) && CanEdit;

        /// <summary>The chip of the head, as the other editors of the window (a new cama says «Sin insertar» in its item of «Nuevo»).</summary>
        public string StatusText => _writing ? "Escribiendo" : _dirty ? "Modificado" : "Limpio";

        /// <summary>«1 valor cambiado», «3 valores cambiados» against the base of «Modificado»; empty when nothing changed.</summary>
        public string DiffText
        {
            get
            {
                if (!_dirty || _baselineBed == null) return string.Empty;
                var now = _draft.Snapshot();
                var count = 0;
                if (now.BedType != _baselineBed.BedType) count++;
                if (!string.Equals(now.RollerId, _baselineBed.RollerId, StringComparison.OrdinalIgnoreCase)) count++;
                if (Math.Abs(now.LaneDepth - _baselineBed.LaneDepth) > 1e-9) count++;
                if (Math.Abs(now.PalletDepth - _baselineBed.PalletDepth) > 1e-9) count++;
                if (now.RollerPitchOverride != _baselineBed.RollerPitchOverride) count++;
                if (now.BrakeEveryRollers != _baselineBed.BrakeEveryRollers) count++;
                if (!string.Equals((Name ?? string.Empty).Trim(), _baselineName, StringComparison.Ordinal)) count++;
                return count == 0 ? string.Empty : count == 1 ? "1 valor cambiado" : count.ToString(CultureInfo.InvariantCulture) + " valores cambiados";
            }
        }

        public string Message => _message;

        public bool MessageIsError => _messageIsError;

        /// <summary>The line under the head: what the cama is, and why it has no «Insertar vista…» (question 5).</summary>
        public string SubLine => _isNewRack
            ? "Cama nueva: todavía no está en el dibujo. «Insertar en el dibujo…» pide el punto en AutoCAD y, si no le pones nombre, toma el automático («Cama N», I-60)."
            : "Cama de rodamiento · en el dibujo: vista lateral. Es de una sola vista, así que no tiene «Insertar vista…»: «Actualizar» redibuja su bloque, y las copias del mismo bloque cambian con él.";

        // ------------------------------------------------------------------ the fields

        public CamaChoiceItem TypeField { get; }

        public CamaChoiceItem RollerField { get; }

        public CamaFieldItem LaneField { get; }

        public CamaFieldItem PalletField { get; }

        public CamaFieldItem PitchField { get; }

        public CamaFieldItem BrakesField { get; }

        /// <summary>«Lo esencial»: the type, the roller, the depth of the bed and the depth of the pallet.</summary>
        public IReadOnlyList<object> Essentials { get; }

        /// <summary>«Avanzado»: the pitch of the rollers and a brake every N rollers (question 3).</summary>
        public IReadOnlyList<CamaFieldItem> AdvancedFields { get; }

        /// <summary>How many values are marked now: the refused ones and the problems of what was read (question 4).</summary>
        public int MarkedCount => MarkedKeys().Count;

        public bool HasMarks => MarkedCount > 0;

        /// <summary>
        /// The BOM of the draft for the page «BOM y lista de racks» (a copy), with the rule of «Calculado» and the scheme (CM-S5, point 4 of the
        /// review): while a value is marked, there is none, and the page shows the one of the drawing.
        /// </summary>
        public RackCad.Application.Bom.BillOfMaterials DraftBom() => HasMarks ? null : _draft.Bom();

        // ------------------------------------------------------------------ «Calculado» and «Esquema de la cama»

        public IReadOnlyList<CamaCalcItem> Calculated => _calculated;

        public string CalculatedNote => _layout == null
            ? "Corrige los valores marcados: el cálculo usa todos los campos."
            : _isNewRack
                ? "La lista de materiales (riel, tope, rodillos y frenos) estará en «BOM y lista de racks» cuando la cama esté en el dibujo."
                : "La lista de materiales (riel, tope, rodillos y frenos) está en «BOM y lista de racks» («Ver su BOM»).";

        /// <summary>Where the builder places the pieces (the same instances as the drawing and the BOM); null while a value is marked.</summary>
        public CamaLayout Layout => _layout;

        public bool HasLayout => _layout != null;

        public bool NoLayout => _layout == null;

        public bool IsSchemaOpen
        {
            get => _schemaOpen;
            set => SetProperty(ref _schemaOpen, value);
        }

        /// <summary>«42 rodillos · 1 freno», or «Sin calcular».</summary>
        public string SchemaSummary => _layout == null
            ? "Sin calcular"
            : CamaDraftRules.Count(_layout.Rollers.Count, "rodillo", "rodillos") + " · " + CamaDraftRules.Count(_layout.Brakes.Count, "freno", "frenos");

        /// <summary>What the scheme says to a screen reader: the tope, the rollers and the brakes with where they are, on the rail.</summary>
        public string SchemaText
        {
            get
            {
                if (_layout == null) return "Esquema de la cama: sin calcular.";
                var brakes = _layout.Brakes.Count == 0 ? string.Empty : " (a " + string.Join(", ", _layout.Brakes.Select(CamaDraftRules.Num)) + " in)";
                return "Esquema de la cama: tope, " + CamaDraftRules.Count(_layout.Rollers.Count, "rodillo", "rodillos") + " y "
                       + CamaDraftRules.Count(_layout.Brakes.Count, "freno", "frenos") + brakes + " en " + CamaDraftRules.Num(_layout.RailLength) + " in de riel.";
            }
        }

        /// <summary>The ends of the scheme: the tope (unloading) at 0 and the loading end at the length of the rail.</summary>
        public string SchemaEnds => _layout == null ? string.Empty : "Tope (descarga) · 0 in  —  Carga · " + CamaDraftRules.Num(_layout.RailLength) + " in";

        /// <summary>How far apart the marks of the scale are, as the prototype: 12 in up to 120 in of rail, then 24, 48 and 96.</summary>
        public double SchemaTickStep => _layout == null ? 12.0 : _layout.RailLength <= 120.0 ? 12.0 : _layout.RailLength <= 240.0 ? 24.0 : _layout.RailLength <= 480.0 ? 48.0 : 96.0;

        public string SchemaScaleText => "Marcas de la escala cada " + CamaDraftRules.Num(SchemaTickStep) + " in.";

        public IReadOnlyList<string> PositionLines => _positionLines;

        /// <summary>Where the measures start, and what the scheme is not.</summary>
        public string SchemaNote => _layout == null
            ? string.Empty
            : "Medidas desde el troquel del tope, que está a " + CamaDraftRules.Num(_layout.TopeOrigin) + " in del inicio del riel. Es un esquema, no una vista del dibujo: no lleva cotas.";

        // ------------------------------------------------------------------ «Avanzado»

        public bool IsAdvancedOpen
        {
            get => _advancedOpen;
            set => SetProperty(ref _advancedOpen, value);
        }

        /// <summary>What «Avanzado» says folded too (dolor 7): what is written by hand, and whether something is marked.</summary>
        public string AdvancedSummary
        {
            get
            {
                var pitch = _draft.RollerPitch.HasValue ? "Paso manual: " + CamaDraftRules.Num(_draft.RollerPitch.Value) + " in" : "Paso del rodillo (" + CamaDraftRules.Num(_draft.MinimumPitch) + " in)";
                var brakes = !_draft.IsDynamic ? "Sin frenos" : _draft.BrakeEveryRollers.HasValue ? "Freno cada " + _draft.BrakeEveryRollers.Value.ToString(CultureInfo.InvariantCulture) + " rodillos" : "Frenos por la tarima";
                return pitch + " · " + brakes + (AdvancedHasProblem ? " · con valores marcados" : string.Empty);
            }
        }

        public bool AdvancedHasProblem
        {
            get
            {
                var marked = MarkedKeys();
                return marked.Contains(CamaDraftRules.PitchKey) || marked.Contains(CamaDraftRules.BrakesKey);
            }
        }

        // ------------------------------------------------------------------ the primary action: «Actualizar» or «Insertar en el dibujo…»

        /// <summary>«Actualizar» for a cama of the drawing; «Insertar en el dibujo…» for a new one.</summary>
        public string PrimaryLabel => _isNewRack ? "Insertar en el dibujo…" : "Actualizar";

        public string PrimaryAutomationName => _isNewRack ? "Insertar la cama nueva en el dibujo" : "Actualizar el dibujo con el borrador";

        public bool CanPrimary => string.IsNullOrEmpty(PrimaryBlockedText);

        /// <summary>«Actualizar» can write now (a cama of the drawing with changes and nothing marked).</summary>
        public bool CanUpdate => !_isNewRack && CanPrimary;

        /// <summary>What «Insertar en el dibujo…» of a new cama does (the window inserts it with RACKCAMANUEVO).</summary>
        public Action<CamaEditorViewModel> InsertNewAction
        {
            get => _insertNew;
            set
            {
                _insertNew = value;
                RaiseState();
            }
        }

        /// <summary>Why the primary action cannot run now; empty when it can.</summary>
        public string PrimaryBlockedText
        {
            get
            {
                if (_writing) return WritingText;
                if (_isNewRack && _insertNew == null) return "Aquí no se puede insertar la cama nueva.";
                if (!_isNewRack && _update == null) return "Aquí no se puede escribir el dibujo.";
                var marked = MarkedCount;
                if (marked > 0)
                    return "Corrige " + (marked == 1 ? "el valor marcado" : "los " + marked.ToString(CultureInfo.InvariantCulture) + " valores marcados")
                           + (_isNewRack ? " para insertarla." : " para actualizar.");
                if (!_isNewRack && !_dirty) return "Sin cambios que escribir.";
                if (!_isNewRack && PropertiesBlockUpdate) return PropertiesTab.BlockedText;
                return string.Empty;
            }
        }

        /// <summary>The one gesture that writes the drawing: «Actualizar», or «Insertar en el dibujo…» of a new cama.</summary>
        public void RequestWrite()
        {
            if (!CanPrimary)
            {
                Say(PrimaryBlockedText, error: true);
                return;
            }

            if (_isNewRack) _insertNew(this);
            else _update(this);
        }

        // ------------------------------------------------------------------ «Descartar», «Restaurar layout…», «Cerrar el editor»

        /// <summary>«Descartar»: back to what was last read from the drawing or written to it (a new cama: back to what it opened with).</summary>
        public void Discard()
        {
            if (!CanEdit)
            {
                Say(WritingText, error: true);
                return;
            }

            _draft.LoadFrom(_baselineBed, _baselineName);
            _refused.Clear();
            _choiceErrors.Clear();
            _propertiesTab?.Reset();
            SetRestoreOpen(false);
            SetConfirmingClose(false);
            Say(_isNewRack ? "Borrador descartado." : "Borrador descartado: se muestra lo último que se leyó del dibujo o se escribió en él desde este editor.", error: false);
            OnPropertyChanged(nameof(Name));
            Rebuild();
            UpdateDirty();
        }

        /// <summary>The inline question of «Restaurar layout…» is open (question 6).</summary>
        public bool IsRestoreOpen => _restoreOpen;

        public bool CanRestorePitch => _draft.RollerPitch.HasValue && CanEdit;

        public bool CanRestoreBrakes => _draft.BrakeEveryRollers.HasValue && CanEdit;

        public string RestorePitchLabel => "Paso de rodillos manual" + (_draft.RollerPitch.HasValue ? " (" + CamaDraftRules.Num(_draft.RollerPitch.Value) + " in)" : " (ninguno)");

        public string RestoreBrakesLabel => "Freno cada N rodillos" + (_draft.BrakeEveryRollers.HasValue ? " (" + _draft.BrakeEveryRollers.Value.ToString(CultureInfo.InvariantCulture) + ")" : " (ninguno)");

        public bool RestorePitch
        {
            get => _restorePitch && CanRestorePitch;
            set
            {
                _restorePitch = value;
                OnPropertyChanged(nameof(RestorePitch));
                OnPropertyChanged(nameof(CanConfirmRestore));
            }
        }

        public bool RestoreBrakes
        {
            get => _restoreBrakes && CanRestoreBrakes;
            set
            {
                _restoreBrakes = value;
                OnPropertyChanged(nameof(RestoreBrakes));
                OnPropertyChanged(nameof(CanConfirmRestore));
            }
        }

        public bool CanConfirmRestore => RestorePitch || RestoreBrakes;

        /// <summary>«Restaurar layout…»: opens the question with the two boxes marked; with nothing written by hand it says so and does not open.</summary>
        public void OpenRestore()
        {
            if (!CanEdit)
            {
                Say(WritingText, error: true);
                return;
            }

            SetConfirmingClose(false);
            if (!_draft.RollerPitch.HasValue && !_draft.BrakeEveryRollers.HasValue)
            {
                SetRestoreOpen(false);
                Say(NothingManualText, error: false);
                return;
            }

            _restorePitch = true;
            _restoreBrakes = true;
            SetRestoreOpen(true);
            Say(string.Empty, error: false);
        }

        /// <summary>«Restaurar lo marcado»: the marked values go back to the roller and the pallet, in the draft.</summary>
        public void ConfirmRestore()
        {
            if (!_restoreOpen || !CanEdit || !CanConfirmRestore) return;
            var pitch = RestorePitch;
            var brakes = RestoreBrakes;
            var gone = _draft.Restore(pitch, brakes);
            if (pitch) _refused.Remove(CamaDraftRules.PitchKey);
            if (brakes) _refused.Remove(CamaDraftRules.BrakesKey);
            SetRestoreOpen(false);
            Say(gone.Count == 0
                ? NothingManualText
                : "Layout restaurado en el borrador: se quitó " + string.Join(" y ", gone) + ". El dibujo no cambia hasta «" + (_isNewRack ? "Insertar en el dibujo…" : "Actualizar") + "».", error: false);
            Rebuild();
            UpdateDirty();
        }

        /// <summary>«Cancelar» of «Restaurar layout…»: nothing changes.</summary>
        public void CancelRestore() => SetRestoreOpen(false);

        /// <summary>«Cerrar el editor» of a draft with changes asks first, inline.</summary>
        public bool IsConfirmingClose => _confirmingClose;

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
                SetRestoreOpen(false);
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

        public void CancelClose() => SetConfirmingClose(false);

        // ------------------------------------------------------------------ what the window tells it

        /// <summary>The window handed this draft to AutoCAD: nothing can be edited until it answers or the window stops waiting.</summary>
        public void BeginWriting()
        {
            if (_writing) return;
            _writing = true;
            _writeTaken = false;
            SetConfirmingClose(false);
            SetRestoreOpen(false);
            _propertiesTab?.Lock(true);
            ForgetRefusalsThatDoNotApply(); // point 6 of the review: what is written is the draft, with no hidden mark left behind
            Say(_isNewRack ? "Insertando la cama en el dibujo: AutoCAD pide el punto de inserción…" : "Escribiendo la cama en el dibujo…", error: false);
            Rebuild();
        }

        // The pallet and the brakes do not apply to a Pushback: a value refused there is forgotten when the cama becomes Pushback or is written
        // (point 6 of the review: it came back, marked, after a write in Pushback, when the cama became Dinámica again).
        private void ForgetRefusalsThatDoNotApply()
        {
            if (_draft.IsDynamic) return;
            _refused.Remove(CamaDraftRules.PalletKey);
            _refused.Remove(CamaDraftRules.BrakesKey);
        }

        /// <summary>What the editor says while AutoCAD writes it: «Dejar de esperar» only until the command takes the write (M-11).</summary>
        public string WritingText => _writeTaken ? TakenWriteText : WaitingWriteText;

        /// <summary>The command of AutoCAD took the write: its answer will come, and «Dejar de esperar» is no longer offered.</summary>
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

        /// <summary>The window wrote what was sent: that bed and that name are the drawing now, and «Descartar» goes back to them.</summary>
        public void MarkWritten(string message, string writtenJson, string writtenName)
        {
            var written = Store.Deserialize(writtenJson);
            if (written != null)
            {
                _baselineBed = written;
                _baselineJson = Store.Serialize(written);
            }

            _baselineName = (writtenName ?? string.Empty).Trim();
            Say(message, error: false);
            UpdateDirty();
            RaiseState();
        }

        /// <summary>AutoCAD placed the new cama: from now on it is a cama of the drawing, with «Actualizar»; it takes the name the drawing gave it.</summary>
        public void MarkInserted(string name = null)
        {
            if (!_isNewRack) return;
            _isNewRack = false;
            if (!string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(_draft.Name))
            {
                _draft.Name = name.Trim();
                _baselineName = _draft.Name;
                OnPropertyChanged(nameof(Name));
            }

            OnPropertyChanged(nameof(IsNewRack));
            OnPropertyChanged(nameof(NameHint));
            OnPropertyChanged(nameof(SubLine));
            OnPropertyChanged(nameof(PrimaryLabel));
            OnPropertyChanged(nameof(PrimaryAutomationName));
            UpdateDirty();
            Rebuild();
        }

        /// <summary>What the window or the view needs to say.</summary>
        public void Say(string text, bool error)
        {
            _message = text ?? string.Empty;
            _messageIsError = error && _message.Length > 0;
            OnPropertyChanged(nameof(Message));
            OnPropertyChanged(nameof(MessageIsError));
        }

        // ------------------------------------------------------------------ «Duplicar» of a draft (T4 new)

        /// <summary>The base of «Modificado» and of «Descartar» (a copy), for the copy of «Duplicar».</summary>
        public FlowBedConfiguration BaselineBed => CamaDraftRules.Copy(_baselineBed);

        /// <summary>«Duplicar»: the copy opens over the base of its original and takes the copied bed as an edit, so it is «Modificado» when its original was.</summary>
        public bool TakeDesignAsEdit(FlowBedConfiguration bed)
        {
            if (bed == null || !CanEdit) return false;
            _draft.LoadFrom(bed, _draft.Name);
            Rebuild();
            UpdateDirty();
            return true;
        }

        // ------------------------------------------------------------------ the tab «Propiedades» (VA-07)

        /// <summary>The index of the tab «Propiedades», the last one of the editor.</summary>
        public const int PropertiesTabIndex = 1;

        public RackPropertiesTabViewModel PropertiesTab
        {
            get
            {
                if (_propertiesTab != null) return _propertiesTab;
                _propertiesTab = new RackPropertiesTabViewModel();
                _propertiesTab.Changed += OnPropertiesTabChanged;
                return _propertiesTab;
            }
        }

        private bool PropertiesDirty => _propertiesTab != null && _propertiesTab.IsDirty;

        private bool PropertiesBlockUpdate => _propertiesTab != null && _propertiesTab.HasProblems;

        /// <summary>The properties read with the cama by the door (RACKCAMAABRIR).</summary>
        public void LoadProperties(RackPropertiesRead read) => PropertiesTab.Load(read);

        /// <summary>What «Actualizar» writes in the properties of the cama, or null when the tab writes nothing.</summary>
        public RackPropertiesWrite PropertiesWrite() => _propertiesTab?.BuildWrite();

        /// <summary>The answer of a write: when the properties were written, the tab goes on from what the drawing holds now.</summary>
        public void PropertiesWritten(RackDesignWritten written)
        {
            if (written != null && written.Written && written.PropertiesError == null) _propertiesTab?.Written(written.Properties);
        }

        private void OnPropertiesTabChanged()
        {
            UpdateDirty();
            RaiseState();
        }

        // ------------------------------------------------------------------ «Más» › «Guardar en biblioteca…» (UT-04)

        public bool IsMoreOpen
        {
            get => _moreOpen;
            set
            {
                if (!SetProperty(ref _moreOpen, value)) return;
                RaiseMore();
            }
        }

        public string MoreItemStatus => _moreOpen ? "abierto" : "cerrado";

        public void ToggleMore() => IsMoreOpen = !IsMoreOpen;

        /// <summary>What «Guardar en biblioteca…» does with this editor (the window shows the page of the library); null when it is not offered.</summary>
        public Action<CamaEditorViewModel> SaveToLibraryAction
        {
            get => _saveToLibrary;
            set
            {
                _saveToLibrary = value;
                RaiseMore();
            }
        }

        public bool CanSaveToLibrary => string.IsNullOrEmpty(SaveToLibraryBlockedText);

        /// <summary>Why «Guardar en biblioteca…» cannot run now; empty when it can.</summary>
        public string SaveToLibraryBlockedText => _saveToLibrary == null ? "Aquí no se puede guardar en la biblioteca." : string.Empty;

        /// <summary>«Guardar en biblioteca…»: a value marked stops it and is named; otherwise the window shows the line of the save.</summary>
        public void SaveToLibrary()
        {
            var refusal = SaveRefusal();
            if (refusal != null)
            {
                Say(refusal, error: true);
                return;
            }

            IsMoreOpen = false;
            _saveToLibrary(this);
        }

        /// <summary>
        /// Why the draft cannot be saved now, or null: a value written and refused, a value read that the window does not accept, then
        /// <see cref="SaveToLibraryBlockedText"/>. «Más» asks it, and the line of the library asks it again at «Guardar» and «Reemplazar».
        /// </summary>
        public string SaveRefusal()
        {
            var marked = MarkedKeys(); // the pallet and the brakes of a Pushback do not apply: a value refused there does not stop it
            foreach (var field in AllFields())
            {
                if (marked.Contains(field.Key) && _refused.TryGetValue(field.Key, out var refused))
                    return "No se puede guardar en la biblioteca: «" + field.Label + "» tiene un valor que no se aplicó (" + refused.Error.Trim().TrimEnd('.')
                           + "). Corrígelo o vuelve a escribir el que tenía.";
            }

            var problems = _draft.Problems();
            if (problems.Count > 0)
                return "No se puede guardar en la biblioteca: " + string.Join(" ", problems.Values.Distinct()) + " Corrígelo antes de guardar.";
            var blocked = SaveToLibraryBlockedText;
            return string.IsNullOrEmpty(blocked) ? null : blocked;
        }

        // ------------------------------------------------------------------ the gestures of the fields

        // A text of a field: the same one it shows clears its mark and changes nothing; otherwise the draft takes it or the field keeps it,
        // marked with why not. Every change recomputes the scheme and «Calculado».
        private string CommitField(string key, string text)
        {
            if (!CanEdit)
            {
                Say(WritingText, error: true);
                Rebuild();
                return WritingText;
            }

            var typed = (text ?? string.Empty).Trim();
            if (string.Equals(typed, DraftText(key), StringComparison.Ordinal))
            {
                if (_refused.Remove(key)) Rebuild();
                return null;
            }

            var error = Apply(key, typed);
            if (error != null)
            {
                _refused[key] = (text ?? string.Empty, error);
                Rebuild();
                return error;
            }

            _refused.Remove(key);
            _choiceErrors.Clear();
            Say(string.Empty, error: false);
            Rebuild();
            UpdateDirty();
            return null;
        }

        private string Apply(string key, string typed)
        {
            switch (key)
            {
                case CamaDraftRules.LaneKey:
                    return UiSupport.TryNum(typed, out var lane) ? _draft.SetLaneDepth(lane) : CamaDraftRules.LaneDepthText;
                case CamaDraftRules.PalletKey:
                    return UiSupport.TryNum(typed, out var pallet) ? _draft.SetPalletDepth(pallet) : CamaDraftRules.PalletDepthText;
                case CamaDraftRules.PitchKey:
                    if (typed.Length == 0) return _draft.SetPitch(null);
                    return UiSupport.TryNum(typed, out var pitch) ? _draft.SetPitch(pitch) : CamaDraftRules.PitchPositiveText;
                case CamaDraftRules.BrakesKey:
                    if (typed.Length == 0) return _draft.SetBrakeEvery(null);
                    return UiSupport.TryInt(typed, out var every) ? _draft.SetBrakeEvery(every) : CamaDraftRules.BrakesText;
                default:
                    return null;
            }
        }

        private void ChooseType(string value)
        {
            if (!CanEdit)
            {
                Say(WritingText, error: true);
                Rebuild();
                return;
            }

            var type = string.Equals(value, nameof(FlowBedType.Pushback), StringComparison.Ordinal) ? FlowBedType.Pushback : FlowBedType.Dynamic;
            _draft.SetBedType(type, out var note);
            // Question 8: the pallet that was empty is now a value of the draft; a mark of the pallet goes with it.
            if (note != null) _refused.Remove(CamaDraftRules.PalletKey);
            ForgetRefusalsThatDoNotApply(); // point 6 of the review
            _choiceErrors.Clear();
            Say(note ?? string.Empty, error: false);
            Rebuild();
            UpdateDirty();
        }

        private void ChooseRoller(string value)
        {
            if (!CanEdit)
            {
                Say(WritingText, error: true);
                Rebuild();
                return;
            }

            var error = _draft.SetRoller(value);
            _choiceErrors.Clear();
            if (error != null) _choiceErrors[CamaDraftRules.RollerKey] = error;
            else Say(string.Empty, error: false);
            Rebuild();
            UpdateDirty();
        }

        // ------------------------------------------------------------------ internals

        private IEnumerable<CamaFieldItem> AllFields()
        {
            yield return LaneField;
            yield return PalletField;
            yield return PitchField;
            yield return BrakesField;
        }

        private string DraftText(string key)
        {
            switch (key)
            {
                case CamaDraftRules.LaneKey: return CamaDraftRules.Num(_draft.LaneDepth);
                case CamaDraftRules.PalletKey: return _draft.PalletDepth > 0.0 ? CamaDraftRules.Num(_draft.PalletDepth) : string.Empty;
                case CamaDraftRules.PitchKey: return _draft.RollerPitch.HasValue ? CamaDraftRules.Num(_draft.RollerPitch.Value) : string.Empty;
                case CamaDraftRules.BrakesKey: return _draft.BrakeEveryRollers.HasValue ? _draft.BrakeEveryRollers.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
                default: return string.Empty;
            }
        }

        // The keys marked now: a value refused in its field, or what the draft has that the window does not accept. The pallet and the
        // brakes of a Pushback do not apply, so they are not marked.
        private HashSet<string> MarkedKeys()
        {
            var keys = new HashSet<string>(_draft.Problems().Keys, StringComparer.Ordinal);
            foreach (var key in _refused.Keys)
            {
                if (!_draft.IsDynamic && (key == CamaDraftRules.PalletKey || key == CamaDraftRules.BrakesKey)) continue;
                keys.Add(key);
            }

            return keys;
        }

        private string ErrorOf(string key)
        {
            if (!_draft.IsDynamic && (key == CamaDraftRules.PalletKey || key == CamaDraftRules.BrakesKey)) return string.Empty;
            if (_refused.TryGetValue(key, out var refused)) return refused.Error;
            return _draft.Problems().TryGetValue(key, out var problem) ? problem : string.Empty;
        }

        private string TextOf(string key)
        {
            if (_draft.IsDynamic || (key != CamaDraftRules.PalletKey && key != CamaDraftRules.BrakesKey))
                if (_refused.TryGetValue(key, out var refused)) return refused.Text;
            return DraftText(key);
        }

        private void TakeBaseline()
        {
            _baselineBed = _draft.Snapshot();
            _baselineJson = _draft.Json;
            _baselineName = (_draft.Name ?? string.Empty).Trim();
            _dirty = false;
            RaiseState();
        }

        // «Modificado» is a difference with what the drawing has (as last read or written), never a gesture.
        private void UpdateDirty()
        {
            var dirty = !string.Equals(_draft.Json, _baselineJson, StringComparison.Ordinal)
                        || !string.Equals((_draft.Name ?? string.Empty).Trim(), _baselineName, StringComparison.Ordinal)
                        || PropertiesDirty;
            if (dirty != _dirty)
            {
                _dirty = dirty;
                if (!dirty) SetConfirmingClose(false);
            }

            RaiseState();
        }

        private void SetConfirmingClose(bool value)
        {
            if (_confirmingClose == value) return;
            _confirmingClose = value;
            OnPropertyChanged(nameof(IsConfirmingClose));
        }

        private void SetRestoreOpen(bool value)
        {
            if (_restoreOpen != value)
            {
                _restoreOpen = value;
                OnPropertyChanged(nameof(IsRestoreOpen));
            }

            RaiseRestore();
        }

        // Every change of the draft or of the state: the fields in place, «Calculado», the scheme and «Avanzado».
        private void Rebuild()
        {
            var locked = !CanEdit;
            var dynamic = _draft.IsDynamic;
            var roller = _draft.Roller;
            var minimum = _draft.MinimumPitch;

            TypeField.Show(new[] { new CamaChoiceOption(nameof(FlowBedType.Dynamic), "Dinámica"), new CamaChoiceOption(nameof(FlowBedType.Pushback), "Pushback") },
                dynamic ? nameof(FlowBedType.Dynamic) : nameof(FlowBedType.Pushback), string.Empty,
                dynamic ? "Lleva frenos según la tarima." : "Sin frenos: la tarima no aplica.", locked);

            var options = _draft.RollerOptions.Select(r => new CamaChoiceOption(r.Id, r.Label)).ToList();
            if (roller == null && !string.IsNullOrWhiteSpace(_draft.RollerId)) options.Add(new CamaChoiceOption(_draft.RollerId, _draft.RollerId + " (no está en el catálogo)"));
            var rollerError = _choiceErrors.TryGetValue(CamaDraftRules.RollerKey, out var chosen) ? chosen : ErrorOf(CamaDraftRules.RollerKey);
            RollerField.Show(options, roller?.Id ?? _draft.RollerId, rollerError,
                roller == null ? string.Empty : "Capacidad del catálogo: " + CamaDraftRules.Num(roller.CapacityKg) + " kg. Paso automático: " + CamaDraftRules.Num(roller.AutomaticPitch) + " in.",
                locked);

            LaneField.Show(TextOf(CamaDraftRules.LaneKey), ErrorOf(CamaDraftRules.LaneKey), "Largo del riel («Fondo de cama» en el editor clásico).", locked);
            PalletField.Show(TextOf(CamaDraftRules.PalletKey), ErrorOf(CamaDraftRules.PalletKey),
                dynamic ? "Separa los frenos: tarima + 1 in o más («Fondo de tarima» en el editor clásico)." : "No aplica en Pushback: no lleva frenos. Conserva su valor.",
                locked || !dynamic);
            PitchField.Show(TextOf(CamaDraftRules.PitchKey), ErrorOf(CamaDraftRules.PitchKey),
                "Vacío = automático, según el rodillo (" + CamaDraftRules.Num(minimum) + " in con este). Un entero de pulgadas, no menor que el rodillo.", locked);
            BrakesField.Show(TextOf(CamaDraftRules.BrakesKey), ErrorOf(CamaDraftRules.BrakesKey),
                dynamic ? "Vacío = según la profundidad de tarima, como siempre." : "No aplica en Pushback. Conserva su valor.", locked || !dynamic);

            _layout = MarkedCount > 0 ? null : _draft.Layout();
            _calculated = BuildCalculated(roller);
            _positionLines = CamaDraftRules.PositionLines(_layout);
            OnPropertyChanged(nameof(Layout));
            OnPropertyChanged(nameof(HasLayout));
            OnPropertyChanged(nameof(NoLayout));
            OnPropertyChanged(nameof(Calculated));
            OnPropertyChanged(nameof(CalculatedNote));
            OnPropertyChanged(nameof(PositionLines));
            OnPropertyChanged(nameof(SchemaSummary));
            OnPropertyChanged(nameof(SchemaText));
            OnPropertyChanged(nameof(SchemaEnds));
            OnPropertyChanged(nameof(SchemaNote));
            OnPropertyChanged(nameof(SchemaTickStep));
            OnPropertyChanged(nameof(SchemaScaleText));
            OnPropertyChanged(nameof(AdvancedSummary));
            OnPropertyChanged(nameof(AdvancedHasProblem));
            OnPropertyChanged(nameof(MarkedCount));
            OnPropertyChanged(nameof(HasMarks));
            RaiseRestore();
            RaiseState();
        }

        private IReadOnlyList<CamaCalcItem> BuildCalculated(CamaRollerOption roller)
        {
            if (_layout == null)
            {
                return new[]
                {
                    new CamaCalcItem("Rodillos", "—", "Sin calcular.", false),
                    new CamaCalcItem("Frenos", "—", "Sin calcular.", false),
                };
            }

            string brakesHint;
            if (!_draft.IsDynamic) brakesHint = "Pushback: sin frenos.";
            else if (_layout.Brakes.Count == 0) brakesHint = "El riel no llega al primer freno.";
            else if (_draft.BrakeEveryRollers.HasValue) brakesHint = "Uno cada " + _draft.BrakeEveryRollers.Value.ToString(CultureInfo.InvariantCulture) + " rodillos.";
            else brakesHint = "Por la tarima: cada " + CamaDraftRules.Num(_draft.PalletDepth + FlowBedDefaults.BrakeClearanceOverPallet) + " in o más.";

            var rollerHint = roller == null
                ? "Del rodillo elegido."
                : "De " + CamaDraftRules.Num(roller.Diameter) + " in" + (string.IsNullOrWhiteSpace(roller.Gauge) ? "." : ", calibre " + roller.Gauge.Trim() + ".");
            return new[]
            {
                new CamaCalcItem("Riel", CamaDraftRules.Num(_layout.RailLength) + " in", "Largo = profundidad de la cama.", false),
                new CamaCalcItem("Rodillos", _layout.Rollers.Count.ToString(CultureInfo.InvariantCulture), rollerHint, false),
                new CamaCalcItem("Frenos", _layout.Brakes.Count.ToString(CultureInfo.InvariantCulture), brakesHint, false),
                new CamaCalcItem("Paso", CamaDraftRules.Num(_layout.Pitch) + " in", _layout.ManualPitch ? "Manual (en «Avanzado»)." : "El del rodillo.", _layout.ManualPitch),
                new CamaCalcItem("Tope", "1", "Ocupa " + CamaDraftRules.Num(_layout.TopeLength) + " in desde el primer troquel.", false),
            };
        }

        private void RaiseRestore()
        {
            OnPropertyChanged(nameof(CanRestorePitch));
            OnPropertyChanged(nameof(CanRestoreBrakes));
            OnPropertyChanged(nameof(RestorePitchLabel));
            OnPropertyChanged(nameof(RestoreBrakesLabel));
            OnPropertyChanged(nameof(RestorePitch));
            OnPropertyChanged(nameof(RestoreBrakes));
            OnPropertyChanged(nameof(CanConfirmRestore));
        }

        private void RaiseState()
        {
            OnPropertyChanged(nameof(IsDirty));
            OnPropertyChanged(nameof(IsWriting));
            OnPropertyChanged(nameof(CanEdit));
            OnPropertyChanged(nameof(IsNameReadOnly));
            OnPropertyChanged(nameof(CanDiscard));
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(DiffText));
            OnPropertyChanged(nameof(CanPrimary));
            OnPropertyChanged(nameof(CanUpdate));
            OnPropertyChanged(nameof(PrimaryBlockedText));
            OnPropertyChanged(nameof(PrimaryLabel));
            OnPropertyChanged(nameof(WritingText));
        }

        private void RaiseMore()
        {
            OnPropertyChanged(nameof(IsMoreOpen));
            OnPropertyChanged(nameof(MoreItemStatus));
            OnPropertyChanged(nameof(CanSaveToLibrary));
            OnPropertyChanged(nameof(SaveToLibraryBlockedText));
        }
    }
}
