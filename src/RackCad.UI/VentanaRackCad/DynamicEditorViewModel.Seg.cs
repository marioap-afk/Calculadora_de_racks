using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Selective;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Selective;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T4 seg (I-64, cuarta tanda, part «seg»; the prototype «seguridad», SG-01 to SG-07): the safety of a Dinámico is edited in «Extremos y
    /// seguridad» without the classic dialog. The table «Seguridad por poste» and the inspector of each family are the shared
    /// <see cref="RackCadSafetyPanelViewModel"/>; this is what the Dinámico gives it, with its own words for the sides («Salida / Entrada»,
    /// SG-02). The table is rebuilt with the summary every recomposition counts already (<c>RebuildSafety</c>).
    /// </summary>
    public sealed partial class DynamicEditorViewModel : IRackCadSafetyPanelHost
    {
        private RackCadSafetyPanelViewModel _safetyPanel;
        private IReadOnlyList<RackCadSafetyRow> _safetyRows = Array.Empty<RackCadSafetyRow>();

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

        /// <summary>The head of the card of the guide: the inspector shows its family.</summary>
        public void PickSafetyFamily(string type) => SafetyPanel.Pick(type);

        // A recomposition counted the safety post by post: the rows of the table come from that same summary.
        private void RebuildSafetyPanel(IReadOnlyList<DynamicPostSafetyRow> summary)
        {
            _safetyRows = _editor.System == null || _editor.Catalog == null
                ? Array.Empty<RackCadSafetyRow>()
                : DynamicSafetyRows.Build(_editor.System, _editor.Catalog, summary);
            _safetyPanel?.Refresh();
        }

        private void RefreshSafetyPanel() => _safetyPanel?.Refresh();

        RackCadSafetySystem IRackCadSafetyPanelHost.SafetySystem => RackCadSafetySystem.Dynamic;

        RackCatalog IRackCadSafetyPanelHost.SafetyCatalog => _editor.Catalog;

        bool IRackCadSafetyPanelHost.CanEditSafetyPanel => CanEdit && _editor.System != null;

        string IRackCadSafetyPanelHost.SafetyBlockedText => WritingText;

        int IRackCadSafetyPanelHost.SafetyFondoCount => 1;

        int IRackCadSafetyPanelHost.SafetyPostCount => _safetyRows.Count > 0 ? _safetyRows.Count : _editor.SafetyPostCount;

        IReadOnlyList<SelectiveSafetySelection> IRackCadSafetyPanelHost.CopySafetyForPanel() => _editor.CopySafety();

        IReadOnlyList<RackCadSafetyRow> IRackCadSafetyPanelHost.SafetyRows() => _safetyRows;

        IReadOnlyList<SelectiveParrillaPlan.Cell> IRackCadSafetyPanelHost.SafetyParrillaPlan => Array.Empty<SelectiveParrillaPlan.Cell>();

        SelectiveDesviadorPlan.ClearanceIssue IRackCadSafetyPanelHost.SafetyDesviadorClearance => null;

        string IRackCadSafetyPanelHost.WriteSafety(Func<List<SelectiveSafetySelection>, string> edit, string notice)
        {
            if (!CanEdit)
            {
                Say(WritingText, error: true);
                return WritingText;
            }

            var copy = _editor.CopySafety().ToList();
            var refusal = edit(copy);
            if (refusal != null) return refusal;
            var error = Apply(() => _editor.ReplaceSafety(copy), null);
            if (error == null && !string.IsNullOrWhiteSpace(notice)) Say(notice, error: false);
            return error;
        }

        // The guide of the entrance: the active cells with it, the active cells, and the pieces the BOM counts (a pair per cell).
        (int On, int Total, int Pieces) IRackCadSafetyPanelHost.CellFamily(string type)
        {
            if (!SelectiveSafetyDefaults.IsType(type, SelectiveSafetyDefaults.GuiaType)) return (0, 0, 0);
            var cells = ActiveCells().ToList();
            var guides = new HashSet<(int, int)>(_editor.GuideCells);
            return (cells.Count(guides.Contains), cells.Count, _safetyRows.Sum(r => r.Guides));
        }

        string IRackCadSafetyPanelHost.SetCellFamilyEverywhere(string type, bool on)
        {
            if (!SelectiveSafetyDefaults.IsType(type, SelectiveSafetyDefaults.GuiaType)) return null;
            if (on) GuideEverywhere();
            else GuideNowhere();
            return MessageIsError ? Message : null;
        }

        void IRackCadSafetyPanelHost.ShowMatrix() => SelectedTab = 1;
    }
}
