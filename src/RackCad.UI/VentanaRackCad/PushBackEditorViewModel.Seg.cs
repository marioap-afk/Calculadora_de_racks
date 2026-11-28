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
    /// T5 pb, PB-S03 (SG-09: Push Back adopts the design of the fourth batch): the safety of a Push Back is edited in «Extremos y seguridad»
    /// without the classic dialog. The table «Seguridad por poste» and the inspector of each family are the shared
    /// <see cref="RackCadSafetyPanelViewModel"/>, with the words of Push Back («Entrada/Salida / Posterior», SG-02) and its authority by side
    /// (I-42). Every write goes to a copy of the safety of the draft (<see cref="PushBackSafetyCopy"/>), as the classic dialog writes it
    /// (<see cref="PushBackSafetyEdit"/>), and the draft takes the copy and is recomposed; a refusal writes nothing.
    /// </summary>
    public sealed partial class PushBackEditorViewModel : IPushBackSafetyPanelHost
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

        // Every recomposition: the rows of the table are counted again as the BOM of Push Back counts them.
        private void RebuildSafetyPanel()
        {
            _safetyRows = _draft.System == null ? Array.Empty<RackCadSafetyRow>() : _draft.SafetyRows();
            _safetyPanel?.Refresh();
        }

        RackCadSafetySystem IRackCadSafetyPanelHost.SafetySystem => RackCadSafetySystem.PushBack;

        RackCatalog IRackCadSafetyPanelHost.SafetyCatalog => _draft.Catalog;

        bool IRackCadSafetyPanelHost.CanEditSafetyPanel => CanEdit && _draft.System != null;

        string IRackCadSafetyPanelHost.SafetyBlockedText => WritingText;

        int IRackCadSafetyPanelHost.SafetyFondoCount => 1;

        int IRackCadSafetyPanelHost.SafetyPostCount => _draft.SafetyPostCount;

        IReadOnlyList<SelectiveSafetySelection> IRackCadSafetyPanelHost.CopySafetyForPanel() => _draft.CopySafety().Safety;

        IReadOnlyList<RackCadSafetyRow> IRackCadSafetyPanelHost.SafetyRows() => _safetyRows;

        IReadOnlyList<SelectiveParrillaPlan.Cell> IRackCadSafetyPanelHost.SafetyParrillaPlan => Array.Empty<SelectiveParrillaPlan.Cell>();

        SelectiveDesviadorPlan.ClearanceIssue IRackCadSafetyPanelHost.SafetyDesviadorClearance => null;

        string IRackCadSafetyPanelHost.WriteSafety(Func<List<SelectiveSafetySelection>, string> edit, string notice)
            => ((IPushBackSafetyPanelHost)this).WritePushBackSafety(copy => edit(copy.Safety), notice);

        // Push Back has no family of the cells in the panel: its rear tope has its own card (and «T» in «Matriz»).
        (int On, int Total, int Pieces) IRackCadSafetyPanelHost.CellFamily(string type) => (0, 0, 0);

        string IRackCadSafetyPanelHost.SetCellFamilyEverywhere(string type, bool on) => null;

        void IRackCadSafetyPanelHost.ShowMatrix() => SelectedTab = MatrixTabIndex;

        bool IPushBackSafetyPanelHost.SafetyTwoSided => _draft.IsTwoSided;

        PushBackSafetyCopy IPushBackSafetyPanelHost.CopyPushBackSafety() => _draft.CopySafety();

        IReadOnlyList<int> IPushBackSafetyPanelHost.SafetyDesviadorLevels => _draft.DesviadorLevelsPerPost();

        IReadOnlyList<SelectiveSafetySelection> IPushBackSafetyPanelHost.ResolvedSafety()
            => _draft.System?.Structure?.SafetySelections?.Where(s => s != null).ToList() ?? new List<SelectiveSafetySelection>();

        string IPushBackSafetyPanelHost.WritePushBackSafety(Func<PushBackSafetyCopy, string> edit, string notice)
        {
            if (!CanEdit)
            {
                Say(WritingText, error: true);
                return WritingText;
            }

            var copy = _draft.CopySafety();
            string refusal;
            try
            {
                refusal = edit(copy);
            }
            catch (InvalidOperationException ex)
            {
                refusal = ex.Message;
            }

            if (refusal != null) return refusal;
            var error = Apply(() => _draft.ReplaceSafety(copy), null);
            if (error == null && !string.IsNullOrWhiteSpace(notice)) Say(notice, error: false);
            return error;
        }
    }
}
