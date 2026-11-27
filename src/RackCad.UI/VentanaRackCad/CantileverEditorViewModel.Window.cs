using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Shared;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// What the window gives the Cantilever editor (T5 ct): «Insertar vista…» and «Insertar en el dibujo…» with the reusable panel of the views
    /// (<see cref="RackViewInsertPanelViewModel"/>, DG-07: Frontal · Lateral por estación · Planta), the request of the door that waits, the
    /// draft of another drawing, «Más» › «Guardar en biblioteca…» (UT-04) and the tab «Propiedades» (VA-07). The same rules as the editors of
    /// Selectivo and Dinámico: the panel follows the draft, is hidden while AutoCAD writes and comes back if the door refuses the write.
    /// </summary>
    public sealed partial class CantileverEditorViewModel
    {
        public const int PropertiesTabIndex = 4;

        private Action<CantileverEditorViewModel, IReadOnlyList<RackViewAddress>> _insertViews;
        private Action<CantileverEditorViewModel, IReadOnlyList<RackViewAddress>> _insertNew;
        private RackViewInsertPanelViewModel _insertPanel;
        private RackViewInsertPanelViewModel _suspendedInsertPanel;
        private Func<IReadOnlyList<string>> _existingViewKinds;
        private Func<string> _otherRequestText;
        private Func<string> _otherDrawingText;
        private bool _moreOpen;
        private Action<CantileverEditorViewModel> _saveToLibrary;
        private RackPropertiesTabViewModel _propertiesTab;

        /// <summary>«Insertar vista…» of a line of the drawing (RACKCANTILEVERINSERTAR); the window sets it.</summary>
        public Action<CantileverEditorViewModel, IReadOnlyList<RackViewAddress>> InsertViewsAction
        {
            get => _insertViews;
            set { _insertViews = value; RaiseState(); }
        }

        /// <summary>«Insertar en el dibujo…» of a new line (RACKCANTILEVERNUEVO); the window sets it.</summary>
        public Action<CantileverEditorViewModel, IReadOnlyList<RackViewAddress>> InsertNewAction
        {
            get => _insertNew;
            set { _insertNew = value; RaiseState(); }
        }

        public Func<IReadOnlyList<string>> ExistingViewKinds
        {
            get => _existingViewKinds;
            set { _existingViewKinds = value; RefreshInsertPanel(); }
        }

        public Func<string> OtherRequestText
        {
            get => _otherRequestText;
            set { _otherRequestText = value; RaiseState(); }
        }

        public Func<string> OtherDrawingText
        {
            get => _otherDrawingText;
            set { _otherDrawingText = value; RaiseState(); }
        }

        private string OtherRequestBlockedText => _otherRequestText?.Invoke() ?? string.Empty;

        private string OtherDrawingBlockedText => _otherDrawingText?.Invoke() ?? string.Empty;

        // ------------------------------------------------------------------ «Insertar vista…» (DG-01 to DG-08)

        public bool ShowInsertView => !_isNewRack;

        public RackViewInsertPanelViewModel InsertPanel => _insertPanel;

        public bool IsInsertPanelOpen => _insertPanel != null;

        public string InsertPanelStatus => IsInsertPanelOpen ? "panel abierto" : string.Empty;

        public bool CanOpenInsertPanel => string.IsNullOrEmpty(InsertPanelBlockedText);

        public string InsertPanelBlockedText
        {
            get
            {
                var action = _isNewRack ? _insertNew : _insertViews;
                if (action == null || string.IsNullOrWhiteSpace(RackId))
                    return _isNewRack ? "Aquí no se puede insertar la línea nueva." : "Aquí no se pueden insertar vistas de la línea.";
                return _writing ? WritingText : string.Empty;
            }
        }

        /// <summary>Why «Insertar N vistas» cannot write now (DG-05), or empty: the reasons of the panel, as for Selectivo and Dinámico.</summary>
        public string InsertViewBlockedText
        {
            get
            {
                if (_writing) return WritingText;
                var other = OtherRequestBlockedText;
                if (other.Length > 0) return other;
                var drawing = OtherDrawingBlockedText;
                if (drawing.Length > 0) return drawing;
                if (SegmentsPending) return "Aplica o descarta antes la secuencia.";
                if (!_draft.IsResolved) return "La línea no se resuelve: completa antes lo que falta.";
                if (_isNewRack && PropertiesBlockUpdate) return PropertiesTab.BlockedText; // T6 pr: «Insertar en el dibujo…» writes them (VA-S9)
                return string.Empty;
            }
        }

        public bool CanInsertView => string.IsNullOrEmpty(InsertViewBlockedText);

        /// <summary>«Insertar vista…» (or «Insertar en el dibujo…» of a new line): the panel opens under the head, also when it cannot insert now.</summary>
        public void InsertView()
        {
            if (!CanOpenInsertPanel)
            {
                Say(InsertPanelBlockedText, error: true);
                return;
            }

            if (_insertPanel != null) return;
            SetConfirmingClose(false);
            _insertPanel = new RackViewInsertPanelViewModel(RackSystemKind.Cantilever, InsertOptions(), InsertPanelContext(), InsertChosenViews, CloseInsertPanel);
            RaiseInsertPanel();
        }

        /// <summary>The views of the line: Frontal, Planta and one Lateral per station (as «Insertar varias…» of the classic editor).</summary>
        public IReadOnlyList<RackViewInsertOption> InsertOptions()
        {
            var options = new List<RackViewInsertOption>
            {
                new("Frontal", RackViewAddress.Whole(DimensionViewKind.Frontal)),
                new("Planta", RackViewAddress.Whole(DimensionViewKind.Planta)),
            };
            var stations = _draft.IsResolved ? _draft.Computation.Line.Stations.Count : _draft.Design.StationCount;
            for (var i = 0; i < stations; i++) options.Add(new RackViewInsertOption("Lateral · estación " + (i + 1), RackViewAddress.Station(i)));
            return options;
        }

        public void CloseInsertPanel()
        {
            _suspendedInsertPanel = null;
            if (_insertPanel == null) return;
            _insertPanel = null;
            RaiseInsertPanel();
        }

        /// <summary>The window handed the write to AutoCAD: the panel hidden while writing does not come back.</summary>
        public void WriteHandedOver() => _suspendedInsertPanel = null;

        public void RefreshInsertPanel()
        {
            if (_writing)
            {
                SuspendInsertPanel();
                return;
            }

            if (_insertPanel == null && _suspendedInsertPanel != null)
            {
                _insertPanel = _suspendedInsertPanel;
                _suspendedInsertPanel = null;
                RaiseInsertPanel();
            }

            _insertPanel?.Follow(InsertOptions(), InsertPanelContext());
        }

        /// <summary>The window: the door changed (another request waits for AutoCAD, or not); the button and the open panel say it.</summary>
        public void RefreshInsertState()
        {
            OnPropertyChanged(nameof(CanInsertView));
            OnPropertyChanged(nameof(InsertViewBlockedText));
            OnPropertyChanged(nameof(CanUpdate));
            OnPropertyChanged(nameof(UpdateBlockedText));
            if (_writing)
            {
                SuspendInsertPanel();
                return;
            }

            _insertPanel?.Update(InsertPanelContext());
        }

        private void SuspendInsertPanel()
        {
            if (_insertPanel == null) return;
            _suspendedInsertPanel = _insertPanel;
            _insertPanel = null;
            RaiseInsertPanel();
        }

        private void InsertChosenViews(IReadOnlyList<RackViewAddress> views)
        {
            if (views == null || views.Count == 0) return;
            if (!CanInsertView)
            {
                Say(InsertViewBlockedText, error: true);
                return;
            }

            (_isNewRack ? _insertNew : _insertViews)?.Invoke(this, views);
        }

        private RackViewInsertPanelContext InsertPanelContext() => new RackViewInsertPanelContext(
            Name,
            _isNewRack,
            "Cantilever",
            _dirty,
            InsertViewBlockedText,
            _existingViewKinds?.Invoke(),
            new RackViewInsertGroupTexts(
                "Toda la línea, mirando en +Y.",
                "Una estación, mirando en −X: columna, base y brazos. El arriostramiento vive entre estaciones, así que no aparece aquí.",
                "Toda la línea desde arriba (brazos y tensores según «Línea» › «Dibujo»)."));

        private void RaiseInsertPanel()
        {
            OnPropertyChanged(nameof(InsertPanel));
            OnPropertyChanged(nameof(IsInsertPanelOpen));
            OnPropertyChanged(nameof(InsertPanelStatus));
            OnPropertyChanged(nameof(CanOpenInsertPanel));
            OnPropertyChanged(nameof(InsertPanelBlockedText));
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

        public Action<CantileverEditorViewModel> SaveToLibraryAction
        {
            get => _saveToLibrary;
            set
            {
                _saveToLibrary = value;
                RaiseMore();
            }
        }

        public bool CanSaveToLibrary => string.IsNullOrEmpty(SaveToLibraryBlockedText);

        /// <summary>Why «Guardar en biblioteca…» cannot run now: the line must resolve, as «Guardar en biblioteca» of the classic editor.</summary>
        public string SaveToLibraryBlockedText
        {
            get
            {
                if (_saveToLibrary == null) return "Aquí no se puede guardar en la biblioteca.";
                if (SegmentsPending) return "Aplica o descarta antes la secuencia.";
                if (!_draft.IsResolved) return "Corrige los datos: no se puede guardar una línea que no se resolvió.";
                return string.Empty;
            }
        }

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

        /// <summary>Why the draft cannot be saved now, or null; the line of the library asks it again at «Guardar» and «Reemplazar».</summary>
        public string SaveRefusal()
        {
            var blocked = SaveToLibraryBlockedText;
            return string.IsNullOrEmpty(blocked) ? null : blocked;
        }

        private void RaiseMore()
        {
            OnPropertyChanged(nameof(IsMoreOpen));
            OnPropertyChanged(nameof(MoreItemStatus));
            OnPropertyChanged(nameof(CanSaveToLibrary));
            OnPropertyChanged(nameof(SaveToLibraryBlockedText));
        }

        // ------------------------------------------------------------------ the tab «Propiedades» (VA-07)

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

        public void LoadProperties(RackPropertiesRead read) => PropertiesTab.Load(read);

        public RackPropertiesWrite PropertiesWrite() => _propertiesTab?.BuildWrite();

        public void PropertiesWritten(RackDesignWritten written)
        {
            _propertiesTab?.Answered(written); // T6 pr: also the answer of «Insertar en el dibujo…» of a new line (VA-S9)
            Rebuild();
        }

        private void OnPropertiesTabChanged() => Rebuild();

        private void RaiseWindowState()
        {
            OnPropertyChanged(nameof(InsertViewBlockedText));
            OnPropertyChanged(nameof(CanInsertView));
            OnPropertyChanged(nameof(CanOpenInsertPanel));
            OnPropertyChanged(nameof(InsertPanelBlockedText));
            RaiseMore();
            if (_writing) SuspendInsertPanel();
            else RefreshInsertPanel();
        }
    }
}
