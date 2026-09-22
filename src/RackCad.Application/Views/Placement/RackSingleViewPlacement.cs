using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Preparation;

namespace RackCad.Application.Views.Placement
{
    public enum RackSingleViewPlacementStatus
    {
        BlockedForNonRequirementReason,
        Placed,
        PlacedWithReport,
        Cancelled,
        DefinitionFailed,
        PlacementFailed,
        CleanupFailed
    }

    public enum RackRequirementIssue { Missing, InvalidKey }
    public enum RackRequirementRole { Required, OptionalVisual }
    public enum RackRequirementCause { None, LibraryUnavailable }

    public sealed class RackRequirementReportItem
    {
        public RackRequirementReportItem(string key, string piece, RackRequirementRole role,
            RackRequirementIssue issue, RackRequirementCause cause = RackRequirementCause.None)
        {
            Key = key;
            Piece = piece;
            Role = role;
            Issue = issue;
            Cause = cause;
        }

        public string Key { get; }
        public string Piece { get; }
        public RackRequirementRole Role { get; }
        public RackRequirementIssue Issue { get; }
        public RackRequirementCause Cause { get; }
    }

    public sealed class RackRequirementReport
    {
        public static RackRequirementReport Empty { get; } = new RackRequirementReport(Array.Empty<RackRequirementReportItem>());
        public RackRequirementReport(IReadOnlyList<RackRequirementReportItem> items)
            => Items = items ?? throw new ArgumentNullException(nameof(items));
        public IReadOnlyList<RackRequirementReportItem> Items { get; }
    }

