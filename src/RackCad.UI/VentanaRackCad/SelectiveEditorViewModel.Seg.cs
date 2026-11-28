using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Selective;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Selective;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T4 seg (I-64, cuarta tanda, part «seg»; the prototype «seguridad», SG-01 to SG-08): the safety of a Selectivo is edited in «Extremos y
    /// seguridad» without the classic dialog. The table «Seguridad por poste» and the inspector of each family are the shared
    /// <see cref="RackCadSafetyPanelViewModel"/>; this is what the Selectivo gives it. What reads the BOM is counted, as the rest of the tab,
    /// once per recomposition and only while the tab is shown (P1-04): the panel is refreshed from <c>RaiseSafetyTab</c>.
    /// </summary>
    public sealed partial class SelectiveEditorViewModel : IRackCadSafetyPanelHost
    {
        private RackCadSafetyPanelViewModel _safetyPanel;

        /// <summary>The editable table by post and the inspector of the families («Variante y medidas»).</summary>
        public RackCadSafetyPanelViewModel SafetyPanel
        {
            get
            {
                if (_safetyPanel == null)
                {
                    _safetyPanel = new RackCadSafetyPanelViewModel(this) { SayAction = Say };
                    _safetyPanel.Refresh();
                }

                return _safetyPanel;
            }
        }

        /// <summary>The head of a card (T or P): the inspector shows that family.</summary>
        public void PickSafetyFamily(string type) => SafetyPanel.Pick(type);

        // Called by the safety tab when it is shown or the draft changed while it is shown; a panel not asked for yet is built when read.
        private void RefreshSafetyPanel() => _safetyPanel?.Refresh();

        RackCadSafetySystem IRackCadSafetyPanelHost.SafetySystem => RackCadSafetySystem.Selective;

        RackCatalog IRackCadSafetyPanelHost.SafetyCatalog => _editor.Catalog;

        bool IRackCadSafetyPanelHost.CanEditSafetyPanel => CanEdit && _editor.System != null;

        string IRackCadSafetyPanelHost.SafetyBlockedText => BlockedEditText;

        int IRackCadSafetyPanelHost.SafetyFondoCount => _editor.FondoCount;

        // The posts of the classic dialog: those of the fondo with the most fronts (RackSelectiveWindow.Safety_Click).
        int IRackCadSafetyPanelHost.SafetyPostCount => Enumerable.Range(0, _editor.FondoCount).Select(_editor.FrontCount).DefaultIfEmpty(0).Max() + 1;

        IReadOnlyList<SelectiveSafetySelection> IRackCadSafetyPanelHost.CopySafetyForPanel() => _editor.CopySafety();

        IReadOnlyList<RackCadSafetyRow> IRackCadSafetyPanelHost.SafetyRows()
        {
            RefreshSafetyCounts();
            return _safetyCounts.Rows;
        }

        IReadOnlyList<SelectiveParrillaPlan.Cell> IRackCadSafetyPanelHost.SafetyParrillaPlan
        {
            get
            {
                RefreshSafetyCounts();
                return _safetyCounts.ParrillaPlan;
            }
        }

        SelectiveDesviadorPlan.ClearanceIssue IRackCadSafetyPanelHost.SafetyDesviadorClearance
        {
            get
            {
                RefreshSafetyCounts();
                return _safetyCounts.DesviadorClearance;
            }
        }

        string IRackCadSafetyPanelHost.WriteSafety(Func<List<SelectiveSafetySelection>, string> edit, string notice)
        {
            var error = Apply(() =>
            {
                var copy = _editor.CopySafety().ToList();
                var refusal = edit(copy);
                if (refusal != null) return SelectiveDraftEdit.Refuse(refusal);
                _editor.ReplaceSafety(copy);
                return Done;
            });
            if (error == null && !string.IsNullOrWhiteSpace(notice)) Say(notice, error: false);
            OnSafetyChanged();
            return error;
        }

        (int On, int Total, int Pieces) IRackCadSafetyPanelHost.CellFamily(string type)
        {
            var (on, total) = SelectiveSafetyView.CellsOn(_editor, type);
            RefreshSafetyCounts();
            var pieces = string.Equals(type, SelectiveSafetyDefaults.TopeType, StringComparison.Ordinal) ? _safetyCounts.Topes : _safetyCounts.Parrillas;
            return (on, total, pieces);
        }

        string IRackCadSafetyPanelHost.SetCellFamilyEverywhere(string type, bool on) => SetFamilyEverywhere(type, on);

        void IRackCadSafetyPanelHost.ShowMatrix() => SelectedTab = 1;
    }
}
