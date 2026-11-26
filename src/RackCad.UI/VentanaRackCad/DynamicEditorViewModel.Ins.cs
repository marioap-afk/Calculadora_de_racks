using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Shared;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// The fourth batch of the window, part «ins» (I-64 A-14, line D; the prototype «dialogos», DG-01 to DG-08, accepted by the Owner on
    /// 2026-11-26), in the Dinámico editor: «Insertar vista…» (and «Insertar en el dibujo…» of a new Dinámico) opens the panel
    /// <see cref="RackViewInsertPanelViewModel"/> under the head of the editor instead of the classic dialog «Insertar varias vistas» as a
    /// modal of AutoCAD, the same panel as the Selectivo, with «Salida» and «Entrada». It opens also when the views cannot be inserted now
    /// and says why (DG-05); only while AutoCAD writes this rack is the button off. It follows the draft (DG-08), and «Insertar N vistas» is
    /// the write of «Insertar vista…» as before (RACKDINAMICOINSERTAR, or RACKDINAMICONUEVO for a new Dinámico).
    /// </summary>
    public sealed partial class DynamicEditorViewModel
    {
        private RackViewInsertPanelViewModel _insertPanel;
        private Func<IReadOnlyList<string>> _existingViewKinds;
        private Func<string> _otherRequestText;

        /// <summary>The panel «Insertar vista…», while it is open; null when it is closed.</summary>
        public RackViewInsertPanelViewModel InsertPanel => _insertPanel;

        public bool IsInsertPanelOpen => _insertPanel != null;

        /// <summary>«panel abierto» while the panel is open: the item status of the button «Insertar vista…» for UI Automation.</summary>
        public string InsertPanelStatus => IsInsertPanelOpen ? "panel abierto" : string.Empty;

        /// <summary>The button «Insertar vista…» is on: the window gave the editor its insertion, the rack has an identity, AutoCAD does not write it.</summary>
        public bool CanOpenInsertPanel => string.IsNullOrEmpty(InsertPanelBlockedText);

        /// <summary>Why the button «Insertar vista…» is off; empty when it is on.</summary>
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

        /// <summary>The kinds of view the drawing already has for this rack, as the list of the window read them (DG-04); the window sets it.</summary>
        public Func<IReadOnlyList<string>> ExistingViewKinds
        {
            get => _existingViewKinds;
            set { _existingViewKinds = value; RefreshInsertPanel(); }
        }

        /// <summary>Why the door of the window cannot take a write now (another request waits for AutoCAD), or empty; the window sets it.</summary>
        public Func<string> OtherRequestText
        {
            get => _otherRequestText;
            set { _otherRequestText = value; RaiseState(); }
        }

        private string OtherRequestBlockedText => _otherRequestText?.Invoke() ?? string.Empty;

        /// <summary>The views «Insertar vista…» offers now: the two frontal cuts, a lateral by post that has one, the plan.</summary>
        public IReadOnlyList<RackViewInsertOption> InsertOptions() =>
            RackCadModalDialogs.DynamicViewOptions(_editor.System, _editor.Catalog).Select(o => new RackViewInsertOption(o.Label, o.Address)).ToList();

        private void OpenInsertPanel()
        {
            if (_insertPanel != null) return;
            SetConfirmingClose(false);
            _insertPanel = new RackViewInsertPanelViewModel(RackSystemKind.PalletFlow, InsertOptions(), InsertPanelContext(), InsertChosenViews,
                CloseInsertPanel);
            RaiseInsertPanel();
        }

        /// <summary>«Cancelar» of the panel: it closes and nothing is inserted.</summary>
        public void CloseInsertPanel()
        {
            if (_insertPanel == null) return;
            _insertPanel = null;
            RaiseInsertPanel();
        }

        /// <summary>The window or the editor changed what the panel shows: the panel follows it; while AutoCAD writes this rack it closes.</summary>
        public void RefreshInsertPanel()
        {
            if (_insertPanel == null) return;
            if (_writing)
            {
                CloseInsertPanel();
                return;
            }

            _insertPanel.Follow(InsertOptions(), InsertPanelContext());
        }

        /// <summary>The window: the door changed (another request waits for AutoCAD, or not); the button and the open panel say it.</summary>
        public void RefreshInsertState()
        {
            OnPropertyChanged(nameof(CanInsertView));
            OnPropertyChanged(nameof(InsertViewBlockedText));
            RaiseStateIns();
        }

        private void RaiseStateIns()
        {
            OnPropertyChanged(nameof(CanOpenInsertPanel));
            OnPropertyChanged(nameof(InsertPanelBlockedText));
            if (_insertPanel == null) return;
            if (_writing)
            {
                CloseInsertPanel();
                return;
            }

            _insertPanel.Update(InsertPanelContext());
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
            "Dinámico",
            _dirty,
            InsertViewBlockedText,
            _existingViewKinds?.Invoke(),
            new RackViewInsertGroupTexts(
                "Los dos cortes frontales.",
                "Un corte por poste con frontera: entre dos frentes en blanco no hay (I-33). Puedes marcar varios.",
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
