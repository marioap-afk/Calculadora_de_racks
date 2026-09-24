using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Batch;
using RackCad.Application.Views.Placement;

namespace RackCad.Plugin.Views
{
    /// <summary>
    /// AutoCAD edge for ID18. The Application state machine owns every transition; this adapter maps Autodesk prompt
    /// results and enforces INV-TX-1 before each independent G8 placement.
    /// </summary>
    internal sealed class RackViewBatchDriver<TPrepared> : IRackViewBatchPort<TPrepared>
    {
        private readonly TransactionManager transactions;
        private readonly Func<RackViewBatchItem, RackViewBatchPreparation<TPrepared>> prepare;
        private readonly Func<RackViewBatchRequest, RackViewBatchGateResult> siblingGate;
        private readonly Func<IReadOnlyList<TPrepared>, RackViewBatchRedrawResult> redraw;
        private readonly Func<RackViewBatchItem, TPrepared, RackSingleViewPlacementResult<ObjectId>> place;

        internal RackViewBatchDriver(
            TransactionManager transactions,
            Func<RackViewBatchItem, RackViewBatchPreparation<TPrepared>> prepare,
            Func<RackViewBatchRequest, RackViewBatchGateResult> siblingGate,
            Func<IReadOnlyList<TPrepared>, RackViewBatchRedrawResult> redraw,
            Func<RackViewBatchItem, TPrepared, RackSingleViewPlacementResult<ObjectId>> place)
        {
            this.transactions = transactions ?? throw new ArgumentNullException(nameof(transactions));
            this.prepare = prepare ?? throw new ArgumentNullException(nameof(prepare));
            this.siblingGate = siblingGate ?? throw new ArgumentNullException(nameof(siblingGate));
            this.redraw = redraw ?? throw new ArgumentNullException(nameof(redraw));
            this.place = place ?? throw new ArgumentNullException(nameof(place));
        }

        internal RackViewBatchPlan<TPrepared> Execute(RackViewBatchRequest request)
            => RackViewBatchPlan<TPrepared>.Execute(request, this);

        public bool TryAcceptVariant(RackViewBatchItem requested, out RackViewBatchItem accepted, out string diagnostic)
        {
            accepted = requested;
            diagnostic = null;
            return true;
        }

        public RackViewBatchGateResult CheckSiblingGate(RackViewBatchRequest request) => siblingGate(request);
        public RackViewBatchPreparation<TPrepared> Prepare(RackViewBatchItem item) => prepare(item);
        public RackViewBatchRedrawResult RedrawExisting(IReadOnlyList<TPrepared> prepared) => redraw(prepared);

        public RackViewBatchPlacementResult Place(RackViewBatchItem item, TPrepared prepared)
        {
            if (transactions.TopTransaction != null)
                throw new InvalidOperationException("TOP_TRANSACTION_OPEN");
            var result = place(item, prepared);
            var warnings = ToWarnings(item.Address, result.Report);
            switch (result.Status)
            {
                case RackSingleViewPlacementStatus.Placed:
                case RackSingleViewPlacementStatus.PlacedWithReport:
                    return RackViewBatchPlacementResult.Placed(warnings);
                case RackSingleViewPlacementStatus.Cancelled:
                    return RackViewBatchPlacementResult.Cancelled(warnings);
                case RackSingleViewPlacementStatus.Stopped:
                    return RackViewBatchPlacementResult.Stopped(warnings);
                default:
                    throw new InvalidOperationException("PLACEMENT_" + result.Status.ToString().ToUpperInvariant()
                        + (string.IsNullOrWhiteSpace(result.Diagnostic) ? string.Empty : ": " + result.Diagnostic));
            }
        }

        private static IReadOnlyList<RackViewBatchWarning> ToWarnings(
            RackViewAddress address, RackRequirementReport report)
        {
            if (report == null || report.Items.Count == 0) return Array.Empty<RackViewBatchWarning>();
            var warnings = new RackViewBatchWarning[report.Items.Count];
            for (var index = 0; index < report.Items.Count; index++)
            {
                var item = report.Items[index];
                warnings[index] = new RackViewBatchWarning(
                    item.Issue == RackRequirementIssue.Missing ? "MISSING_REQUIRED_BLOCK" : "INVALID_BLOCK_KEY",
                    address,
                    string.IsNullOrWhiteSpace(item.Key) ? item.Piece : item.Key);
            }
            return warnings;
        }
    }
}
