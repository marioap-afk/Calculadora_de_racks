using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Shared;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// «Insertar vista…» of the Push Back editor (T5 pb; DG-07 of the prototype «dialogos» and question 11 of «pushback»: one panel instead of
    /// «Insertar lateral», the two frontal buttons, «Planta», «Insertar varias…» and «Insertar vista actual», with A and B by their name): the
    /// reusable panel <see cref="RackViewInsertPanelViewModel"/>, as in Selectivo and Dinámico. It opens also when the views cannot be
    /// inserted now and says why; only while AutoCAD writes this rack is the button off. «Insertar N vistas» is the write of the window:
    /// RACKPUSHBACKINSERTAR (the draft in the views that exist, then the new ones), or RACKPUSHBACKNUEVO for a new Push Back. Nothing closes
    /// the window.
    /// </summary>
    public sealed partial class PushBackEditorViewModel
    {
        private RackViewInsertPanelViewModel _insertPanel;
        private RackViewInsertPanelViewModel _suspendedInsertPanel;
        private Action<PushBackEditorViewModel, IReadOnlyList<RackViewAddress>> _insertViews;
        private Action<PushBackEditorViewModel, IReadOnlyList<RackViewAddress>> _insertNew;
        private Func<IReadOnlyList<string>> _existingViewKinds;
        private Func<string> _otherRequestText;
        private Func<string> _otherDrawingText;

        /// <summary>What «Insertar vista…» does with the views chosen: the window first writes the draft in the views that exist, then inserts them.</summary>
        public Action<PushBackEditorViewModel, IReadOnlyList<RackViewAddress>> InsertViewsAction
        {
            get => _insertViews;
            set { _insertViews = value; RaiseState(); }
        }

        /// <summary>What «Insertar en el dibujo…» of a new rack does with the views chosen: the window inserts the rack with them.</summary>
        public Action<PushBackEditorViewModel, IReadOnlyList<RackViewAddress>> InsertNewAction
        {
            get => _insertNew;
            set { _insertNew = value; RaiseState(); }
        }

        public string InsertViewLabel => _isNewRack ? "Insertar en el dibujo…" : "Insertar vista…";

        public bool CanInsertView => string.IsNullOrEmpty(InsertViewBlockedText);

        /// <summary>Why the views cannot be inserted now; empty when they can (the panel says it next to «Insertar N vistas»).</summary>
        public string InsertViewBlockedText
        {
            get
            {
                var action = _isNewRack ? _insertNew : _insertViews;
                if (action == null || string.IsNullOrWhiteSpace(RackId))
                    return _isNewRack ? "Aquí no se puede insertar el rack nuevo." : "Aquí no se pueden insertar vistas del rack.";
                if (_writing) return WritingText;
                var drawing = _otherDrawingText?.Invoke() ?? string.Empty;
                if (!string.IsNullOrEmpty(drawing)) return drawing;
                var other = _otherRequestText?.Invoke() ?? string.Empty;
                if (!string.IsNullOrEmpty(other)) return other;
                if (_draft.Design == null) return "No hay un borrador válido que escribir.";
                if (!string.IsNullOrEmpty(_draft.LastError)) return _draft.LastError;
                if (_draft.IsOutputBlocked) return "No se puede dibujar: " + _draft.OutputStatus;
                return string.Empty;
            }
        }

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
                    return _isNewRack ? "Aquí no se puede insertar el rack nuevo." : "Aquí no se pueden insertar vistas del rack.";
                return _writing ? WritingText : string.Empty;
            }
        }

        /// <summary>The kinds of view the drawing already has for this rack (the last reading of the list, DG-04); the window sets it.</summary>
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

        /// <summary>«Insertar vista…»: opens the panel under the head of the editor (it writes only with «Insertar N vistas»).</summary>
        public void InsertView()
        {
            if (!CanOpenInsertPanel)
            {
                Say(InsertPanelBlockedText, error: true);
                return;
            }

            if (_insertPanel != null) return;
            SetConfirmingClose(false);
            _insertPanel = new RackViewInsertPanelViewModel(RackSystemKind.PushBack, InsertOptions(), InsertPanelContext(), InsertChosenViews, CloseInsertPanel);
            RaiseInsertPanel();
        }

        /// <summary>
        /// The views «Insertar vista…» offers now: a lateral by post that has a cut, the two frontal cuts of each side (the side B only in a
        /// two-way rack) and the plan, with the labels of «Insertar varias…» of the classic editor.
        /// </summary>
        public IReadOnlyList<RackViewInsertOption> InsertOptions()
        {
            var options = new List<RackViewInsertOption>();
            foreach (var cut in _draft.Computation?.LateralCortes ?? Array.Empty<RackCad.Application.Systems.Dynamic.DynamicLateralCorte>())
                options.Add(new RackViewInsertOption("Lateral · poste " + (cut.PostIndex + 1), RackViewAddress.Post(cut.PostIndex)));
            options.Add(new RackViewInsertOption("Frontal · entrada/salida A", RackViewAddress.PushBackCut(RackPushBackEnd.EntradaSalida, RackPushBackSide.A)));
            options.Add(new RackViewInsertOption("Frontal · posterior A", RackViewAddress.PushBackCut(RackPushBackEnd.Posterior, RackPushBackSide.A)));
            if (IsTwoSided)
            {
                options.Add(new RackViewInsertOption("Frontal · entrada/salida B", RackViewAddress.PushBackCut(RackPushBackEnd.EntradaSalida, RackPushBackSide.B)));
                options.Add(new RackViewInsertOption("Frontal · posterior B", RackViewAddress.PushBackCut(RackPushBackEnd.Posterior, RackPushBackSide.B)));
            }

            options.Add(new RackViewInsertOption("Planta", RackViewAddress.Whole(DimensionViewKind.Planta)));
            return options;
        }

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

        /// <summary>The window or the editor changed what the panel shows: it follows (hidden while AutoCAD writes, back if the door refused).</summary>
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

        /// <summary>The window: the door changed (another request waits for AutoCAD, or not).</summary>
        public void RefreshInsertState()
        {
            OnPropertyChanged(nameof(CanInsertView));
            OnPropertyChanged(nameof(InsertViewBlockedText));
            RaiseStateIns();
        }

        private void SuspendInsertPanel()
        {
            if (_insertPanel == null) return;
            _suspendedInsertPanel = _insertPanel;
            _insertPanel = null;
            RaiseInsertPanel();
        }

        private void RaiseStateIns()
        {
            OnPropertyChanged(nameof(CanOpenInsertPanel));
            OnPropertyChanged(nameof(InsertPanelBlockedText));
            if (_writing)
            {
                SuspendInsertPanel();
                return;
            }

            _insertPanel?.Update(InsertPanelContext());
        }

        private void InsertChosenViews(IReadOnlyList<RackViewAddress> views)
        {
            if (views == null || views.Count == 0) return;
            if (!CanInsertView)
            {
                Say(InsertViewBlockedText, error: true);
                return;
            }

            (_isNewRack ? _insertNew : _insertViews)(this, views);
        }

        private RackViewInsertPanelContext InsertPanelContext() => new RackViewInsertPanelContext(
            Name,
            _isNewRack,
            "Push Back",
            _dirty,
            InsertViewBlockedText,
            _existingViewKinds?.Invoke(),
            new RackViewInsertGroupTexts(
                IsTwoSided
                    ? "Los dos cortes de cada lado: entrada/salida (el extremo bajo, donde se carga) y posterior (el alto, el del tope)."
                    : "Los dos cortes: entrada/salida (el extremo bajo, donde se carga) y posterior (el alto, el del tope).",
                "Un corte por poste: entre dos frentes en blanco no hay. Puedes marcar varios.",
                "Todo el rack."));

        private void RaiseInsertPanel()
        {
            OnPropertyChanged(nameof(InsertPanel));
            OnPropertyChanged(nameof(IsInsertPanelOpen));
            OnPropertyChanged(nameof(InsertPanelStatus));
            OnPropertyChanged(nameof(CanOpenInsertPanel));
            OnPropertyChanged(nameof(InsertPanelBlockedText));
        }
    }
}
