using System;
using RackCad.Application.Views.Redraw;

namespace RackCad.Application.Views.Insertion
{
    public enum RackSiblingInsertOutcome
    {
        VariantCancelled,
        PreflightFailed,
        RedrawRolledBack,
        RedrawApplied,
        RedrawNotRequired,
        PlacementBlocked,
        PlacementCancelled,
        PlacementFailed,
        PlacementApplied
    }

    public readonly struct RackInsertGateResult
    {
        private RackInsertGateResult(bool accepted, string diagnostic)
        {
            Accepted = accepted;
            Diagnostic = diagnostic;
        }

        public bool Accepted { get; }
        public string Diagnostic { get; }
        public static RackInsertGateResult Proceed() => new RackInsertGateResult(true, null);
        public static RackInsertGateResult Reject(string diagnostic) => new RackInsertGateResult(false, diagnostic);
    }

    public enum RackInsertPlacementKind { Applied, Cancelled, Failed }

    public readonly struct RackInsertPlacementResult
    {
        private RackInsertPlacementResult(RackInsertPlacementKind kind, string diagnostic)
        {
            Kind = kind;
            Diagnostic = diagnostic;
        }

        public RackInsertPlacementKind Kind { get; }
        public string Diagnostic { get; }
        public static RackInsertPlacementResult Applied(string diagnostic = null) => new RackInsertPlacementResult(RackInsertPlacementKind.Applied, diagnostic);
        public static RackInsertPlacementResult Cancelled(string diagnostic = null) => new RackInsertPlacementResult(RackInsertPlacementKind.Cancelled, diagnostic);
        public static RackInsertPlacementResult Failed(string diagnostic) => new RackInsertPlacementResult(RackInsertPlacementKind.Failed, diagnostic);
    }

    public sealed class RackSiblingInsertResult
    {
        internal RackSiblingInsertResult(RackSiblingInsertOutcome outcome, string diagnostic,
            RackSiblingRedrawDisposition? disposition, RackSiblingInsertOutcome? redrawOutcome)
        {
            Outcome = outcome;
            Diagnostic = diagnostic;
            RedrawDisposition = disposition;
            RedrawOutcome = redrawOutcome;
        }

        public RackSiblingInsertOutcome Outcome { get; }
        public string Diagnostic { get; }
        public RackSiblingRedrawDisposition? RedrawDisposition { get; }
        public RackSiblingInsertOutcome? RedrawOutcome { get; }
    }

    /// <summary>
    /// Product-owned ordering for one existing-rack Insert gesture. The port owns AutoCAD details; this driver owns
    /// the fail-closed order and guarantees that every gate sees the same membership object from one scan.
    /// </summary>
    public interface IRackSiblingInsertPort<TVariant, TPrepared, TRedrawUnit>
    {
        RackSiblingMembershipSnapshot ScanAndClassifyOnce();
        bool TryChooseVariant(out TVariant variant);
        RackInsertGateResult CheckCustomProperties(RackSiblingMembershipSnapshot membership);
        RackInsertGateResult CheckAuthored(RackSiblingMembershipSnapshot membership);
        bool TryPrepare(TVariant variant, RackSiblingMembershipSnapshot membership, out TPrepared prepared, out string diagnostic);
        ISiblingRedrawPort<TRedrawUnit> CreateRedrawPort(TPrepared prepared);
        bool HasOpenTopTransaction { get; }
        RackInsertPlacementResult Place(TPrepared prepared, RackSiblingRedrawPlan<TRedrawUnit> deferredRedraw);
    }

