using System;
using System.Collections.Generic;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Shared;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T7 cab (I-64, séptima tanda; the Owner on 2026-11-27: «Las dos»): what the editor of a cabecera of its own gains to be a draft of
    /// «Nuevo» and to insert views, as the editors of the other systems.
    /// <list type="bullet">
    /// <item><b>A new cabecera</b> (<see cref="ForNewRack"/>): a draft («Borrador N», SD-06) that is not in the drawing; «Actualizar» is
    /// not offered, «Insertar en el dibujo…» places it (RACKCABECERANUEVO) and <see cref="MarkInserted"/> makes it a cabecera of the
    /// drawing.</item>
    /// <item><b>«Insertar vista…»</b> of a cabecera of the drawing (RACKCABECERAINSERTAR): the reusable panel
    /// <see cref="RackViewInsertPanelViewModel"/> (DG-07), with the views the classic configurator offers (lateral and planta), and the
    /// same rules as Push Back and Dinámico: it opens also when the views cannot be inserted now and says why; «El dibujo cambió» stops it
    /// (T6st-S5).</item>
    /// <item><b>«Propiedades»</b> (VA-07; VA-S9 for a new one): the tab of the other editors, the fifth one. Its rows are part of the
    /// draft; a wrong one stops «Actualizar» and, in a new cabecera, «Insertar en el dibujo…».</item>
    /// </list>
    /// A cabecera inside the editor of its rack (embedded) has none of this: it only writes the draft of that rack.
    /// </summary>
    public sealed partial class CabeceraEditorViewModel
    {
        /// <summary>The tabs: 0 Cabecera, 1 Alzado (the first one), 2 Postes y placas, 3 Avanzado, 4 Propiedades (T7 cab).</summary>
        public const int PropertiesTabIndex = 4;

        private bool _isNewRack;
        private bool _isCopy;

        /// <summary>A new cabecera of «Nuevo» (T7 cab): «Insertar en el dibujo…» hands the editor to the window, which inserts it.</summary>
        public static CabeceraEditorViewModel ForNewRack(CabeceraDraftEditor editor, string rackId, Action<CabeceraEditorViewModel> update)
        {
            var viewModel = new CabeceraEditorViewModel(editor, null, update, false, rackId) { _isNewRack = true };
            viewModel.Rebuild();
            return viewModel;
        }

        /// <summary>A new cabecera of the window: not in the drawing until «Insertar en el dibujo…» places it.</summary>
        public bool IsNewRack => _isNewRack;

        /// <summary>
        /// AutoCAD placed the new cabecera: from now on it is a cabecera of the drawing. The dispatch of «Nuevo» named it (I-60: «Cabecera N»
        /// when it had no name, or the name of a template); the editor takes the name the drawing gave it, and what the drawing holds is the
        /// cabecera sent with that name.
        /// </summary>
        public void MarkInserted(string name = null)
        {
            if (!_isNewRack) return;
            _isNewRack = false;
            if (!string.IsNullOrWhiteSpace(name) && !string.Equals(name.Trim(), Name.Trim(), StringComparison.Ordinal))
            {
                _editor.SetName(name.Trim());
                _editor.MarkWritten(); // the editor was locked while AutoCAD wrote it: the frame is the one that was sent
            }

            Rebuild();
        }

        // ------------------------------------------------------------------ «Duplicar» (the rule of the other drafts)

        /// <summary>The copy of «Duplicar» takes the frame of its original as an edit over the base of its original: «Modificado» exactly when it differs.</summary>
        internal bool TakeDesignAsEdit(RackFrameConfiguration design)
        {
            if (design == null || !CanEdit || IsEmbedded) return false;
            _editor.TakeAsEdit(design);
            _isCopy = true; // T6 ui: its «Descartar» goes back to how its original opened, and its text says so
            CleanSelection(resetToFirst: true);
            Rebuild();
            return true;
        }

        /// <summary>What the window says first in the editor of a draft opened from a design (a copy of «Duplicar»).</summary>
        internal void SayOpened(string text) => Say(text, false);

        // ------------------------------------------------------------------ «Propiedades» (VA-07; VA-S9)

        private RackPropertiesTabViewModel _propertiesTab;

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

        /// <summary>The properties read with the cabecera by the door (RACKCABECERAABRIR).</summary>
        public void LoadProperties(RackPropertiesRead read)
        {
            if (IsEmbedded) return;
            PropertiesTab.Load(read);
            RaiseState();
        }

        /// <summary>What «Actualizar» (and «Insertar en el dibujo…» of a new cabecera) writes in its properties, or null when the tab writes nothing.</summary>
        public RackPropertiesWrite PropertiesWrite() => _propertiesTab?.BuildWrite();

        /// <summary>The answer of a write: the tab goes on from what the drawing holds now (or keeps the rows that were not written, pr).</summary>
        public void PropertiesWritten(RackDesignWritten written)
        {
            _propertiesTab?.Answered(written);
            RaiseState();
        }

        private void OnPropertiesTabChanged() => RaiseState();

        // ------------------------------------------------------------------ «Insertar vista…» / «Insertar en el dibujo…» (DG-07)

        private RackViewInsertPanelViewModel _insertPanel;
        private RackViewInsertPanelViewModel _suspendedInsertPanel;
        private Action<CabeceraEditorViewModel, IReadOnlyList<RackViewAddress>> _insertViews;
        private Action<CabeceraEditorViewModel, IReadOnlyList<RackViewAddress>> _insertNew;
        private Func<IReadOnlyList<string>> _existingViewKinds;
        private Func<string> _otherRequestText;
        private Func<string> _otherDrawingText;

        /// <summary>What «Insertar vista…» does with the views chosen: the window first writes the cabecera in the views that exist, then inserts them.</summary>
        public Action<CabeceraEditorViewModel, IReadOnlyList<RackViewAddress>> InsertViewsAction
        {
            get => _insertViews;
            set { _insertViews = value; RaiseState(); }
        }

        /// <summary>What «Insertar en el dibujo…» of a new cabecera does with the views chosen: the window inserts it with them.</summary>
        public Action<CabeceraEditorViewModel, IReadOnlyList<RackViewAddress>> InsertNewAction
        {
            get => _insertNew;
            set { _insertNew = value; RaiseState(); }
        }

        /// <summary>The kinds of view the drawing already has for this cabecera (the last reading of the list, DG-04); the window sets it.</summary>
        public Func<IReadOnlyList<string>> ExistingViewKinds
        {
            get => _existingViewKinds;
            set { _existingViewKinds = value; RefreshInsertPanel(); }
        }

        /// <summary>Why the door cannot take a write now (another request waits for AutoCAD), or empty; the window sets it.</summary>
        public Func<string> OtherRequestText
        {
            get => _otherRequestText;
            set { _otherRequestText = value; RaiseState(); }
        }

        /// <summary>Why the door cannot write this draft now because it is of another drawing, or empty; the window sets it.</summary>
        public Func<string> OtherDrawingText
        {
            get => _otherDrawingText;
            set { _otherDrawingText = value; RaiseState(); }
        }

        /// <summary>The button of the head (a cabecera of its own only).</summary>
        public string InsertViewLabel => _isNewRack ? "Insertar en el dibujo…" : "Insertar vista…";

        public bool CanInsertView => string.IsNullOrEmpty(InsertViewBlockedText);

        /// <summary>Why the views cannot be inserted now; empty when they can (the panel says it next to «Insertar N vistas»).</summary>
        public string InsertViewBlockedText
        {
            get
            {
                var refusal = InsertPanelBlockedText;
                if (!string.IsNullOrEmpty(refusal)) return refusal;
                var drawing = _otherDrawingText?.Invoke() ?? string.Empty;
                if (!string.IsNullOrEmpty(drawing)) return drawing;
                var other = _otherRequestText?.Invoke() ?? string.Empty;
                if (!string.IsNullOrEmpty(other)) return other;
                if (_drawingChange?.IsChanged == true) return _drawingChange.InsertBlockedText; // T6 st: it would write the cabecera over the drawing
                if (_isNewRack && PropertiesBlockUpdate) return PropertiesTab.BlockedText; // VA-S9: «Insertar en el dibujo…» writes them
                return string.Empty;
            }
        }

        public RackViewInsertPanelViewModel InsertPanel => _insertPanel;

        public bool IsInsertPanelOpen => _insertPanel != null;

        public string InsertPanelStatus => IsInsertPanelOpen ? "panel abierto" : string.Empty;

        public bool CanOpenInsertPanel => string.IsNullOrEmpty(InsertPanelBlockedText);

        /// <summary>Why the panel does not open: no door, an embedded cabecera, or AutoCAD writes this cabecera (DG-05: otherwise it opens and says why).</summary>
        public string InsertPanelBlockedText
        {
            get
            {
                var action = _isNewRack ? _insertNew : _insertViews;
                if (IsEmbedded || action == null || string.IsNullOrWhiteSpace(RackId))
                    return _isNewRack ? "Aquí no se puede insertar la cabecera nueva." : "Aquí no se pueden insertar vistas de la cabecera.";
                return _writing ? (_writeTaken ? TakenWriteText : WaitingWriteText) : string.Empty;
            }
        }

        /// <summary>«Insertar vista…» / «Insertar en el dibujo…»: opens the panel under the head of the editor (it writes only with «Insertar N vistas»).</summary>
        public void InsertView()
        {
            if (!CanOpenInsertPanel)
            {
                Say(InsertPanelBlockedText, true);
                return;
            }

            if (_insertPanel != null) return;
            IsConfirmingClose = false;
            _insertPanel = new RackViewInsertPanelViewModel(RackSystemKind.Selective, InsertOptions(), InsertPanelContext(), InsertChosenViews, CloseInsertPanel);
            RaiseInsertPanel();
        }

        /// <summary>The views of a cabecera, as the classic configurator offers them («Insertar en AutoCAD», «Insertar planta», «Insertar varias…»).</summary>
        public static IReadOnlyList<RackViewInsertOption> InsertOptions() => new[]
        {
            new RackViewInsertOption("Lateral", RackViewAddress.Whole(DimensionViewKind.Lateral)),
            new RackViewInsertOption("Planta", RackViewAddress.Whole(DimensionViewKind.Planta)),
        };

        /// <summary>«Cancelar» of the panel: it closes and nothing is inserted.</summary>
        public void CloseInsertPanel()
        {
            _suspendedInsertPanel = null;
            if (_insertPanel == null) return;
            _insertPanel = null;
            RaiseInsertPanel();
        }

        /// <summary>The window handed the write to AutoCAD: the panel hidden while writing does not come back.</summary>
        public void WriteHandedOver() => _suspendedInsertPanel = null;

        /// <summary>The window: the door changed (another request waits for AutoCAD, or not).</summary>
        public void RefreshInsertState() => RaiseNewRack();

        // The panel follows the editor: hidden while AutoCAD writes, back if the door refused; its reason and the state of the draft.
        private void RefreshInsertPanel()
        {
            if (_writing)
            {
                if (_insertPanel == null) return;
                _suspendedInsertPanel = _insertPanel;
                _insertPanel = null;
                RaiseInsertPanel();
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

        private void InsertChosenViews(IReadOnlyList<RackViewAddress> views)
        {
            if (views == null || views.Count == 0) return;
            if (!CanInsertView)
            {
                Say(InsertViewBlockedText, true);
                return;
            }

            (_isNewRack ? _insertNew : _insertViews)(this, views);
        }

        private RackViewInsertPanelContext InsertPanelContext() => new RackViewInsertPanelContext(
            Name,
            _isNewRack,
            "Cabecera",
            IsDirty,
            InsertViewBlockedText,
            _existingViewKinds?.Invoke(),
            new RackViewInsertGroupTexts(
                string.Empty,
                "La cabecera vista de lado: los dos postes, las horizontales y las diagonales.",
                "La cabecera vista desde arriba: los dos postes y sus placas, enlazada a la misma cabecera."));

        private void RaiseInsertPanel()
        {
            OnPropertyChanged(nameof(InsertPanel));
            OnPropertyChanged(nameof(IsInsertPanelOpen));
            OnPropertyChanged(nameof(InsertPanelStatus));
            OnPropertyChanged(nameof(CanOpenInsertPanel));
            OnPropertyChanged(nameof(InsertPanelBlockedText));
        }

        // One line of RaiseState: what this part shows follows the editor.
        private void RaiseNewRack()
        {
            OnPropertyChanged(nameof(IsNewRack));
            OnPropertyChanged(nameof(InsertViewLabel));
            OnPropertyChanged(nameof(CanInsertView));
            OnPropertyChanged(nameof(InsertViewBlockedText));
            OnPropertyChanged(nameof(CanOpenInsertPanel));
            OnPropertyChanged(nameof(InsertPanelBlockedText));
            RefreshInsertPanel();
        }
    }
}
