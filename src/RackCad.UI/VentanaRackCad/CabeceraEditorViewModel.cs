using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Media;
using RackCad.Application.Catalogs;
using RackCad.Application.RackFrames;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.RackFrames;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>One row of «Alzado»: a panel (P) or a horizontal (H) of the frame, from the top down, the selector of the editor (CB-04).</summary>
    public sealed class CabeceraElevationRow : ObservableObject
    {
        private string _value = string.Empty;
        private string _text = string.Empty;
        private bool _isSelected;
        private bool _isPrimary;
        private bool _differs;
        private bool _isDouble;
        private bool _hasBracing;
        private Geometry _bracing = Geometry.Empty;
        private string _noBracingText = string.Empty;
        private double _rowHeight = 28.0;
        private string _accessibleText = string.Empty;

        public CabeceraElevationRow(bool isPanel, int index)
        {
            IsPanel = isPanel;
            Index = index;
            Label = (isPanel ? "P" : "H") + (index + 1).ToString(CultureInfo.InvariantCulture);
        }

        public bool IsPanel { get; }

        public bool IsHorizontal => !IsPanel;

        public int Index { get; }

        public string Label { get; }

        /// <summary>«44 in» (the clear of a panel) or «48 in» (the elevation of a horizontal).</summary>
        public string Value { get => _value; private set => SetProperty(ref _value, value); }

        public string Text { get => _text; private set => SetProperty(ref _text, value); }

        public bool IsSelected { get => _isSelected; private set => SetProperty(ref _isSelected, value); }

        public bool IsPrimary { get => _isPrimary; private set => SetProperty(ref _isPrimary, value); }

        /// <summary>«Distinta del estándar» (the legend of the prototype).</summary>
        public bool Differs { get => _differs; private set => SetProperty(ref _differs, value); }

        /// <summary>A horizontal with quantity 2 or more (another travesaño one troquel higher).</summary>
        public bool IsDouble { get => _isDouble; private set => SetProperty(ref _isDouble, value); }

        public bool HasBracing { get => _hasBracing; private set => SetProperty(ref _hasBracing, value); }

        /// <summary>The diagonals of the panel in a 100 × 100 box (stretched to the row).</summary>
        public Geometry Bracing { get => _bracing; private set => SetProperty(ref _bracing, value); }

        public string NoBracingText { get => _noBracingText; private set => SetProperty(ref _noBracingText, value); }

        /// <summary>The height of the row, at scale with the clear of the panel (a horizontal is a fixed bar).</summary>
        public double RowHeight { get => _rowHeight; private set => SetProperty(ref _rowHeight, value); }

        /// <summary>Unique in the elevation: «P2, panel de 44 in, diagonal simple, elegido».</summary>
        public string AccessibleText { get => _accessibleText; private set => SetProperty(ref _accessibleText, value); }

        internal void Set(string value, string text, bool selected, bool primary, bool differs, bool isDouble, Geometry bracing, string noBracing, double height, string accessible)
        {
            Value = value;
            Text = text;
            IsSelected = selected;
            IsPrimary = primary;
            Differs = differs;
            IsDouble = isDouble;
            HasBracing = bracing != null && !bracing.IsEmpty();
            Bracing = bracing ?? Geometry.Empty;
            NoBracingText = noBracing ?? string.Empty;
            RowHeight = height;
            AccessibleText = accessible;
        }
    }

    /// <summary>A box of the editor of cabeceras (e.g. «Poste reforzado»), «varía» when the two posts differ; a click writes at once.</summary>
    public sealed class CabeceraCheckItem : ObservableObject
    {
        private Func<bool, string> _commit;
        private bool? _isChecked;
        private string _label;
        private bool _isReadOnly;
        private string _error = string.Empty;

        public CabeceraCheckItem(string key, string label, bool? isChecked, Func<bool, string> commit, bool isReadOnly)
        {
            Key = key;
            _label = label;
            _isChecked = isChecked;
            _commit = commit;
            _isReadOnly = isReadOnly;
        }

        public string Key { get; }

        public string Label { get => _label; private set => SetProperty(ref _label, value); }

        public bool IsEnabled => !_isReadOnly;

        public string Error { get => _error; private set => SetProperty(ref _error, value ?? string.Empty); }

        /// <summary>null = «varía»: a click on it puts the value in both posts (the rule of S-20 / D-11, never the false WPF sends).</summary>
        public bool? IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isReadOnly || _commit == null) return;
                var next = _isChecked == null ? true : value == true;
                if (_isChecked == next) return;
                Error = _commit(next) ?? string.Empty;

                // A refused write leaves the box as the draft is (a written one was refreshed already by the recomposition).
                if (Error.Length > 0) OnPropertyChanged(nameof(IsChecked));
            }
        }

        internal void Refresh(CabeceraCheckItem next)
        {
            _commit = next._commit;
            Label = next._label;
            var readOnlyChanged = _isReadOnly != next._isReadOnly;
            _isReadOnly = next._isReadOnly;
            if (readOnlyChanged) OnPropertyChanged(nameof(IsEnabled));
            _isChecked = next._isChecked;
            OnPropertyChanged(nameof(IsChecked));
        }
    }

    /// <summary>A column of «Postes y placas»: «Los dos postes», or «Poste izquierdo» / «Poste derecho».</summary>
    public sealed class CabeceraSideColumn : ObservableObject
    {
        public CabeceraSideColumn(string title, IReadOnlyList<object> fields)
        {
            Title = title;
            Fields = fields;
        }

        public string Title { get; }

        public IReadOnlyList<object> Fields { get; private set; }

        internal void Refresh(CabeceraSideColumn next)
        {
            if (CabeceraEditorViewModel.RefreshInPlace(Fields, next.Fields)) return;
            Fields = next.Fields;
            OnPropertyChanged(nameof(Fields));
        }
    }

    /// <summary>A box of «Restaurar estándar…» (the categories of decision 8 of Selectivo, CB-05) with its count of changes.</summary>
    public sealed class CabeceraRestoreBox : ObservableObject
    {
        private readonly Action _changed;
        private bool _isChecked = true;
        private int _count;

        internal CabeceraRestoreBox(CabeceraChangeCategory category, Action changed)
        {
            Category = category;
            _changed = changed;
        }

        public CabeceraChangeCategory Category { get; }

        public int Count => _count;

        public string Label => CabeceraDraftEditor.CategoryLabel(Category) + (_count == 0 ? " (ninguno)" : " (" + _count.ToString(CultureInfo.InvariantCulture) + ")");

        public bool IsEnabled => _count > 0;

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (!SetProperty(ref _isChecked, value)) return;
                _changed?.Invoke();
            }
        }

        internal void SetCount(int count)
        {
            if (_count == count) return;
            _count = count;
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(nameof(Label));
            OnPropertyChanged(nameof(IsEnabled));
        }
    }

    /// <summary>A destination of «Copiar a otros postes…» (or «…módulos»), CB-10: one box per post and fondo.</summary>
    public sealed record CabeceraCopyTarget(string Key, string Label);

    public sealed class CabeceraCopyTargetItem : ObservableObject
    {
        private readonly Action _changed;
        private bool _isChecked;

        internal CabeceraCopyTargetItem(CabeceraCopyTarget target, Action changed)
        {
            Target = target;
            _changed = changed;
        }

        public CabeceraCopyTarget Target { get; }

        public string Label => Target.Label;

        public string AccessibleText => "Copiar a " + Target.Label;

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (!SetProperty(ref _isChecked, value)) return;
                _changed?.Invoke();
            }
        }
    }

    /// <summary>What the rack that holds an embedded cabecera answered to «Aplicar al borrador» or to a copy.</summary>
    public sealed record CabeceraHostAnswer(bool Applied, bool NeedsConfirmation, string Message)
    {
        public static CabeceraHostAnswer Done(string message) => new CabeceraHostAnswer(true, false, message);

        public static CabeceraHostAnswer Refuse(string message) => new CabeceraHostAnswer(false, false, message);

        public static CabeceraHostAnswer Confirm(string message) => new CabeceraHostAnswer(false, true, message);
    }

    /// <summary>
    /// The rack that holds an embedded cabecera (CB-01: the cabecera of a post or of a module, inside the editor of its rack): its name, the
    /// title of the cabecera, the destinations of a copy, and the writes in ITS draft. Nothing here writes AutoCAD.
    /// </summary>
    public interface ICabeceraEditorHost
    {
        string HostName { get; }

        string Title { get; }

        string CopyLabel { get; }

        IReadOnlyList<CabeceraCopyTarget> CopyTargets { get; }

        /// <summary>The cabecera into the draft of the rack. <paramref name="force"/> = «Aplicar igual» after a severe height notice (CB-09).</summary>
        CabeceraHostAnswer Apply(RackFrameConfiguration cabecera, bool force);

        /// <summary>«Aplicar aquí y copiar a n destinos»: the cabecera here and in each target, each with its depth and its peralte; all or nothing.</summary>
        CabeceraHostAnswer Copy(RackFrameConfiguration cabecera, IReadOnlyList<CabeceraCopyTarget> targets, bool force);

        /// <summary>«‹ Rack»: the editor of the rack again; the cabecera editor is forgotten.</summary>
        void Back();
    }

    /// <summary>
    /// The editor of cabeceras of the RackCad window (I-64, fourth batch, part cb; prototype «cabecera», CB-01 to CB-13): the tabs Cabecera,
    /// Alzado (the first one), Postes y placas and Avanzado over a <see cref="CabeceraDraftEditor"/>, with the same fields and the same one
    /// way to confirm as Selectivo and Dinámico (a field goes to the draft when it is left or with Enter; a list, when a value is chosen).
    /// <para>
    /// Two entries (CB-01): a cabecera of its own (a rack «Cabecera N» of the list, «Editar aquí»), whose head has «Actualizar», «Descartar»,
    /// «Restaurar estándar…» and «Cerrar el editor» and writes through the door of the window; and the cabecera of a post of a Selectivo
    /// (<see cref="ICabeceraEditorHost"/>), whose head has «‹ Rack», «Aplicar al borrador», «Copiar a otros postes…», «Descartar cambios» and
    /// «Restaurar estándar…», and that only writes the draft of that rack. No modal window: every question is inline.
    /// </para>
    /// </summary>
    public sealed class CabeceraEditorViewModel : ObservableObject
    {
        public const string DefaultNewPanelClear = "44";
        private const string Varies = "Varía";
        private const string WaitingWriteText = "AutoCAD está escribiendo la cabecera: espera a que termine o pulsa «Dejar de esperar».";
        private const string TakenWriteText = "AutoCAD ya tomó la escritura de la cabecera: espera a que termine, o termina o cancela la orden en AutoCAD; la ventana recibirá su respuesta.";

        private static readonly Geometry SingleRight = Frozen("M0,100 L100,0");
        private static readonly Geometry SingleLeft = Frozen("M100,100 L0,0");
        private static readonly Geometry DoubleRight = Frozen("M0,100 L100,20 M0,80 L100,0");
        private static readonly Geometry DoubleLeft = Frozen("M100,100 L0,20 M100,80 L0,0");
        private static readonly Geometry Cross = Frozen("M0,100 L100,0 M0,0 L100,100");
        private static readonly Geometry KBrace = Frozen("M0,100 L100,50 M0,0 L100,50");

        private readonly CabeceraDraftEditor _editor;
        private readonly ICabeceraEditorHost _host;
        private readonly Action<CabeceraEditorViewModel> _update;
        private readonly bool _wasCustom;
        private readonly List<(bool Panel, int Index)> _selection = new List<(bool Panel, int Index)>();
        private (bool Panel, int Index) _primary;

        private int _selectedTab = 1;
        private bool _writing;
        private bool _writeTaken;
        private bool _confirmingClose;
        private bool _confirmingBack;
        private bool _confirmingHeight;
        private bool _heightForCopy;
        private string _heightText = string.Empty;
        private bool _isRegenOpen;
        private Func<CabeceraEdit> _pendingRegen;
        private string _regenText = string.Empty;
        private IReadOnlyList<string> _regenLosses = Array.Empty<string>();
        private bool _isRestoreOpen;
        private bool _isRestoreLossesExpanded;
        private bool _isCopyOpen;
        private bool _sidesTogether;
        private bool _isConnectionsExpanded;
        private string _addClear = DefaultNewPanelClear;
        private string _message = string.Empty;
        private bool _messageIsError;

        private IReadOnlyList<object> _essentials = Array.Empty<object>();
        private IReadOnlyList<SelectiveCalcItem> _calculated = Array.Empty<SelectiveCalcItem>();
        private IReadOnlyList<string> _changes = Array.Empty<string>();
        private IReadOnlyList<string> _notices = Array.Empty<string>();
        private IReadOnlyList<CabeceraElevationRow> _rows = Array.Empty<CabeceraElevationRow>();
        private IReadOnlyList<object> _panelFields = Array.Empty<object>();
        private IReadOnlyList<object> _connectionFields = Array.Empty<object>();
        private IReadOnlyList<object> _horizontalFields = Array.Empty<object>();
        private IReadOnlyList<CabeceraSideColumn> _sideColumns = Array.Empty<CabeceraSideColumn>();
        private IReadOnlyList<object> _celosiaFields = Array.Empty<object>();
        private IReadOnlyList<SelectiveCalcItem> _fixedValues = Array.Empty<SelectiveCalcItem>();
        private readonly IReadOnlyList<CabeceraRestoreBox> _restoreBoxes;
        private IReadOnlyList<string> _restoreLosses = Array.Empty<string>();
        private IReadOnlyList<CabeceraCopyTargetItem> _copyTargets = Array.Empty<CabeceraCopyTargetItem>();

        private CabeceraEditorViewModel(CabeceraDraftEditor editor, ICabeceraEditorHost host, Action<CabeceraEditorViewModel> update, bool wasCustom, string rackId)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _host = host;
            _update = update;
            _wasCustom = wasCustom;
            RackId = rackId;
            _sidesTogether = editor.SidesAreEqual;
            _restoreBoxes = Enum.GetValues(typeof(CabeceraChangeCategory)).Cast<CabeceraChangeCategory>()
                .Select(c => new CabeceraRestoreBox(c, RefreshRestoreLosses)).ToList();
            _primary = (editor.PanelCount > 0, 0);
            _selection.Add(_primary);
            Rebuild();
        }

        /// <summary>The cabecera of a post or a module inside the editor of its rack (CB-01, B): it only writes the draft of that rack.</summary>
        public static CabeceraEditorViewModel Embedded(CabeceraDraftEditor editor, ICabeceraEditorHost host, bool wasCustom)
            => new CabeceraEditorViewModel(editor, host ?? throw new ArgumentNullException(nameof(host)), null, wasCustom, null);

        /// <summary>A cabecera of its own (a rack «Cabecera N» of the list, CB-01, A): «Actualizar» hands the editor to the window.</summary>
        public static CabeceraEditorViewModel Standalone(CabeceraDraftEditor editor, string rackId, Action<CabeceraEditorViewModel> update)
            => new CabeceraEditorViewModel(editor, null, update, false, rackId);

        public CabeceraDraftEditor Editor => _editor;

        public string RackId { get; }

        public bool IsEmbedded => _host != null;

        public bool IsStandalone => _host == null;

        /// <summary>The name of a cabecera of its own (a cabecera of a post has none, inventory dolor 13).</summary>
        public string Name
        {
            get => _editor.Configuration.Name ?? string.Empty;
            set
            {
                if (IsEmbedded || !CanEdit) return;
                if (string.Equals((value ?? string.Empty).Trim(), Name.Trim(), StringComparison.Ordinal)) return;
                _editor.SetName(value);
                Rebuild();
            }
        }

        public bool IsNameReadOnly => !CanEdit;

        public string HostName => _host?.HostName ?? string.Empty;

        public string BackText => "‹ " + HostName;

        public string BackAccessibleText => "Volver al editor de " + HostName;

        public string Title => _host?.Title ?? Name;

        public string CopyLabel => _host?.CopyLabel ?? "Copiar a otros postes…";

        public string CopyTitle => CopyLabel.Replace("…", string.Empty).Replace("Copiar a", "Copiar esta cabecera a");

        public bool IsWriting => _writing;

        public bool CanEdit => !_writing;

        public bool IsDirty => _editor.IsDirty;

        public bool IsCustom => _editor.Changes().Count > 0 || (_wasCustom && !IsDirty);

        public string StatusText => IsEmbedded
            ? (IsDirty ? "Cambios sin aplicar" : IsCustom ? "Personalizada" : "Calculada")
            : _writing ? "Escribiendo" : IsDirty ? "Modificado" : "Limpio";

        public string DiffText
        {
            get
            {
                var changes = _editor.Changes().Count;
                var head = IsDirty ? (IsEmbedded ? "Cambios sin aplicar al borrador" : "Cambios sin escribir") : (IsEmbedded ? "Sin cambios que aplicar." : "Sin cambios que escribir.");
                return changes > 0 ? head + " · Personalizada: " + Count(changes, "cambio", "cambios") + " respecto al estándar" : head;
            }
        }

        public string SubLine => IsEmbedded
            ? "Editas una copia de la cabecera de " + (_editor.Context.Where ?? Title) + " de " + HostName + ". «Aplicar al borrador» la pasa al rack, que queda «Modificado»; el dibujo cambia con «Actualizar» de " + HostName + ". Nada aquí escribe en AutoCAD."
            : "Cabecera · plantilla «" + (_editor.Template?.Name ?? string.Empty) + "». Lee y escribe por su puerta (RACKCABECERAABRIR / RACKCABECERAAPLICAR), sin bloquear AutoCAD.";

        // ------------------------------------------------------------------ the head: standalone

        public bool CanUpdate => IsStandalone && !_writing && IsDirty && _update != null;

        public string UpdateBlockedText
        {
            get
            {
                if (!IsStandalone) return string.Empty;
                if (_writing) return _writeTaken ? TakenWriteText : WaitingWriteText;
                if (_update == null) return "La ventana no tiene la puerta de AutoCAD para escribir la cabecera.";
                return string.Empty;
            }
        }

        public bool CanDiscard => !_writing && IsDirty;

        public bool CanRestore => !_writing && _editor.Changes().Count > 0;

        public bool CanClose => !_writing;

        public bool IsConfirmingClose
        {
            get => _confirmingClose;
            private set
            {
                if (!SetProperty(ref _confirmingClose, value)) return;
                if (value) CloseInlinePanels(except: nameof(IsConfirmingClose));
            }
        }

        public string CloseQuestion => "La cabecera tiene cambios que no están en el dibujo. Cerrar el editor no los escribe; el dibujo tampoco se cierra con cambios sin escribir. ¿Descartarlos y cerrar?";

        /// <summary>Raised when the editor asks the window to forget it (standalone): «Cerrar el editor».</summary>
        public event Action<CabeceraEditorViewModel> CloseRequested;

        public void RequestWrite()
        {
            if (!CanUpdate) return;
            _update(this);
        }

        public void RequestClose()
        {
            if (_writing) return;
            if (IsDirty)
            {
                IsConfirmingClose = true;
                return;
            }

            CloseRequested?.Invoke(this);
        }

        public void ConfirmClose(bool discard)
        {
            IsConfirmingClose = false;
            if (!discard || _writing) return;
            CloseRequested?.Invoke(this);
        }

        /// <summary>The window starts a write (standalone): nothing can be edited until its answer.</summary>
        public void SetWriting(bool writing)
        {
            _writing = writing;
            if (!writing) _writeTaken = false;
            if (writing) CloseInlinePanels(except: null);
            Rebuild();
        }

        /// <summary>AutoCAD took the write: «Dejar de esperar» is no longer offered (S-28).</summary>
        public void WriteTaken()
        {
            _writeTaken = true;
            RaiseState();
        }

        /// <summary>The answer of a write (standalone): written = «Limpio» on what was written; else it stays «Modificado».</summary>
        public void MarkWritten(bool written, string message)
        {
            _writing = false;
            _writeTaken = false;
            if (written) _editor.MarkWritten();
            Say(message, !written);
            Rebuild();
        }

        // ------------------------------------------------------------------ the head: embedded

        public bool CanApply => IsEmbedded && !_writing && IsDirty;

        public bool CanCopy => IsEmbedded && !_writing && (_host?.CopyTargets?.Count ?? 0) > 0;

        public bool IsConfirmingBack
        {
            get => _confirmingBack;
            private set
            {
                if (!SetProperty(ref _confirmingBack, value)) return;
                if (value) CloseInlinePanels(except: nameof(IsConfirmingBack));
            }
        }

        public string BackQuestion => "La cabecera de " + (_editor.Context.Where ?? Title) + " tiene cambios que aún no están en el borrador de " + HostName + ".";

        public bool IsConfirmingHeight
        {
            get => _confirmingHeight;
            private set
            {
                if (!SetProperty(ref _confirmingHeight, value)) return;
                if (value) CloseInlinePanels(except: nameof(IsConfirmingHeight));
            }
        }

        public string HeightQuestion => _heightText;

        public string HeightNote
        {
            get
            {
                var note = _editor.HeightNote();
                return note.Severity == CabeceraHeightSeverity.None ? string.Empty
                    : (note.Severity == CabeceraHeightSeverity.Severe ? "Por debajo del nivel de carga. " : "Aviso. ") + note.Text;
            }
        }

        /// <summary>«Aplicar al borrador» (CB-09: below the load level it asks «Aplicar igual» inline first).</summary>
        public void Apply() => ApplyToHost(force: false, back: false);

        /// <summary>«Aplicar y volver» of the question of «‹ Rack».</summary>
        public void ApplyAndBack() => ApplyToHost(force: false, back: true);

        /// <summary>«Aplicar igual» of the height question.</summary>
        public void ConfirmHeight(bool apply)
        {
            IsConfirmingHeight = false;
            if (!apply) return;
            if (_heightForCopy) CopyToHost(force: true);
            else ApplyToHost(force: true, back: false);
        }

        /// <summary>«‹ Rack»: with changes not applied it asks first (CB-10 of the inventory, dolor 10: closing is no longer applying).</summary>
        public void Back()
        {
            if (_host == null) return;
            if (IsDirty)
            {
                IsConfirmingBack = true;
                return;
            }

            _host.Back();
        }

        public void ConfirmBack(bool drop)
        {
            IsConfirmingBack = false;
            if (drop) _host?.Back();
        }

        public void CancelBack() => IsConfirmingBack = false;

        private void ApplyToHost(bool force, bool back)
        {
            if (_host == null || _writing) return;
            IsConfirmingBack = false;
            if (!IsDirty)
            {
                if (back) _host.Back();
                return;
            }

            CabeceraHostAnswer answer;
            try
            {
                answer = _host.Apply(_editor.Snapshot(), force);
            }
            catch (InvalidOperationException ex)
            {
                answer = CabeceraHostAnswer.Refuse(ex.Message);
            }

            if (answer.NeedsConfirmation)
            {
                _heightForCopy = false;
                _heightText = answer.Message;
                OnPropertyChanged(nameof(HeightQuestion));
                IsConfirmingHeight = true;
                return;
            }

            if (!answer.Applied)
            {
                Say(answer.Message, true);
                return;
            }

            // The host closes the embedded editor and says what it did; nothing else to do here.
        }

        // ------------------------------------------------------------------ «Copiar a otros postes…» (CB-10)

        public bool IsCopyOpen
        {
            get => _isCopyOpen;
            private set
            {
                if (!SetProperty(ref _isCopyOpen, value)) return;
                if (value) CloseInlinePanels(except: nameof(IsCopyOpen));
            }
        }

        public IReadOnlyList<CabeceraCopyTargetItem> CopyTargets => _copyTargets;

        public int ChosenCopyCount => _copyTargets.Count(t => t.IsChecked);

        public bool CanConfirmCopy => !_writing && ChosenCopyCount > 0;

        public string CopyConfirmLabel => ChosenCopyCount > 0 ? "Aplicar aquí y copiar a " + Count(ChosenCopyCount, "destino", "destinos") : "Elige un destino";

        public string CopyNote => "Cada copia toma la profundidad de su fondo y el peralte de su poste; si un destino no vale, no se copia nada.";

        public void OpenCopy()
        {
            if (!CanCopy) return;
            _copyTargets = (_host.CopyTargets ?? Array.Empty<CabeceraCopyTarget>()).Select(t => new CabeceraCopyTargetItem(t, RaiseCopy)).ToList();
            OnPropertyChanged(nameof(CopyTargets));
            RaiseCopy();
            IsCopyOpen = true;
        }

        public void CopyAll(bool on)
        {
            foreach (var target in _copyTargets) target.IsChecked = on;
        }

        public void CancelCopy() => IsCopyOpen = false;

        public void ConfirmCopy() => CopyToHost(force: false);

        private void CopyToHost(bool force)
        {
            if (_host == null || _writing) return;
            var chosen = _copyTargets.Where(t => t.IsChecked).Select(t => t.Target).ToList();
            if (chosen.Count == 0) return;
            CabeceraHostAnswer answer;
            try
            {
                answer = _host.Copy(_editor.Snapshot(), chosen, force);
            }
            catch (InvalidOperationException ex)
            {
                answer = CabeceraHostAnswer.Refuse(ex.Message);
            }

            if (answer.NeedsConfirmation)
            {
                _heightForCopy = true;
                _heightText = answer.Message;
                OnPropertyChanged(nameof(HeightQuestion));
                IsConfirmingHeight = true;
                return;
            }

            if (!answer.Applied) Say(answer.Message, true);
        }

        private void RaiseCopy()
        {
            OnPropertyChanged(nameof(ChosenCopyCount));
            OnPropertyChanged(nameof(CanConfirmCopy));
            OnPropertyChanged(nameof(CopyConfirmLabel));
        }

        // ------------------------------------------------------------------ «Descartar» and «Restaurar estándar…»

        public void Discard()
        {
            if (!CanDiscard) return;
            _editor.Discard();
            _sidesTogether = _editor.SidesAreEqual;
            CloseInlinePanels(except: null);
            CleanSelection();
            Say(IsEmbedded ? "Cambios descartados: la copia vuelve a la cabecera de " + (_editor.Context.Where ?? Title) + " como está en el borrador del rack." : "Borrador descartado: se muestra lo último que se leyó del dibujo o se escribió en él.", false);
            Rebuild();
        }

        public bool IsRestoreOpen
        {
            get => _isRestoreOpen;
            private set
            {
                if (!SetProperty(ref _isRestoreOpen, value)) return;
                if (value) CloseInlinePanels(except: nameof(IsRestoreOpen));
            }
        }

        public IReadOnlyList<CabeceraRestoreBox> RestoreBoxes => _restoreBoxes;

        public string RestoreIntro => "Vuelve a la plantilla «" + (_editor.Template?.Name ?? string.Empty) + "» a " + CabeceraDraftEditor.Inches(_editor.GeneratedHeight)
                                      + ", con el poste y la profundidad de ahora. Marca lo que quieres restaurar:";

        public string RestoreFoot => IsEmbedded ? "Va a la copia: " + HostName + " no cambia hasta «Aplicar al borrador»." : "Va al borrador: el dibujo no cambia hasta «Actualizar».";

        public IReadOnlyList<string> RestoreLosses => _restoreLosses;

        public bool HasRestoreLosses => _restoreLosses.Count > 0;

        public bool IsRestoreLossesExpanded { get => _isRestoreLossesExpanded; set => SetProperty(ref _isRestoreLossesExpanded, value); }

        public bool CanConfirmRestore => !_writing && _restoreLosses.Count > 0;

        public void OpenRestore()
        {
            if (!CanRestore) return;
            foreach (var box in _restoreBoxes) box.IsChecked = true;
            IsRestoreLossesExpanded = false;
            RefreshRestoreLosses();
            IsRestoreOpen = true;
        }

        public void CancelRestore() => IsRestoreOpen = false;

        public void ConfirmRestore()
        {
            if (!CanConfirmRestore) return;
            var categories = _restoreBoxes.Where(b => b.IsChecked && b.IsEnabled).Select(b => b.Category).ToList();
            var result = _editor.Restore(categories);
            IsRestoreOpen = false;
            if (!result.Applied)
            {
                Say(result.Error, true);
                return;
            }

            _sidesTogether = _editor.SidesAreEqual || _sidesTogether;
            CleanSelection();
            Say(result.Notice, false);
            Rebuild();
        }

        private void RefreshRestoreLosses()
        {
            var changes = _editor.Changes();
            foreach (var box in _restoreBoxes) box.SetCount(changes.Count(c => c.Category == box.Category));
            var marked = _restoreBoxes.Where(b => b.IsChecked && b.IsEnabled).Select(b => b.Category).ToList();
            _restoreLosses = changes.Where(c => marked.Contains(c.Category)).Select(c => c.Line).ToList();
            OnPropertyChanged(nameof(RestoreLosses));
            OnPropertyChanged(nameof(HasRestoreLosses));
            OnPropertyChanged(nameof(CanConfirmRestore));
            OnPropertyChanged(nameof(RestoreIntro));
        }

        // ------------------------------------------------------------------ regenerate (CB-02)

        public bool IsRegenOpen
        {
            get => _isRegenOpen;
            private set
            {
                if (!SetProperty(ref _isRegenOpen, value)) return;
                if (value) CloseInlinePanels(except: nameof(IsRegenOpen));
            }
        }

        public string RegenText => _regenText;

        public IReadOnlyList<string> RegenLosses => _regenLosses;

        public void ConfirmRegen(bool regenerate)
        {
            var pending = _pendingRegen;
            _pendingRegen = null;
            IsRegenOpen = false;
            if (!regenerate || pending == null)
            {
                Rebuild();
                return;
            }

            var result = pending();
            if (!result.Applied)
            {
                Say(result.Error, true);
                Rebuild();
                return;
            }

            CleanSelection(resetToFirst: true);
            Say(result.Notice, false);
            Rebuild();
        }

        // ------------------------------------------------------------------ messages

        public string Message { get => _message; private set => SetProperty(ref _message, value ?? string.Empty); }

        public bool MessageIsError { get => _messageIsError; private set => SetProperty(ref _messageIsError, value); }

        /// <summary>A failure of the view, contained and said in the message of the editor.</summary>
        public void Fail(Exception ex) => Say("El editor de cabeceras tuvo un fallo contenido: " + ex?.Message, true);

        private void Say(string text, bool error)
        {
            Message = text ?? string.Empty;
            MessageIsError = error && !string.IsNullOrEmpty(text);
        }

        // ------------------------------------------------------------------ tabs

        /// <summary>0 Cabecera, 1 Alzado (the first one, CB-04), 2 Postes y placas, 3 Avanzado.</summary>
        public int SelectedTab { get => _selectedTab; set => SetProperty(ref _selectedTab, value); }

        public IReadOnlyList<object> Essentials => _essentials;

        public IReadOnlyList<SelectiveCalcItem> Calculated => _calculated;

        public IReadOnlyList<string> Changes => _changes;

        public bool HasChanges => _changes.Count > 0;

        public string ChangesSummary => _changes.Count > 0 ? Count(_changes.Count, "cambio", "cambios") + ": la cabecera es «Personalizada»" : "La cabecera es la estándar";

        public string NoChangesText => _changes.Count > 0 ? string.Empty : "Ninguno: es la cabecera que da la plantilla con este alto, este poste y esta profundidad.";

        public IReadOnlyList<string> Notices => _notices;

        public string NoNoticesText => _notices.Count > 0 ? string.Empty : "Sin avisos: el modelo es consistente.";

        // ------------------------------------------------------------------ «Alzado» (CB-04)

        public IReadOnlyList<CabeceraElevationRow> Rows => _rows;

        public string ElevationSummary => Count(_editor.PanelCount, "panel", "paneles") + " · " + Count(_editor.HorizontalCount, "horizontal", "horizontales")
                                          + " · " + CabeceraDraftEditor.Inches(_editor.Height) + " de alto";

        public string BaseText => "placa base en los dos postes · " + CabeceraDraftEditor.Num(_editor.Height) + " × " + CabeceraDraftEditor.Num(_editor.Configuration.Depth) + " in";

        public bool AllPanelsSelected => SelectedPanels.Count == _editor.PanelCount && _editor.PanelCount > 0 && SelectedHorizontals.Count == 0;

        public bool AllHorizontalsSelected => SelectedHorizontals.Count == _editor.HorizontalCount && SelectedPanels.Count == 0;

        public bool EverythingSelected => SelectedPanels.Count == _editor.PanelCount && SelectedHorizontals.Count == _editor.HorizontalCount;

        public IReadOnlyList<int> SelectedPanels => _selection.Where(s => s.Panel).Select(s => s.Index).OrderBy(i => i).ToList();

        public IReadOnlyList<int> SelectedHorizontals => _selection.Where(s => !s.Panel).Select(s => s.Index).OrderBy(i => i).ToList();

        public string AddClear
        {
            get => _addClear;
            set
            {
                if (!SetProperty(ref _addClear, value ?? string.Empty)) return;
                OnPropertyChanged(nameof(CanAddPanel));
                OnPropertyChanged(nameof(AddPanelNote));
            }
        }

        public bool CanAddPanel => CanEdit && TryNumber(_addClear, out var clear) && clear > 0.0;

        public string AddPanelNote => "La altura del poste pasa a " + (TryNumber(_addClear, out var clear) && clear > 0.0
            ? CabeceraDraftEditor.Inches(_editor.TopElevation + clear + RackFrameConfigurationFactory.PostTopRemate) : "—") + ".";

        public string InspectorTitle
        {
            get
            {
                var panels = SelectedPanels;
                var horizontals = SelectedHorizontals;
                if (panels.Count == 1 && horizontals.Count == 0)
                {
                    var panel = _editor.Panels[panels[0]];
                    return "P" + (panels[0] + 1) + " · panel de " + CabeceraDraftEditor.Inches(panel.EndElevation - panel.StartElevation);
                }

                if (horizontals.Count == 1 && panels.Count == 0)
                    return "H" + (horizontals[0] + 1) + " · horizontal a " + CabeceraDraftEditor.Inches(_editor.Horizontals[horizontals[0]].Elevation);
                if (panels.Count > 0 && horizontals.Count > 0) return "Selección: " + Count(panels.Count, "panel", "paneles") + " y " + Count(horizontals.Count, "horizontal", "horizontales");
                return panels.Count > 0 ? Count(panels.Count, "panel", "paneles") : Count(horizontals.Count, "horizontal", "horizontales");
            }
        }

        public string TargetText
        {
            get
            {
                var parts = new List<string>();
                if (SelectedPanels.Count > 0) parts.Add(string.Join(", ", SelectedPanels.Select(i => "P" + (i + 1))));
                if (SelectedHorizontals.Count > 0) parts.Add(string.Join(", ", SelectedHorizontals.Select(i => "H" + (i + 1))));
                return "Destino: " + string.Join(" · ", parts) + (IsEmbedded ? " (en la copia)" : string.Empty);
            }
        }

        public bool HasPanelSelection => SelectedPanels.Count > 0;

        public bool HasHorizontalSelection => SelectedHorizontals.Count > 0;

        public string PanelGroupTitle => SelectedPanels.Count > 1 ? "Paneles (" + SelectedPanels.Count + ")" : "Panel";

        public string HorizontalGroupTitle => SelectedHorizontals.Count > 1 ? "Horizontales (" + SelectedHorizontals.Count + ")" : "Horizontal";

        public IReadOnlyList<object> PanelFields => _panelFields;

        public IReadOnlyList<object> ConnectionFields => _connectionFields;

        public bool IsConnectionsExpanded { get => _isConnectionsExpanded; set => SetProperty(ref _isConnectionsExpanded, value); }

        public string ConnectionsSummary
        {
            get
            {
                var panels = SelectedPanels.Select(i => _editor.Panels[i]).ToList();
                if (panels.Count == 0) return string.Empty;
                var mixed = panels.Select(p => p.StartConnectionPointId).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1
                            || panels.Select(p => p.EndConnectionPointId).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1;
                return mixed ? Varies : "En el poste → en el travesaño";
            }
        }

        public IReadOnlyList<object> HorizontalFields => _horizontalFields;

        public bool CanSplit => CanEdit && SelectedPanels.Count == 1 && SelectedHorizontals.Count == 0 && PanelClear(SelectedPanels[0]) >= 4.0;

        public bool CanCombine => CanEdit && SelectedPanels.Count == 1 && SelectedHorizontals.Count == 0 && SelectedPanels[0] < _editor.PanelCount - 1 && _editor.HorizontalCount > 2;

        public bool CanPanelsToStandard => CanEdit && SelectedPanels.Any(_editor.PanelDiffers);

        public bool CanDuplicate => CanEdit && SelectedHorizontals.Count == 1;

        public bool CanDelete => CanEdit && SelectedHorizontals.Count > 0 && _editor.HorizontalCount - SelectedHorizontals.Count >= 2;

        public string TopNote => SelectedHorizontals.Contains(_editor.HorizontalCount - 1)
            ? "H" + _editor.HorizontalCount + " es la última: moverla cambia la altura del poste (esta + 4 in) sin regenerar nada." : string.Empty;

        /// <summary>A click on a row: alone, with Ctrl (add or remove) or with Shift (a range of the same kind from the primary one).</summary>
        public void ClickRow(bool isPanel, int index, bool ctrl, bool shift)
        {
            var key = (isPanel, index);
            if (shift && _primary.Panel == isPanel)
            {
                var from = Math.Min(_primary.Index, index);
                var to = Math.Max(_primary.Index, index);
                _selection.Clear();
                for (var i = from; i <= to; i++) _selection.Add((isPanel, i));
            }
            else if (ctrl)
            {
                if (_selection.Contains(key))
                {
                    if (_selection.Count == 1) return;
                    _selection.Remove(key);
                    if (_primary == key) _primary = _selection[0];
                }
                else
                {
                    _selection.Add(key);
                    _primary = key;
                }
            }
            else
            {
                _selection.Clear();
                _selection.Add(key);
                _primary = key;
            }

            Rebuild();
        }

        /// <summary>«Paneles», «Horizontales» or «Todo».</summary>
        public void SelectKind(bool panels, bool horizontals)
        {
            var next = new List<(bool, int)>();
            if (panels) next.AddRange(Enumerable.Range(0, _editor.PanelCount).Select(i => (true, i)));
            if (horizontals) next.AddRange(Enumerable.Range(0, _editor.HorizontalCount).Select(i => (false, i)));
            if (next.Count == 0) return;
            _selection.Clear();
            _selection.AddRange(next);
            _primary = next[0];
            Rebuild();
        }

        public void AddPanel()
        {
            if (!CanAddPanel || !TryNumber(_addClear, out var clear)) return;
            var result = _editor.AddPanelOnTop(clear);
            if (!result.Applied)
            {
                Say(result.Error, true);
                return;
            }

            _selection.Clear();
            _primary = (true, _editor.PanelCount - 1);
            _selection.Add(_primary);
            Say(result.Notice, false);
            Rebuild();
        }

        public void SplitPanel() => Structural(() => _editor.SplitPanel(SelectedPanels[0]), CanSplit, () => { var i = SelectedPanels[0]; return new[] { (true, i), (true, i + 1) }; });

        public void CombinePanel() => Structural(() => _editor.CombineWithAbove(SelectedPanels[0]), CanCombine, () => new[] { (true, SelectedPanels[0]) });

        public void PanelsToStandard() => Structural(() => _editor.PanelsToStandard(SelectedPanels), CanPanelsToStandard, null);

        public void DuplicateHorizontal()
        {
            if (!CanDuplicate) return;
            var source = SelectedHorizontals[0];
            var target = _editor.Horizontals[source].Elevation + (_editor.Configuration.PanelClear > 0.0 ? _editor.Configuration.PanelClear : 44.0);
            var result = _editor.DuplicateAbove(source);
            if (!result.Applied)
            {
                Say(result.Error, true);
                Rebuild();
                return;
            }

            var index = _editor.Horizontals.ToList().FindIndex(h => Math.Abs(h.Elevation - target) < 0.01);
            _selection.Clear();
            _primary = (false, Math.Max(0, index));
            _selection.Add(_primary);
            Say(result.Notice, false);
            Rebuild();
        }

        public void DeleteHorizontals() => Structural(() => _editor.DeleteHorizontals(SelectedHorizontals), CanDelete, () => new[] { (true, 0) });

        // ------------------------------------------------------------------ «Postes y placas» (CB-07)

        /// <summary>«Igual en los dos postes»: on with different posts the right one takes the left one.</summary>
        public bool SidesTogether
        {
            get => _sidesTogether;
            set
            {
                if (!CanEdit || _sidesTogether == value) return;
                if (value && !_editor.SidesAreEqual)
                {
                    var result = _editor.MakeSidesEqual();
                    Say(result.Notice, false);
                }

                _sidesTogether = value;
                OnPropertyChanged();
                Rebuild();
            }
        }

        public IReadOnlyList<CabeceraSideColumn> SideColumns => _sideColumns;

        public string SidesSummary
        {
            get
            {
                string Of(PostAssembly post) => post == null || !post.HasReinforcement ? "sin refuerzo" : "reforzado " + CabeceraDraftEditor.Inches(post.ReinforcementHeight);
                return _editor.SidesAreEqual ? "Los dos iguales: " + Of(_editor.Side(PostSide.Left))
                    : "Izquierdo: " + Of(_editor.Side(PostSide.Left)) + " · Derecho: " + Of(_editor.Side(PostSide.Right));
            }
        }

        // ------------------------------------------------------------------ «Avanzado»

        public IReadOnlyList<object> CelosiaFields => _celosiaFields;

        public IReadOnlyList<SelectiveCalcItem> FixedValues => _fixedValues;

        // ------------------------------------------------------------------ the rebuild

        private void Rebuild()
        {
            CleanSelection();
            MergeList(ref _essentials, BuildEssentials(), nameof(Essentials));
            _calculated = BuildCalculated();
            _changes = _editor.Changes().Select(c => c.Line).ToList();
            _notices = _editor.Notices();
            BuildRows();
            MergeList(ref _panelFields, BuildPanelFields(), nameof(PanelFields));
            MergeList(ref _connectionFields, BuildConnectionFields(), nameof(ConnectionFields));
            MergeList(ref _horizontalFields, BuildHorizontalFields(), nameof(HorizontalFields));
            MergeColumns(BuildSideColumns());
            MergeList(ref _celosiaFields, BuildCelosiaFields(), nameof(CelosiaFields));
            _fixedValues = BuildFixed();
            if (_isRestoreOpen)
            {
                RefreshRestoreLosses();
                if (_restoreLosses.Count == 0 && _restoreBoxes.All(b => !b.IsEnabled)) IsRestoreOpen = false;
            }

            RaiseState();
        }

        private void RaiseState()
        {
            foreach (var name in new[]
            {
                nameof(Name), nameof(IsNameReadOnly), nameof(Title), nameof(IsWriting), nameof(CanEdit), nameof(IsDirty), nameof(IsCustom), nameof(StatusText),
                nameof(DiffText), nameof(SubLine), nameof(CanUpdate), nameof(UpdateBlockedText), nameof(CanDiscard), nameof(CanRestore), nameof(CanClose),
                nameof(CanApply), nameof(CanCopy), nameof(HeightNote), nameof(Calculated), nameof(Changes), nameof(HasChanges), nameof(ChangesSummary),
                nameof(NoChangesText), nameof(Notices), nameof(NoNoticesText), nameof(ElevationSummary), nameof(BaseText), nameof(AllPanelsSelected),
                nameof(AllHorizontalsSelected), nameof(EverythingSelected), nameof(SelectedPanels), nameof(SelectedHorizontals), nameof(CanAddPanel),
                nameof(AddPanelNote), nameof(InspectorTitle), nameof(TargetText), nameof(HasPanelSelection), nameof(HasHorizontalSelection),
                nameof(PanelGroupTitle), nameof(HorizontalGroupTitle), nameof(ConnectionsSummary), nameof(CanSplit), nameof(CanCombine),
                nameof(CanPanelsToStandard), nameof(CanDuplicate), nameof(CanDelete), nameof(TopNote), nameof(SidesTogether), nameof(SidesSummary),
                nameof(FixedValues), nameof(RestoreIntro), nameof(RestoreFoot), nameof(CanConfirmRestore), nameof(CanConfirmCopy),
            })
            {
                OnPropertyChanged(name);
            }
        }

        private IReadOnlyList<object> BuildEssentials()
        {
            var readOnly = !CanEdit;
            var context = _editor.Context;
            var templates = _editor.Templates.Select(t => new SelectiveChoiceOption(t.Id, TemplateLabel(t))).ToList();
            var template = new SelectiveChoiceItem("tpl", "Plantilla", templates, _editor.TemplateId,
                "Da perfiles, arreglo y placa; conserva el alto y la profundidad.", value => Regenerate(() => _editor.SetTemplate(value),
                    "Cambiar a la plantilla «" + TemplateLabel(_editor.Templates.FirstOrDefault(t => t.Id == value)) + "»"), readOnly);
            var posts = Options(_editor.Catalog.PostProfiles, _editor.Side(PostSide.Left)?.PostCatalogId);
            var post = new SelectiveChoiceItem("post", "Poste", posts, _editor.Side(PostSide.Left)?.PostCatalogId,
                "Los dos postes; el refuerzo, en «Postes y placas».", value => Apply(() => _editor.SetPostProfile(value)), readOnly);
            var height = new DynamicFieldItem("alto", "Alto", "in", CabeceraDraftEditor.Num(_editor.Height),
                "La altura del poste: la última horizontal (" + CabeceraDraftEditor.Inches(_editor.TopElevation) + ") + 4 in. Escribir otro alto regenera desde la plantilla.",
                text =>
                {
                    if (!TryNumber(text, out var value) || !(value > 0.0)) return "El alto debe ser un número mayor que cero.";
                    return Regenerate(() => _editor.SetHeight(value), "Cambiar el alto a " + CabeceraDraftEditor.Inches(value));
                }, readOnly);
            var depthLocked = context.LockedDepth.HasValue;
            var depth = new DynamicFieldItem("depth", "Profundidad", "in", CabeceraDraftEditor.Num(_editor.Configuration.Depth),
                depthLocked ? context.DepthOrigin ?? "La da el rack." : "Hoy «Fondo». De la cara frontal a la posterior.",
                text =>
                {
                    if (!TryNumber(text, out var value) || !(value > 0.0)) return "La profundidad debe ser un número mayor que cero.";
                    return Apply(() => _editor.SetDepth(value));
                }, readOnly || depthLocked);
            var peralteLocked = context.LockedPostPeralte.HasValue;
            var own = _editor.OwnPostPeralte();
            var peralteHint = peralteLocked ? context.PostPeralteOrigin ?? "Lo da el rack."
                : context.Embedded ? (own.HasValue ? string.Empty : "Del rack: " + CabeceraDraftEditor.Num(context.RackPostPeralte) + ". ") + (context.PostPeralteOrigin ?? "Vacío = el del rack.")
                : (own.HasValue ? string.Empty : "Del perfil. ") + "Vacío = el ancho del perfil. Cambia la planta; no la lateral.";
            var peralte = new DynamicFieldItem("peralte", "Peralte del poste", "in", peralteLocked ? CabeceraDraftEditor.Num(context.LockedPostPeralte.Value) : own.HasValue ? CabeceraDraftEditor.Num(own.Value) : string.Empty,
                peralteHint, text =>
                {
                    if (string.IsNullOrWhiteSpace(text)) return Apply(() => _editor.SetPostPeralte(null));
                    if (!TryNumber(text, out var value) || !(value > 0.0)) return "El peralte debe ser un número mayor que cero (vacío = " + (context.Embedded ? "el del rack" : "el del perfil") + ").";
                    return Apply(() => _editor.SetPostPeralte(value));
                }, readOnly || peralteLocked);
            return new object[] { template, post, height, depth, peralte };
        }

        private IReadOnlyList<SelectiveCalcItem> BuildCalculated()
        {
            var panels = _editor.Panels;
            var braced = panels.Count(p => p.Arrangement != BracingPattern.NoBracing);
            var standard = _editor.Standard();
            var stdBraced = standard?.BracingPanels.Count(p => p.Arrangement != BracingPattern.NoBracing) ?? 0;
            var list = new List<SelectiveCalcItem>
            {
                new SelectiveCalcItem("Altura del poste", CabeceraDraftEditor.Inches(_editor.Height), "La última horizontal + 4 in de remate."),
                new SelectiveCalcItem("Paneles", panels.Count.ToString(CultureInfo.InvariantCulture), braced + " con celosía · " + (panels.Count - braced) + " sin celosía"),
                new SelectiveCalcItem("Horizontales", _editor.HorizontalCount.ToString(CultureInfo.InvariantCulture), _editor.Horizontals.Count(h => h.Quantity > 1) + " dobles"),
                new SelectiveCalcItem("Profundidad", CabeceraDraftEditor.Inches(_editor.Configuration.Depth), _editor.Context.LockedDepth.HasValue ? "La del rack." : "La de esta cabecera."),
            };
            if (standard != null)
            {
                list.Add(new SelectiveCalcItem("Lo que da la plantilla", Count(standard.BracingPanels.Count, "panel", "paneles"),
                    "«" + TemplateLabel(_editor.Template) + "» a " + CabeceraDraftEditor.Inches(_editor.GeneratedHeight) + ": " + stdBraced + " con celosía y "
                    + (standard.BracingPanels.Count - stdBraced) + " de cierre, sin diagonal."));
            }

            return list;
        }

        private void BuildRows()
        {
            var horizontals = _editor.Horizontals;
            var panels = _editor.Panels;
            var next = new List<CabeceraElevationRow>();
            for (var i = horizontals.Count - 1; i >= 0; i--)
            {
                next.Add(Find(false, i) ?? new CabeceraElevationRow(false, i));
                if (i > 0) next.Add(Find(true, i - 1) ?? new CabeceraElevationRow(true, i - 1));
            }

            for (var r = 0; r < next.Count; r++)
            {
                var row = next[r];
                var selected = _selection.Contains((row.IsPanel, row.Index));
                var primary = selected && _primary == (row.IsPanel, row.Index);
                if (row.IsHorizontal)
                {
                    var h = horizontals[row.Index];
                    var isDouble = h.Quantity > 1;
                    var value = CabeceraDraftEditor.Inches(h.Elevation);
                    var text = "horizontal" + (isDouble ? " doble" : string.Empty) + (string.IsNullOrWhiteSpace(h.Notes) ? string.Empty : " · nota");
                    row.Set(value, text, selected, primary, _editor.HorizontalDiffers(row.Index), isDouble, null, string.Empty, 22.0,
                        row.Label + ", horizontal a " + value + (isDouble ? ", doble" : string.Empty) + (selected ? ", elegida" : string.Empty));
                }
                else
                {
                    var p = panels[row.Index];
                    var clear = p.EndElevation - p.StartElevation;
                    var value = CabeceraDraftEditor.Inches(clear);
                    var arrangement = CabeceraDraftEditor.ArrangementLabel(p.Arrangement);
                    var diagonal = p.Arrangement == BracingPattern.SingleDiagonal || p.Arrangement == BracingPattern.DoubleDiagonal;
                    var text = Lower(arrangement) + (diagonal ? " · " + Lower(CabeceraDraftEditor.DirectionLabel(p.DiagonalDirection)) : string.Empty);
                    var runs = p.ResolveDiagonalDirection();
                    Geometry geometry = null;
                    switch (p.Arrangement)
                    {
                        case BracingPattern.SingleDiagonal: geometry = runs == DiagonalDirection.UpLeft ? SingleLeft : SingleRight; break;
                        case BracingPattern.DoubleDiagonal: geometry = runs == DiagonalDirection.UpLeft ? DoubleLeft : DoubleRight; break;
                        case BracingPattern.XBracing: geometry = Cross; break;
                        case BracingPattern.KBracing: geometry = KBrace; break;
                    }

                    var none = geometry == null ? (p.Arrangement == BracingPattern.Custom ? "personalizado" : "sin celosía") : string.Empty;
                    var height = Math.Max(28.0, Math.Min(96.0, clear * 1.1));
                    row.Set(value, text, selected, primary, _editor.PanelDiffers(row.Index), false, geometry, none, height,
                        row.Label + ", panel de " + value + ", " + Lower(arrangement) + (selected ? ", elegido" : string.Empty));
                }
            }

            if (next.Count != _rows.Count || next.Where((row, i) => !ReferenceEquals(row, _rows[i])).Any())
            {
                _rows = next;
                OnPropertyChanged(nameof(Rows));
            }

            CabeceraElevationRow Find(bool panel, int index) => _rows.FirstOrDefault(r => r.IsPanel == panel && r.Index == index);
        }

        private IReadOnlyList<object> BuildPanelFields()
        {
            var panels = SelectedPanels;
            if (panels.Count == 0) return Array.Empty<object>();
            var readOnly = !CanEdit;
            var chosen = panels.Select(i => _editor.Panels[i]).ToList();
            var arrangements = Enum.GetValues(typeof(BracingPattern)).Cast<BracingPattern>()
                .Select(a => new SelectiveChoiceOption(a.ToString(), CabeceraDraftEditor.ArrangementLabel(a))).ToList();
            var directions = Enum.GetValues(typeof(DiagonalDirection)).Cast<DiagonalDirection>()
                .Select(d => new SelectiveChoiceOption(d.ToString(), CabeceraDraftEditor.DirectionLabel(d))).ToList();
            var anyDiagonal = chosen.Any(p => p.Arrangement == BracingPattern.SingleDiagonal || p.Arrangement == BracingPattern.DoubleDiagonal);
            var allNone = chosen.All(p => p.Arrangement == BracingPattern.NoBracing);
            var arrangement = Choice("pArr", "Arreglo", arrangements, chosen.Select(p => p.Arrangement.ToString()), "Hoy «Arreglo de panel».",
                value => Apply(() => _editor.SetPanelArrangement(SelectedPanels, (BracingPattern)Enum.Parse(typeof(BracingPattern), value))), readOnly);
            var direction = Choice("pDir", "Dirección", directions, chosen.Select(p => p.DiagonalDirection.ToString()),
                anyDiagonal ? "Auto alterna de un panel al siguiente." : "Solo en diagonal simple o doble.",
                value => Apply(() => _editor.SetPanelDirection(SelectedPanels, (DiagonalDirection)Enum.Parse(typeof(DiagonalDirection), value))), readOnly || !anyDiagonal);
            var profile = Choice("pProf", "Perfil de la diagonal", Options(_editor.Catalog.TrussProfiles, chosen.Select(p => p.DiagonalProfileId).ToArray()),
                chosen.Select(p => p.DiagonalProfileId), string.Empty, value => Apply(() => _editor.SetPanelProfile(SelectedPanels, value)), readOnly || allNone);
            var clears = chosen.Select(p => CabeceraDraftEditor.Num(p.EndElevation - p.StartElevation)).Distinct().ToList();
            var clear = new DynamicFieldItem("pClear", "Claro", "in", clears.Count == 1 ? clears[0] : string.Empty,
                clears.Count == 1 ? "La distancia entre sus dos horizontales; cambiarlo mueve lo de arriba y la altura del poste." : Varies + "; escribir aquí lo iguala.",
                text =>
                {
                    if (!TryNumber(text, out var value) || !(value > 0.0)) return "El claro debe ser un número mayor que cero.";
                    return Apply(() => _editor.SetPanelClear(SelectedPanels, value));
                }, readOnly);
            return new object[] { arrangement, direction, profile, clear };
        }

        private IReadOnlyList<object> BuildConnectionFields()
        {
            var panels = SelectedPanels;
            if (panels.Count == 0) return Array.Empty<object>();
            var readOnly = !CanEdit;
            var chosen = panels.Select(i => _editor.Panels[i]).ToList();
            // CB-12: only the points of their role (today every list offers the 15 points of the catalog, inventory dolor 17).
            var start = Choice("pStart", "Punto de inicio", Points("Poste", chosen.Select(p => p.StartConnectionPointId).ToArray()), chosen.Select(p => p.StartConnectionPointId),
                "En el poste.", value => Apply(() => _editor.SetPanelStartPoint(SelectedPanels, value)), readOnly);
            var end = Choice("pEnd", "Punto de fin", Points("Truss", chosen.Select(p => p.EndConnectionPointId).ToArray()), chosen.Select(p => p.EndConnectionPointId),
                "En el travesaño.", value => Apply(() => _editor.SetPanelEndPoint(SelectedPanels, value)), readOnly);
            return new object[] { start, end };
        }

        private IReadOnlyList<object> BuildHorizontalFields()
        {
            var horizontals = SelectedHorizontals;
            if (horizontals.Count == 0) return Array.Empty<object>();
            var readOnly = !CanEdit;
            var chosen = horizontals.Select(i => _editor.Horizontals[i]).ToList();
            var list = new List<object>();
            var last = _editor.HorizontalCount - 1;
            if (horizontals.Count == 1)
            {
                var index = horizontals[0];
                list.Add(new DynamicFieldItem("hElev", "Elevación", "in", CabeceraDraftEditor.Num(chosen[0].Elevation),
                    index == last ? "La última: la altura del poste es esta + 4 in." : index == 0 ? "La primera: en el troquel de inicio." : "Desde la base del poste.",
                    text =>
                    {
                        if (!TryNumber(text, out var value) || !(value >= 0.0)) return "La elevación debe ser un número de 0 o más.";
                        var moved = chosen[0];
                        var error = Apply(() => _editor.SetElevation(index, value));
                        if (error == null)
                        {
                            var at = _editor.Horizontals.ToList().IndexOf(moved);
                            if (at >= 0)
                            {
                                _selection.Clear();
                                _primary = (false, at);
                                _selection.Add(_primary);
                                Rebuild();
                            }
                        }

                        return error;
                    }, readOnly));
            }
            else
            {
                list.Add(new DynamicFieldItem("hShift", "Desplazar", "in", string.Empty, "Mueve las elegidas a la vez (+6 o −6); hoy «Desplazar elevación».",
                    text =>
                    {
                        if (!TryNumber(text, out var value) || Math.Abs(value) < 1e-9) return "Escribe un desplazamiento distinto de cero, por ejemplo +6 o −6.";
                        return Apply(() => _editor.ShiftHorizontals(SelectedHorizontals, value));
                    }, readOnly));
            }

            list.Add(Choice("hProf", "Perfil", Options(_editor.Catalog.TrussProfiles, chosen.Select(h => h.ProfileId).ToArray()), chosen.Select(h => h.ProfileId), string.Empty,
                value => Apply(() => _editor.SetHorizontalProfile(SelectedHorizontals, value)), readOnly));
            var quantities = chosen.Select(h => h.Quantity.ToString(CultureInfo.InvariantCulture)).Distinct().ToList();
            list.Add(new DynamicFieldItem("hQty", "Cantidad", string.Empty, quantities.Count == 1 ? quantities[0] : string.Empty,
                quantities.Count == 1 ? "2 = horizontal doble: otro travesaño un troquel más arriba." : Varies + "; escribir aquí lo iguala.",
                text =>
                {
                    if (!int.TryParse((text ?? string.Empty).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) || value < 1) return "La cantidad debe ser un entero de 1 o más.";
                    return Apply(() => _editor.SetHorizontalQuantity(SelectedHorizontals, value));
                }, readOnly));
            var notes = chosen.Select(h => h.Notes ?? string.Empty).Distinct().ToList();
            list.Add(new DynamicFieldItem("hNotes", "Notas", string.Empty, notes.Count == 1 ? notes[0] : string.Empty,
                notes.Count == 1 ? "Solo informativas: no cambian el dibujo ni el BOM." : Varies + "; escribir aquí lo iguala.",
                text => Apply(() => _editor.SetHorizontalNotes(SelectedHorizontals, text)), readOnly));
            return list;
        }

        private IReadOnlyList<CabeceraSideColumn> BuildSideColumns()
        {
            if (_sidesTogether) return new[] { SideColumn(new[] { PostSide.Left, PostSide.Right }, "Los dos postes", "both") };
            return new[] { SideColumn(new[] { PostSide.Left }, "Poste izquierdo", "left"), SideColumn(new[] { PostSide.Right }, "Poste derecho", "right") };
        }

        private CabeceraSideColumn SideColumn(PostSide[] sides, string title, string key)
        {
            var readOnly = !CanEdit;
            var posts = sides.Select(_editor.Side).Where(p => p != null).ToList();
            var plates = sides.Select(_editor.Plate).Where(p => p != null).ToList();
            var named = sides.Length == 1 ? (sides[0] == PostSide.Left ? "Izquierdo: " : "Derecho: ") : string.Empty;
            var anyReinforced = posts.Any(p => p.HasReinforcement);
            var allReinforced = posts.Count > 0 && posts.All(p => p.HasReinforcement);
            var fields = new List<object>
            {
                Choice(key + "Post", named + "Perfil", Options(_editor.Catalog.PostProfiles, posts.Select(p => p.PostCatalogId).ToArray()), posts.Select(p => p.PostCatalogId), string.Empty,
                    value => Apply(() => _editor.SetSidePost(sides, value)), readOnly),
                Text(key + "Desc", named + "Descripción", posts.Select(p => p.Description ?? string.Empty), string.Empty, text => Apply(() => _editor.SetSideDescription(sides, text)), readOnly),
                new CabeceraCheckItem(key + "Ref", named + "Poste reforzado" + (anyReinforced && !allReinforced ? " (varía; marcar lo pone en los dos)" : string.Empty),
                    anyReinforced && !allReinforced ? (bool?)null : allReinforced, value => Apply(() => _editor.SetSideReinforced(sides, value)), readOnly),
                Choice(key + "RefT", named + "Tipo de refuerzo", Options(_editor.Catalog.PostProfiles, posts.Select(p => p.ReinforcementCatalogId).ToArray()), posts.Select(p => p.ReinforcementCatalogId),
                    "Un refuerzo es un poste.", value => Apply(() => _editor.SetSideReinforcementType(sides, value)), readOnly || !anyReinforced),
                Number(key + "RefH", named + "Altura del refuerzo", "in", posts.Select(p => CabeceraDraftEditor.Num(p.ReinforcementHeight)),
                    "Desde la base; la celosía de esa zona se ancla al troquel del refuerzo.", text =>
                    {
                        if (!TryNumber(text, out var value) || !(value >= 0.0)) return "La altura del refuerzo debe ser un número de 0 o más.";
                        return Apply(() => _editor.SetSideReinforcementHeight(sides, value));
                    }, readOnly || !anyReinforced),
                Choice(key + "Plate", named + "Placa", Options(_editor.Catalog.BasePlates, plates.Select(p => p.PlateCatalogId).ToArray()), plates.Select(p => p.PlateCatalogId), string.Empty,
                    value => Apply(() => _editor.SetSidePlate(sides, value)), readOnly),
                Number(key + "PlPer", named + "Peralte de la placa", "in", plates.Select(p => p.PeralteOverride.HasValue ? CabeceraDraftEditor.Num(p.PeralteOverride.Value) : string.Empty),
                    "Vacío = el del poste.", text =>
                    {
                        if (string.IsNullOrWhiteSpace(text)) return Apply(() => _editor.SetSidePlatePeralte(sides, null));
                        if (!TryNumber(text, out var value) || !(value > 0.0)) return "El peralte de la placa debe ser un número mayor que cero (vacío = el del poste).";
                        return Apply(() => _editor.SetSidePlatePeralte(sides, value));
                    }, readOnly),
                Choice(key + "PlPt", named + "Punto de montaje", Points("BasePlate", plates.Select(p => p.ConnectionPointId).ToArray()), plates.Select(p => p.ConnectionPointId),
                    "Solo los puntos de placa.", value => Apply(() => _editor.SetSidePlatePoint(sides, value)), readOnly),
            };
            return new CabeceraSideColumn(title, fields);
        }

        private IReadOnlyList<object> BuildCelosiaFields()
        {
            var readOnly = !CanEdit;
            var c = _editor.Configuration;
            return new object[]
            {
                Int("celStart", "Inicio de la celosía (troquel)", c.CelosiaStartTroquel, 1, "La primera horizontal va en el troquel n: (n − 1) × 2 in.", v => _editor.SetCelosiaStart(v), readOnly),
                Int("celLow", "Separación de la diagonal abajo (troqueles)", c.DiagonalStartOffsetTroqueles, 0, "Sobre la horizontal de abajo de cada panel.", v => _editor.SetDiagonalStartOffset(v), readOnly),
                Int("celHigh", "Separación de la diagonal arriba (troqueles)", c.DiagonalEndOffsetTroqueles, 0, "Bajo la horizontal de arriba.", v => _editor.SetDiagonalEndOffset(v), readOnly),
            };
        }

        private IReadOnlyList<SelectiveCalcItem> BuildFixed()
        {
            var c = _editor.Configuration;
            return new[]
            {
                new SelectiveCalcItem("Paso de troquel", CabeceraDraftEditor.Inches(c.PasoTroquel), "Los troqueles del poste."),
                new SelectiveCalcItem("Claro estándar", CabeceraDraftEditor.Inches(c.PanelClear), "El del modelo; la plantilla genera siempre con 44 in."),
                new SelectiveCalcItem("Remate del poste", CabeceraDraftEditor.Inches(RackFrameConfigurationFactory.PostTopRemate), "Sobre la última horizontal."),
                new SelectiveCalcItem("Diagonal doble", Count(c.DiagonalDoubleSpacingTroqueles, "troquel", "troqueles"), "Entre las dos diagonales."),
                new SelectiveCalcItem("Horizontal doble", Count(c.HorizontalDoubleOffsetTroqueles, "troquel", "troqueles"), "Entre los dos travesaños."),
            };
        }

        // ------------------------------------------------------------------ the writes

        private string Apply(Func<CabeceraEdit> write)
        {
            if (!CanEdit) return _writeTaken ? TakenWriteText : WaitingWriteText;
            CabeceraEdit result;
            try
            {
                result = write();
            }
            catch (InvalidOperationException ex)
            {
                result = CabeceraEdit.Refuse(ex.Message);
            }
            catch (ArgumentException ex)
            {
                result = CabeceraEdit.Refuse(ex.Message);
            }

            if (!result.Applied) return result.Error;
            if (!string.IsNullOrEmpty(result.Notice)) Say(result.Notice, false);
            else if (_messageIsError) Say(string.Empty, false);
            Rebuild();
            return null;
        }

        // CB-02: the template and the height regenerate; with own changes the window asks inline first and lists what is lost.
        private string Regenerate(Func<CabeceraEdit> regenerate, string what)
        {
            if (!CanEdit) return _writeTaken ? TakenWriteText : WaitingWriteText;
            var losses = _editor.RegenerationLosses();
            if (losses.Count == 0)
            {
                var result = regenerate();
                if (!result.Applied) return result.Error;
                CleanSelection(resetToFirst: true);
                Say(result.Notice, false);
                Rebuild();
                return null;
            }

            _pendingRegen = regenerate;
            _regenText = what + " vuelve a calcular horizontales, paneles, postes y placas desde la plantilla. Se perderán estos cambios:";
            _regenLosses = losses.Select(c => c.Line).ToList();
            OnPropertyChanged(nameof(RegenText));
            OnPropertyChanged(nameof(RegenLosses));
            IsRegenOpen = true;
            return null;
        }

        private void Structural(Func<CabeceraEdit> write, bool allowed, Func<IEnumerable<(bool, int)>> select)
        {
            if (!allowed) return;
            var next = select?.Invoke()?.ToList();
            var result = write();
            if (!result.Applied)
            {
                Say(result.Error, true);
                Rebuild();
                return;
            }

            if (next != null && next.Count > 0)
            {
                _selection.Clear();
                _selection.AddRange(next);
                _primary = next[0];
            }

            Say(result.Notice, false);
            Rebuild();
        }

        private void CleanSelection(bool resetToFirst = false)
        {
            _selection.RemoveAll(s => s.Panel ? s.Index >= _editor.PanelCount : s.Index >= _editor.HorizontalCount);
            if (resetToFirst || _selection.Count == 0)
            {
                _selection.Clear();
                _primary = (_editor.PanelCount > 0, 0);
                _selection.Add(_primary);
            }

            if (!_selection.Contains(_primary)) _primary = _selection[0];
        }

        private void CloseInlinePanels(string except)
        {
            if (except != nameof(IsConfirmingClose)) IsConfirmingClose = false;
            if (except != nameof(IsConfirmingBack)) IsConfirmingBack = false;
            if (except != nameof(IsConfirmingHeight)) IsConfirmingHeight = false;
            if (except != nameof(IsRestoreOpen)) IsRestoreOpen = false;
            if (except != nameof(IsCopyOpen)) IsCopyOpen = false;
            if (except != nameof(IsRegenOpen) && _isRegenOpen)
            {
                _pendingRegen = null;
                IsRegenOpen = false;
            }
        }

        private double PanelClear(int index)
        {
            var panel = index >= 0 && index < _editor.PanelCount ? _editor.Panels[index] : null;
            return panel == null ? 0.0 : panel.EndElevation - panel.StartElevation;
        }

        // ------------------------------------------------------------------ helpers of the fields

        private SelectiveChoiceItem Choice(string key, string label, IReadOnlyList<SelectiveChoiceOption> options, IEnumerable<string> values, string hint,
            Func<string, string> commit, bool readOnly)
        {
            var distinct = values.Select(v => v ?? string.Empty).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var selected = distinct.Count == 1 ? options.FirstOrDefault(o => string.Equals(o.Value, distinct[0], StringComparison.OrdinalIgnoreCase))?.Value ?? distinct[0] : null;
            return new SelectiveChoiceItem(key, label, options, selected, distinct.Count > 1 ? Varies + "; elegir aquí lo iguala." : hint, commit, readOnly);
        }

        private static DynamicFieldItem Text(string key, string label, IEnumerable<string> values, string hint, Func<string, string> commit, bool readOnly)
        {
            var distinct = values.Distinct(StringComparer.Ordinal).ToList();
            return new DynamicFieldItem(key, label, string.Empty, distinct.Count == 1 ? distinct[0] : string.Empty, distinct.Count > 1 ? Varies + "; escribir aquí lo iguala." : hint, commit, readOnly);
        }

        private static DynamicFieldItem Number(string key, string label, string unit, IEnumerable<string> values, string hint, Func<string, string> commit, bool readOnly)
        {
            var distinct = values.Distinct(StringComparer.Ordinal).ToList();
            return new DynamicFieldItem(key, label, unit, distinct.Count == 1 ? distinct[0] : string.Empty, distinct.Count > 1 ? Varies + "; escribir aquí lo iguala." : hint, commit, readOnly);
        }

        private DynamicFieldItem Int(string key, string label, int value, int minimum, string hint, Func<int, CabeceraEdit> write, bool readOnly)
            => new DynamicFieldItem(key, label, string.Empty, value.ToString(CultureInfo.InvariantCulture), hint, text =>
            {
                if (!int.TryParse((text ?? string.Empty).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) || number < minimum)
                    return minimum == 0 ? "Debe ser un entero de 0 o más." : "Debe ser un entero de 1 o más.";
                return Apply(() => write(number));
            }, readOnly);

        private IReadOnlyList<SelectiveChoiceOption> Options<T>(IReadOnlyList<T> entries, params string[] current) where T : CatalogEntryBase
        {
            var list = (entries ?? Array.Empty<T>()).Where(e => !string.IsNullOrWhiteSpace(e.Id)).Select(e => new SelectiveChoiceOption(e.Id, e.Label)).ToList();
            foreach (var id in current ?? Array.Empty<string>())
            {
                if (!string.IsNullOrWhiteSpace(id) && !list.Any(o => string.Equals(o.Value, id, StringComparison.OrdinalIgnoreCase))) list.Add(new SelectiveChoiceOption(id, id));
            }

            return list;
        }

        // CB-12: the connection points of one role (Poste, Truss or BasePlate), and the one in use even if the catalog gives it another role.
        private IReadOnlyList<SelectiveChoiceOption> Points(string role, params string[] current)
        {
            var list = _editor.Catalog.ConnectionPoints
                .Where(p => !string.IsNullOrWhiteSpace(p.Id) && string.Equals(p.Role, role, StringComparison.OrdinalIgnoreCase))
                .GroupBy(p => p.Id, StringComparer.OrdinalIgnoreCase).Select(g => g.First())
                .Select(p => new SelectiveChoiceOption(p.Id, p.Label)).ToList();
            foreach (var id in current ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(id) || list.Any(o => string.Equals(o.Value, id, StringComparison.OrdinalIgnoreCase))) continue;
                list.Add(new SelectiveChoiceOption(id, _editor.LabelOf(id)));
            }

            return list;
        }

        private static string TemplateLabel(RackFrameTemplate template)
        {
            if (template == null) return string.Empty;
            var name = (template.Name ?? template.Id ?? string.Empty).Replace("Estandar", "Estándar");
            return template.Id != null && template.Id.StartsWith("USER-", StringComparison.OrdinalIgnoreCase) ? name + " (propia)" : name;
        }

        private void MergeList(ref IReadOnlyList<object> current, IReadOnlyList<object> next, string propertyName)
        {
            if (RefreshInPlace(current, next)) return;
            current = next;
            OnPropertyChanged(propertyName);
        }

        private void MergeColumns(IReadOnlyList<CabeceraSideColumn> next)
        {
            if (_sideColumns.Count == next.Count && _sideColumns.Zip(next, (a, b) => a.Title == b.Title).All(x => x))
            {
                for (var i = 0; i < next.Count; i++) _sideColumns[i].Refresh(next[i]);
                return;
            }

            _sideColumns = next;
            OnPropertyChanged(nameof(SideColumns));
        }

        /// <summary>The items of a list live as long as the editor (D-06 of Dinámico): the same keys refresh in place.</summary>
        internal static bool RefreshInPlace(IReadOnlyList<object> current, IReadOnlyList<object> next)
        {
            if (current == null || next == null || current.Count != next.Count || current.Count == 0) return false;
            for (var i = 0; i < current.Count; i++)
            {
                if (current[i]?.GetType() != next[i]?.GetType() || KeyOf(current[i]) != KeyOf(next[i])) return false;
            }

            for (var i = 0; i < current.Count; i++)
            {
                switch (current[i])
                {
                    case DynamicFieldItem field: field.Refresh((DynamicFieldItem)next[i]); break;
                    case SelectiveChoiceItem choice: choice.Refresh((SelectiveChoiceItem)next[i]); break;
                    case CabeceraCheckItem check: check.Refresh((CabeceraCheckItem)next[i]); break;
                }
            }

            return true;
        }

        private static string KeyOf(object item) => item switch
        {
            DynamicFieldItem field => field.Key,
            SelectiveChoiceItem choice => choice.Key,
            CabeceraCheckItem check => check.Key,
            _ => null,
        };

        private static bool TryNumber(string text, out double value) => UiSupport.TryNum((text ?? string.Empty).Replace('−', '-').Trim(), out value);

        private static string Count(int count, string one, string many) => count.ToString(CultureInfo.InvariantCulture) + " " + (count == 1 ? one : many);

        private static string Lower(string text) => string.IsNullOrEmpty(text) ? text : char.ToLowerInvariant(text[0]) + text.Substring(1);

        private static Geometry Frozen(string data)
        {
            var geometry = Geometry.Parse(data);
            geometry.Freeze();
            return geometry;
        }
    }
}