    public static class RackSiblingInsertRun
    {
        public static RackSiblingInsertResult Execute<TVariant, TPrepared, TRedrawUnit>(
            IRackSiblingInsertPort<TVariant, TPrepared, TRedrawUnit> port)
        {
            if (port == null) throw new ArgumentNullException(nameof(port));

            var membership = port.ScanAndClassifyOnce()
                ?? throw new InvalidOperationException("The sibling scan did not return a membership snapshot.");

            if (!port.TryChooseVariant(out var variant))
                return Result(RackSiblingInsertOutcome.VariantCancelled, "VARIANT_CANCELLED");

            var properties = port.CheckCustomProperties(membership);
            if (!properties.Accepted)
                return Result(RackSiblingInsertOutcome.PreflightFailed, properties.Diagnostic);

            var authored = port.CheckAuthored(membership);
            if (!authored.Accepted)
                return Result(RackSiblingInsertOutcome.PreflightFailed, authored.Diagnostic);

            if (!port.TryPrepare(variant, membership, out var prepared, out var prepareDiagnostic))
                return Result(RackSiblingInsertOutcome.PreflightFailed, prepareDiagnostic);

            var redraw = RackSiblingRedrawRun.Execute(membership, port.CreateRedrawPort(prepared));
            if (redraw.Outcome == RackSiblingRedrawOutcome.PrepareFailed)
                return Result(RackSiblingInsertOutcome.PreflightFailed, redraw.Diagnostic);
            if (redraw.Outcome == RackSiblingRedrawOutcome.Discarded)
                return Result(RackSiblingInsertOutcome.RedrawRolledBack, redraw.Diagnostic, redraw.Plan?.Disposition);

            var redrawOutcome = redraw.Outcome == RackSiblingRedrawOutcome.Committed
                ? RackSiblingInsertOutcome.RedrawApplied
                : RackSiblingInsertOutcome.RedrawNotRequired;

            if (port.HasOpenTopTransaction)
                return Result(RackSiblingInsertOutcome.PlacementBlocked, "TOP_TRANSACTION_OPEN", redraw.Plan?.Disposition);

            var placement = port.Place(prepared,
                redraw.Plan?.Disposition == RackSiblingRedrawDisposition.DeferToFirstPlacement ? redraw.Plan : null);
            var report = Report(redraw, placement);
            switch (placement.Kind)
            {
                case RackInsertPlacementKind.Applied:
                    return Result(RackSiblingInsertOutcome.PlacementApplied, report, redraw.Plan?.Disposition, redrawOutcome);
                case RackInsertPlacementKind.Cancelled:
                    return Result(RackSiblingInsertOutcome.PlacementCancelled, report, redraw.Plan?.Disposition, redrawOutcome);
                default:
                    return Result(RackSiblingInsertOutcome.PlacementFailed, report, redraw.Plan?.Disposition, redrawOutcome);
            }
        }

        private static string Report<TUnit>(RackSiblingRedrawResult<TUnit> redraw, RackInsertPlacementResult placement)
        {
            var parts = new System.Collections.Generic.List<string>();
            if (!string.IsNullOrWhiteSpace(redraw?.Diagnostic)
                && !string.Equals(redraw.Diagnostic, RackSiblingRedrawDisposition.DeferToFirstPlacement.ToString(), StringComparison.Ordinal)
                && !string.Equals(redraw.Diagnostic, RackSiblingRedrawDisposition.NoMutation.ToString(), StringComparison.Ordinal))
                parts.Add(redraw.Diagnostic);
            if (redraw?.Plan?.ReadOnly.Count > 0)
                parts.Add("READ_ONLY=" + string.Join(",", Keys(redraw.Plan.ReadOnly)));
            if (redraw?.Plan?.EraseUnits.Count > 0
                && (redraw.Plan.Disposition == RackSiblingRedrawDisposition.MutateNow
                    || placement.Kind == RackInsertPlacementKind.Applied))
                parts.Add("ERASE=" + string.Join(",", EraseKeys(redraw.Plan.EraseUnits)));
            if (!string.IsNullOrWhiteSpace(placement.Diagnostic)) parts.Add(placement.Diagnostic);
            return parts.Count == 0 ? null : string.Join("; ", parts);
        }

        private static System.Collections.Generic.IEnumerable<string> Keys(
            System.Collections.Generic.IReadOnlyList<RackSiblingMember> members)
        {
            foreach (var member in members) yield return member.Fact.DefinitionKey;
        }

        private static System.Collections.Generic.IEnumerable<string> EraseKeys<TUnit>(
            System.Collections.Generic.IReadOnlyList<RackSiblingPreparedUnit<TUnit>> units)
        {
            foreach (var unit in units) yield return unit.Member.Fact.DefinitionKey;
        }

        private static RackSiblingInsertResult Result(
            RackSiblingInsertOutcome outcome,
            string diagnostic,
            RackSiblingRedrawDisposition? disposition = null,
            RackSiblingInsertOutcome? redrawOutcome = null)
            => new RackSiblingInsertResult(outcome, diagnostic, disposition, redrawOutcome);
    }
}
