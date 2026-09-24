using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Preparation;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Views.Batch
{
    public enum RackViewBatchOutcome
    {
        VARIANT_CANCELLED,
        SIBLING_GATE_FAILED,
        PREFLIGHT_FAILED,
        PREPARE_FAILED,
        REDRAW_ROLLED_BACK,
        REDRAW_APPLIED,
        REDRAW_NOT_REQUIRED,
        PLACEMENT_CANCELLED_PARTIAL_BATCH,
        PLACEMENT_FAILED_PARTIAL_BATCH,
        COMPLETED
    }

    public enum RackViewBatchRedrawState
    {
        NOT_STARTED,
        REDRAW_APPLIED,
        REDRAW_NOT_REQUIRED,
        REDRAW_ROLLED_BACK
    }

    public enum RackViewBatchStopReason
    {
        NONE,
        MULTIPLE_RACK_IDENTITIES,
        FLOW_BED_MULTI_VIEW_REJECTED,
        VARIANT_CANCELLED,
        SIBLING_GATE_FAILED,
        PREFLIGHT_FAILED,
        PREPARE_FAILED,
        REDRAW_ROLLED_BACK,
        PLACEMENT_CANCELLED,
        PLACEMENT_STOPPED,
        PLACEMENT_FAILED,
        COMPLETED
    }

    public sealed class RackViewBatchItem
    {
        public RackViewBatchItem(string rackId, RackViewAddress address)
        {
            RackId = rackId;
            Address = address;
        }

        public string RackId { get; }
        public RackViewAddress Address { get; }
    }

    public sealed class RackViewBatchRequest
    {
        public RackViewBatchRequest(
            RackProductSourceKind sourceKind,
            RackSystemKind systemKind,
            IReadOnlyList<RackViewBatchItem> views)
        {
            SourceKind = sourceKind;
            SystemKind = systemKind;
            Views = Copy(views);
        }

        public RackProductSourceKind SourceKind { get; }
        public RackSystemKind SystemKind { get; }
        public IReadOnlyList<RackViewBatchItem> Views { get; }

        private static IReadOnlyList<RackViewBatchItem> Copy(IReadOnlyList<RackViewBatchItem> views)
        {
            if (views == null) throw new ArgumentNullException(nameof(views));
            var copy = new RackViewBatchItem[views.Count];
            for (var index = 0; index < views.Count; index++)
                copy[index] = views[index] ?? throw new ArgumentException("A batch view cannot be null.", nameof(views));
            return Array.AsReadOnly(copy);
        }
    }

    public sealed class RackViewBatchWarning
    {
        public RackViewBatchWarning(string code, RackViewAddress address, string diagnostic)
        {
            Code = code;
            Address = address;
            Diagnostic = diagnostic;
        }

        public string Code { get; }
        public RackViewAddress Address { get; }
        public string Diagnostic { get; }
    }

    public readonly struct RackViewBatchGateResult
    {
        private RackViewBatchGateResult(bool accepted, string diagnostic)
        {
            Accepted = accepted;
            Diagnostic = diagnostic;
        }

        public bool Accepted { get; }
        public string Diagnostic { get; }
        public static RackViewBatchGateResult Accept() => new RackViewBatchGateResult(true, null);
        public static RackViewBatchGateResult Reject(string diagnostic) => new RackViewBatchGateResult(false, diagnostic);
    }

    public sealed class RackViewBatchPreparation<TPrepared>
    {
        private RackViewBatchPreparation(bool succeeded, TPrepared prepared, string diagnostic,
            IReadOnlyList<RackViewBatchWarning> warnings)
        {
            Succeeded = succeeded;
            Prepared = prepared;
            Diagnostic = diagnostic;
            Warnings = Copy(warnings);
        }

        public bool Succeeded { get; }
        public TPrepared Prepared { get; }
        public string Diagnostic { get; }
        public IReadOnlyList<RackViewBatchWarning> Warnings { get; }

        public static RackViewBatchPreparation<TPrepared> Success(
            TPrepared prepared,
            IReadOnlyList<RackViewBatchWarning> warnings = null)
            => new RackViewBatchPreparation<TPrepared>(true, prepared, null, warnings);

        public static RackViewBatchPreparation<TPrepared> Failed(string diagnostic)
            => new RackViewBatchPreparation<TPrepared>(false, default, diagnostic, null);

        private static IReadOnlyList<RackViewBatchWarning> Copy(IReadOnlyList<RackViewBatchWarning> warnings)
        {
            if (warnings == null || warnings.Count == 0) return Array.Empty<RackViewBatchWarning>();
            var copy = new RackViewBatchWarning[warnings.Count];
            for (var index = 0; index < warnings.Count; index++)
                copy[index] = warnings[index] ?? throw new ArgumentException("A batch warning cannot be null.", nameof(warnings));
            return Array.AsReadOnly(copy);
        }
    }

    public enum RackViewBatchRedrawResultKind
    {
        Applied,
        NotRequired,
        PreflightFailed,
        RolledBack
    }

    public readonly struct RackViewBatchRedrawResult
    {
        private RackViewBatchRedrawResult(RackViewBatchRedrawResultKind kind, string diagnostic)
        {
            Kind = kind;
            Diagnostic = diagnostic;
        }

        public RackViewBatchRedrawResultKind Kind { get; }
        public string Diagnostic { get; }
        public static RackViewBatchRedrawResult Applied(string diagnostic = null) =>
            new RackViewBatchRedrawResult(RackViewBatchRedrawResultKind.Applied, diagnostic);
        public static RackViewBatchRedrawResult NotRequired(string diagnostic = null) =>
            new RackViewBatchRedrawResult(RackViewBatchRedrawResultKind.NotRequired, diagnostic);
        public static RackViewBatchRedrawResult PreflightFailed(string diagnostic) =>
            new RackViewBatchRedrawResult(RackViewBatchRedrawResultKind.PreflightFailed, diagnostic);
        public static RackViewBatchRedrawResult RolledBack(string diagnostic) =>
            new RackViewBatchRedrawResult(RackViewBatchRedrawResultKind.RolledBack, diagnostic);
    }

    public enum RackViewBatchPlacementResultKind
    {
        Placed,
        Cancelled,
        Stopped
    }

    public readonly struct RackViewBatchPlacementResult
    {
        private RackViewBatchPlacementResult(RackViewBatchPlacementResultKind kind,
            IReadOnlyList<RackViewBatchWarning> warnings)
        {
            Kind = kind;
            Warnings = warnings ?? Array.Empty<RackViewBatchWarning>();
        }

        public RackViewBatchPlacementResultKind Kind { get; }
        public IReadOnlyList<RackViewBatchWarning> Warnings { get; }
        public static RackViewBatchPlacementResult Placed(IReadOnlyList<RackViewBatchWarning> warnings = null) =>
            new RackViewBatchPlacementResult(RackViewBatchPlacementResultKind.Placed, warnings);
        public static RackViewBatchPlacementResult Cancelled(IReadOnlyList<RackViewBatchWarning> warnings = null) =>
            new RackViewBatchPlacementResult(RackViewBatchPlacementResultKind.Cancelled, warnings);
        public static RackViewBatchPlacementResult Stopped(IReadOnlyList<RackViewBatchWarning> warnings = null) =>
            new RackViewBatchPlacementResult(RackViewBatchPlacementResultKind.Stopped, warnings);
    }

    public interface IRackViewBatchPort<TPrepared>
    {
        bool TryAcceptVariant(RackViewBatchItem requested, out RackViewBatchItem accepted, out string diagnostic);
        RackViewBatchGateResult CheckSiblingGate(RackViewBatchRequest request);
        RackViewBatchPreparation<TPrepared> Prepare(RackViewBatchItem item);
        RackViewBatchRedrawResult RedrawExisting(IReadOnlyList<TPrepared> prepared);
        RackViewBatchPlacementResult Place(RackViewBatchItem item, TPrepared prepared);
    }

    public sealed class RackViewBatchReport
    {
        internal RackViewBatchReport(
            string rackId,
            IReadOnlyList<RackViewAddress> requestedOrder,
            IReadOnlyList<RackViewAddress> placedOrder,
            RackViewAddress? currentAddress,
            RackViewBatchRedrawState redrawState,
            RackViewBatchStopReason stopReason,
            string diagnostic,
            IReadOnlyList<RackViewBatchWarning> warnings)
        {
            RackId = rackId;
            RequestedOrder = requestedOrder;
            PlacedOrder = placedOrder;
            CurrentAddress = currentAddress;
            RedrawState = redrawState;
            StopReason = stopReason;
            Diagnostic = diagnostic;
            Warnings = warnings;
        }

        public string RackId { get; }
        public int RequestedCount => RequestedOrder.Count;
        public int PlacedCount => PlacedOrder.Count;
        public IReadOnlyList<RackViewAddress> RequestedOrder { get; }
        public IReadOnlyList<RackViewAddress> PlacedOrder { get; }
        public RackViewAddress? CurrentAddress { get; }
        public RackViewBatchRedrawState RedrawState { get; }
        public RackViewBatchStopReason StopReason { get; }
        public string Diagnostic { get; }
        public IReadOnlyList<RackViewBatchWarning> Warnings { get; }
    }

    /// <summary>
    /// Pure ID18 state machine. It prepares the entire ordered queue before the one existing-rack redraw and never
    /// treats the independent placements as one transaction. The port supplies product preparation, the G9 seam and
    /// G8 placement semantics; this type owns only ordering and terminal outcomes.
    /// </summary>
    public sealed class RackViewBatchPlan<TPrepared>
    {
        private RackViewBatchPlan(
            RackViewBatchOutcome outcome,
            IReadOnlyList<RackViewBatchItem> requestedViews,
            IReadOnlyList<TPrepared> preparedViews,
            RackViewBatchReport report)
        {
            Outcome = outcome;
            RequestedViews = requestedViews;
            PreparedViews = preparedViews;
            Report = report;
        }

        public RackViewBatchOutcome Outcome { get; }
        public IReadOnlyList<RackViewBatchItem> RequestedViews { get; }
        public IReadOnlyList<TPrepared> PreparedViews { get; }
        public RackViewBatchReport Report { get; }
        public bool IsTerminal => Outcome != RackViewBatchOutcome.REDRAW_APPLIED
            && Outcome != RackViewBatchOutcome.REDRAW_NOT_REQUIRED;

        public static RackViewBatchPlan<TPrepared> Execute(
            RackViewBatchRequest request,
            IRackViewBatchPort<TPrepared> port)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (port == null) throw new ArgumentNullException(nameof(port));

            var original = request.Views;
            if (original.Count == 0 || !OneIdentity(original, out var rackId))
                return Finish(RackViewBatchOutcome.PREFLIGHT_FAILED, original, Array.Empty<TPrepared>(),
                    Array.Empty<RackViewAddress>(), null, RackViewBatchRedrawState.NOT_STARTED,
                    RackViewBatchStopReason.MULTIPLE_RACK_IDENTITIES, "ONE_RACK_REQUIRED", null);

            if (request.SystemKind == RackSystemKind.Cama && original.Count > 1)
                return Finish(RackViewBatchOutcome.PREPARE_FAILED, original, Array.Empty<TPrepared>(),
                    Array.Empty<RackViewAddress>(), null, RackViewBatchRedrawState.NOT_STARTED,
                    RackViewBatchStopReason.FLOW_BED_MULTI_VIEW_REJECTED, "FLOW_BED_BATCH_UNSUPPORTED", null);

            var accepted = new List<RackViewBatchItem>(original.Count);
            foreach (var item in original)
            {
                if (!port.TryAcceptVariant(item, out var acceptedItem, out var variantDiagnostic))
                    return Finish(RackViewBatchOutcome.VARIANT_CANCELLED, original, Array.Empty<TPrepared>(),
                        Array.Empty<RackViewAddress>(), item.Address, RackViewBatchRedrawState.NOT_STARTED,
                        RackViewBatchStopReason.VARIANT_CANCELLED, variantDiagnostic ?? "VARIANT_CANCELLED", null);
                if (acceptedItem == null) throw new InvalidOperationException("The accepted batch view cannot be null.");
                accepted.Add(acceptedItem);
            }

            if (!OneIdentity(accepted, out var acceptedRackId)
                || !string.Equals(rackId, acceptedRackId, StringComparison.OrdinalIgnoreCase))
                return Finish(RackViewBatchOutcome.PREFLIGHT_FAILED, accepted, Array.Empty<TPrepared>(),
                    Array.Empty<RackViewAddress>(), null, RackViewBatchRedrawState.NOT_STARTED,
                    RackViewBatchStopReason.MULTIPLE_RACK_IDENTITIES, "ONE_RACK_REQUIRED", null);

            var acceptedRequest = new RackViewBatchRequest(request.SourceKind, request.SystemKind, accepted);
            if (request.SourceKind == RackProductSourceKind.ExistingRack)
            {
                var gate = port.CheckSiblingGate(acceptedRequest);
                if (!gate.Accepted)
                    return Finish(RackViewBatchOutcome.SIBLING_GATE_FAILED, accepted, Array.Empty<TPrepared>(),
                        Array.Empty<RackViewAddress>(), null, RackViewBatchRedrawState.NOT_STARTED,
                        RackViewBatchStopReason.SIBLING_GATE_FAILED, gate.Diagnostic, null);
            }

            var prepared = new List<TPrepared>(accepted.Count);
            var warnings = new List<RackViewBatchWarning>();
            foreach (var item in accepted)
            {
                RackViewBatchPreparation<TPrepared> preparation;
                try
                {
                    preparation = port.Prepare(item);
                }
                catch (Exception ex)
                {
                    return Finish(RackViewBatchOutcome.PREPARE_FAILED, accepted, prepared,
                        Array.Empty<RackViewAddress>(), item.Address, RackViewBatchRedrawState.NOT_STARTED,
                        RackViewBatchStopReason.PREPARE_FAILED, ex.Message, warnings);
                }

                if (preparation == null || !preparation.Succeeded)
                    return Finish(RackViewBatchOutcome.PREPARE_FAILED, accepted, prepared,
                        Array.Empty<RackViewAddress>(), item.Address, RackViewBatchRedrawState.NOT_STARTED,
                        RackViewBatchStopReason.PREPARE_FAILED, preparation?.Diagnostic ?? "PREPARE_FAILED", warnings);
                prepared.Add(preparation.Prepared);
                warnings.AddRange(preparation.Warnings);
            }

            var redrawState = RackViewBatchRedrawState.REDRAW_NOT_REQUIRED;
            if (request.SourceKind == RackProductSourceKind.ExistingRack)
            {
                RackViewBatchRedrawResult redraw;
                try
                {
                    redraw = port.RedrawExisting(prepared);
                }
                catch (Exception ex)
                {
                    return Finish(RackViewBatchOutcome.REDRAW_ROLLED_BACK, accepted, prepared,
                        Array.Empty<RackViewAddress>(), null, RackViewBatchRedrawState.REDRAW_ROLLED_BACK,
                        RackViewBatchStopReason.REDRAW_ROLLED_BACK, ex.Message, warnings);
                }

                if (redraw.Kind == RackViewBatchRedrawResultKind.PreflightFailed)
                    return Finish(RackViewBatchOutcome.PREFLIGHT_FAILED, accepted, prepared,
                        Array.Empty<RackViewAddress>(), null, RackViewBatchRedrawState.NOT_STARTED,
                        RackViewBatchStopReason.PREFLIGHT_FAILED, redraw.Diagnostic, warnings);
                if (redraw.Kind == RackViewBatchRedrawResultKind.RolledBack)
                    return Finish(RackViewBatchOutcome.REDRAW_ROLLED_BACK, accepted, prepared,
                        Array.Empty<RackViewAddress>(), null, RackViewBatchRedrawState.REDRAW_ROLLED_BACK,
                        RackViewBatchStopReason.REDRAW_ROLLED_BACK, redraw.Diagnostic, warnings);
                redrawState = redraw.Kind == RackViewBatchRedrawResultKind.Applied
                    ? RackViewBatchRedrawState.REDRAW_APPLIED
                    : RackViewBatchRedrawState.REDRAW_NOT_REQUIRED;
            }

            var placed = new List<RackViewAddress>(accepted.Count);
            for (var index = 0; index < accepted.Count; index++)
            {
                var item = accepted[index];
                RackViewBatchPlacementResult placement;
                try
                {
                    placement = port.Place(item, prepared[index]);
                }
                catch (Exception ex)
                {
                    return Finish(RackViewBatchOutcome.PLACEMENT_FAILED_PARTIAL_BATCH, accepted, prepared,
                        placed, item.Address, redrawState, RackViewBatchStopReason.PLACEMENT_FAILED, ex.Message, warnings);
                }

                warnings.AddRange(placement.Warnings);

                if (placement.Kind != RackViewBatchPlacementResultKind.Placed)
                    return Finish(RackViewBatchOutcome.PLACEMENT_CANCELLED_PARTIAL_BATCH, accepted, prepared,
                        placed, item.Address, redrawState,
                        placement.Kind == RackViewBatchPlacementResultKind.Stopped
                            ? RackViewBatchStopReason.PLACEMENT_STOPPED
                            : RackViewBatchStopReason.PLACEMENT_CANCELLED,
                        placement.Kind.ToString().ToUpperInvariant(), warnings);
                placed.Add(item.Address);
            }

            return Finish(RackViewBatchOutcome.COMPLETED, accepted, prepared, placed, null, redrawState,
                RackViewBatchStopReason.COMPLETED, "BATCH_COMPLETED", warnings);
        }

        private static bool OneIdentity(IReadOnlyList<RackViewBatchItem> items, out string rackId)
        {
            rackId = items.Count == 0 ? null : items[0].RackId;
            if (string.IsNullOrWhiteSpace(rackId)) return false;
            for (var index = 1; index < items.Count; index++)
                if (!string.Equals(rackId, items[index].RackId, StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }

        private static RackViewBatchPlan<TPrepared> Finish(
            RackViewBatchOutcome outcome,
            IReadOnlyList<RackViewBatchItem> requested,
            IReadOnlyList<TPrepared> prepared,
            IReadOnlyList<RackViewAddress> placed,
            RackViewAddress? current,
            RackViewBatchRedrawState redraw,
            RackViewBatchStopReason stop,
            string diagnostic,
            IReadOnlyList<RackViewBatchWarning> warnings)
        {
            var requestedCopy = requested.ToArray();
            var preparedCopy = prepared.ToArray();
            var requestedOrder = requestedCopy.Select(item => item.Address).ToArray();
            var placedCopy = placed.ToArray();
            var warningCopy = warnings?.ToArray() ?? Array.Empty<RackViewBatchWarning>();
            return new RackViewBatchPlan<TPrepared>(
                outcome,
                Array.AsReadOnly(requestedCopy),
                Array.AsReadOnly(preparedCopy),
                new RackViewBatchReport(
                    requestedCopy.FirstOrDefault()?.RackId,
                    Array.AsReadOnly(requestedOrder),
                    Array.AsReadOnly(placedCopy),
                    current,
                    redraw,
                    stop,
                    diagnostic,
                    Array.AsReadOnly(warningCopy)));
        }
    }
}