    /// <summary>Maps final Foundation availability facts to I-55's ID17/ID18 report vocabulary.</summary>
    public static class RackSingleViewRequirementReporting
    {
        public static RackRequirementReport ForHeaderPlan(
            HeaderRunPlan plan,
            IReadOnlyList<LibraryBlockAvailabilityFact> facts,
            bool libraryUnavailable)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));
            if (facts == null) throw new ArgumentNullException(nameof(facts));

            var items = new List<RackRequirementReportItem>();
            var instances = plan.LooseInstances.Where(i => i != null).Concat(
                plan.Headers.Where(g => g != null).SelectMany(g => g.Instances).Where(i => i != null)).ToList();

            foreach (var instance in instances.Where(RequiresLibraryBlock).Where(i => string.IsNullOrWhiteSpace(i.BlockName)))
                items.Add(Item(instance, null, RackRequirementIssue.InvalidKey, RackRequirementCause.None));

            foreach (var fact in facts.Where(f => f.Availability == LibraryBlockAvailability.Missing))
            {
                var instance = instances.FirstOrDefault(i =>
                    string.Equals(i.BlockName, fact.Requirement.Key, StringComparison.OrdinalIgnoreCase));
                items.Add(instance == null
                    ? new RackRequirementReportItem(fact.Requirement.Key, null, RackRequirementRole.Required,
                        RackRequirementIssue.Missing,
                        libraryUnavailable ? RackRequirementCause.LibraryUnavailable : RackRequirementCause.None)
                    : Item(instance, fact.Requirement.Key, RackRequirementIssue.Missing,
                        libraryUnavailable ? RackRequirementCause.LibraryUnavailable : RackRequirementCause.None));
            }

            return new RackRequirementReport(items);
        }

        private static bool RequiresLibraryBlock(HeaderBlockInstance instance)
            => instance.Role != HeaderBlockRole.Annotation && instance.Role != HeaderBlockRole.Dimension;

        private static RackRequirementReportItem Item(HeaderBlockInstance instance, string key,
            RackRequirementIssue issue, RackRequirementCause cause)
            => new RackRequirementReportItem(key, instance.PieceId,
                instance.Role == HeaderBlockRole.Pallet ? RackRequirementRole.OptionalVisual : RackRequirementRole.Required,
                issue, cause);
    }

    public interface IRackSingleViewRequirementEvaluator<TPayload>
    {
        RackRequirementReport Evaluate(RackPreparedProductView<TPayload> product);
    }

    public readonly struct RackSingleViewDefinition<TDefinition>
    {
        public RackSingleViewDefinition(TDefinition handle, string uniqueName)
        {
            Handle = handle;
            UniqueName = uniqueName;
        }
        public TDefinition Handle { get; }
        public string UniqueName { get; }
    }

    public readonly struct RackSingleViewReference<TReference>
    {
        private RackSingleViewReference(bool placed, bool cancelled, TReference handle, string diagnostic)
        {
            IsPlaced = placed;
            IsCancelled = cancelled;
            Handle = handle;
            Diagnostic = diagnostic;
        }
        public bool IsPlaced { get; }
        public bool IsCancelled { get; }
        public TReference Handle { get; }
        public string Diagnostic { get; }
        public static RackSingleViewReference<TReference> Placed(TReference handle) => new RackSingleViewReference<TReference>(true, false, handle, null);
        public static RackSingleViewReference<TReference> Cancelled() => new RackSingleViewReference<TReference>(false, true, default, null);
        public static RackSingleViewReference<TReference> Failed(string diagnostic) => new RackSingleViewReference<TReference>(false, false, default, diagnostic);
    }

    public readonly struct RackSingleViewCleanupResult
    {
        public RackSingleViewCleanupResult(bool succeeded, string diagnostic = null)
        {
            Succeeded = succeeded;
            Diagnostic = diagnostic;
        }
        public bool Succeeded { get; }
        public string Diagnostic { get; }
    }

    public interface IRackSingleViewMaterializer<TPayload, TDefinition, TReference>
    {
        string ExpectedKind { get; }
        bool CanPlace(RackPreparedProductView<TPayload> product, out string diagnostic);
        RackSingleViewDefinition<TDefinition> Create(RackPreparedProductView<TPayload> product);
        RackSingleViewReference<TReference> Place(RackSingleViewDefinition<TDefinition> definition);
        RackSingleViewCleanupResult Cleanup(RackSingleViewDefinition<TDefinition> definition);
        void Complete(RackPreparedProductView<TPayload> product, TReference reference);
    }

    public sealed class RackSingleViewPlacementResult<TReference>
    {
        internal RackSingleViewPlacementResult(RackSingleViewPlacementStatus status, RackRequirementReport report,
            TReference reference, string blockName, string diagnostic)
        {
            Status = status;
            Report = report ?? RackRequirementReport.Empty;
            Reference = reference;
            BlockName = blockName;
            Diagnostic = diagnostic;
        }
        public RackSingleViewPlacementStatus Status { get; }
        public RackRequirementReport Report { get; }
        public TReference Reference { get; }
        public string BlockName { get; }
        public string Diagnostic { get; }
    }

    /// <summary>ID17/ID18 single-view policy. Requirement facts are reported and never become ID19's strict gate.</summary>
    public static class RackSingleViewPlacement
    {
        public static RackSingleViewPlacementResult<TReference> Place<TPayload, TDefinition, TReference>(
            RackPreparedProductView<TPayload> product,
            IRackSingleViewRequirementEvaluator<TPayload> requirements,
            IRackSingleViewMaterializer<TPayload, TDefinition, TReference> materializer)
        {
            if (product?.Prepared == null || requirements == null || materializer == null)
                return Result(RackSingleViewPlacementStatus.BlockedForNonRequirementReason,
                    RackRequirementReport.Empty, default(TReference), null, "PLACEMENT_INPUT_MISSING");

            if (!string.Equals(product.Prepared.Kind, materializer.ExpectedKind, StringComparison.OrdinalIgnoreCase))
                return Result(RackSingleViewPlacementStatus.BlockedForNonRequirementReason,
                    RackRequirementReport.Empty, default(TReference), null, "PLACEMENT_KIND_MISMATCH");

            if (!materializer.CanPlace(product, out var preflightDiagnostic))
                return Result(RackSingleViewPlacementStatus.BlockedForNonRequirementReason,
                    RackRequirementReport.Empty, default(TReference), null, preflightDiagnostic);

            RackRequirementReport report;
            try
            {
                report = requirements.Evaluate(product) ?? RackRequirementReport.Empty;
            }
            catch (Exception ex)
            {
                return Result(RackSingleViewPlacementStatus.BlockedForNonRequirementReason,
                    RackRequirementReport.Empty, default(TReference), null, ex.Message);
            }

            RackSingleViewDefinition<TDefinition> definition;
            try
            {
                definition = materializer.Create(product);
            }
            catch (Exception ex)
            {
                return Result(RackSingleViewPlacementStatus.DefinitionFailed,
                    report, default(TReference), null, ex.Message);
            }

            RackSingleViewReference<TReference> placed;
            try
            {
                placed = materializer.Place(definition);
            }
            catch (Exception ex)
            {
                return AfterFailedPlacement(materializer, definition, report, ex.Message);
            }

            if (!placed.IsPlaced)
            {
                var status = placed.IsCancelled
                    ? RackSingleViewPlacementStatus.Cancelled
                    : RackSingleViewPlacementStatus.PlacementFailed;
                return AfterFailedPlacement(materializer, definition, report, placed.Diagnostic, status);
            }

            try
            {
                materializer.Complete(product, placed.Handle);
            }
            catch (Exception ex)
            {
                return Result(RackSingleViewPlacementStatus.PlacementFailed,
                    report, placed.Handle, definition.UniqueName, ex.Message);
            }

            return Result(
                report.Items.Count == 0 ? RackSingleViewPlacementStatus.Placed : RackSingleViewPlacementStatus.PlacedWithReport,
                report,
                placed.Handle,
                definition.UniqueName,
                null);
        }

        private static RackSingleViewPlacementResult<TReference> AfterFailedPlacement<TPayload, TDefinition, TReference>(
            IRackSingleViewMaterializer<TPayload, TDefinition, TReference> materializer,
            RackSingleViewDefinition<TDefinition> definition,
            RackRequirementReport report,
            string diagnostic,
            RackSingleViewPlacementStatus status = RackSingleViewPlacementStatus.PlacementFailed)
        {
            try
            {
                var cleanup = materializer.Cleanup(definition);
                return cleanup.Succeeded
                    ? Result(status, report, default(TReference), definition.UniqueName, diagnostic)
                    : Result(RackSingleViewPlacementStatus.CleanupFailed, report, default(TReference),
                        definition.UniqueName, cleanup.Diagnostic);
            }
            catch (Exception ex)
            {
                return Result(RackSingleViewPlacementStatus.CleanupFailed, report, default(TReference),
                    definition.UniqueName, ex.Message);
            }
        }

        private static RackSingleViewPlacementResult<TReference> Result<TReference>(
            RackSingleViewPlacementStatus status,
            RackRequirementReport report,
            TReference reference,
            string blockName,
            string diagnostic)
            => new RackSingleViewPlacementResult<TReference>(status, report, reference, blockName, diagnostic);
    }
}
