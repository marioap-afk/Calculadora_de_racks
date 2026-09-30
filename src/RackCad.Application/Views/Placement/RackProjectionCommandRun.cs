using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Shared;

namespace RackCad.Application.Views.Placement
{
    public enum RackProjectionPointStatus
    {
        Ok,
        None,
        Cancel,
        Error
    }

    /// <summary>One answer of the point prompt. Only <see cref="RackProjectionPointStatus.Ok"/> carries a point.</summary>
    public readonly struct RackProjectionPick
    {
        private RackProjectionPick(RackProjectionPointStatus status, Point3D point, string diagnostic)
        {
            Status = status;
            Point = point;
            Diagnostic = diagnostic;
        }

        public RackProjectionPointStatus Status { get; }
        public Point3D Point { get; }
        public string Diagnostic { get; }

        public static RackProjectionPick Ok(Point3D point) => new RackProjectionPick(RackProjectionPointStatus.Ok, point, null);
        public static RackProjectionPick None() => new RackProjectionPick(RackProjectionPointStatus.None, default, null);
        public static RackProjectionPick Cancel() => new RackProjectionPick(RackProjectionPointStatus.Cancel, default, null);

        public static RackProjectionPick Error(string diagnostic)
            => new RackProjectionPick(RackProjectionPointStatus.Error, default, diagnostic);
    }

    public enum RackProjectionSnapshotFailure
    {
        None,
        NoSelection,
        Cancelled,
        NoRackMembers,
        ReadFailed
    }

    /// <summary>
    /// The ONE read of the drawing (ACQUIRE + SNAPSHOT). Every fact the pure pipeline needs is captured here; after
    /// this object exists nothing rescans the drawing to make a semantic decision.
    /// </summary>
    public sealed class RackProjectionSnapshot
    {
        private RackProjectionSnapshot(
            RackProjectionRequest request,
            IReadOnlyList<string> notices,
            RackProjectionSnapshotFailure failure,
            string reason)
        {
            Request = request;
            Notices = notices ?? Array.Empty<string>();
            Failure = failure;
            Reason = reason;
        }

        public RackProjectionRequest Request { get; }
        public IReadOnlyList<string> Notices { get; }
        public RackProjectionSnapshotFailure Failure { get; }
        public string Reason { get; }
        public bool IsAvailable => Request != null && Failure == RackProjectionSnapshotFailure.None;

        public static RackProjectionSnapshot Available(RackProjectionRequest request, IReadOnlyList<string> notices)
            => new RackProjectionSnapshot(
                request ?? throw new ArgumentNullException(nameof(request)),
                notices,
                RackProjectionSnapshotFailure.None,
                null);

        public static RackProjectionSnapshot Unavailable(
            RackProjectionSnapshotFailure failure, string reason, IReadOnlyList<string> notices = null)
            => new RackProjectionSnapshot(null, notices, failure, reason);
    }

    /// <summary>What the drawing shows after the library import: the current supported query path, not a guess.</summary>
    public sealed class RackProjectionLibraryObservation
    {
        public RackProjectionLibraryObservation(
            IReadOnlyList<LibraryBlockAvailabilityFact> facts, bool libraryFileMissing, string diagnostic = null)
        {
            Facts = facts ?? Array.Empty<LibraryBlockAvailabilityFact>();
            LibraryFileMissing = libraryFileMissing;
            Diagnostic = diagnostic;
        }

        public IReadOnlyList<LibraryBlockAvailabilityFact> Facts { get; }
        public bool LibraryFileMissing { get; }
        public string Diagnostic { get; }
    }

    /// <summary>Everything RACKPROYECTAR needs from AutoCAD. The Plugin implements it; tests fake it.</summary>
    public interface IRackProjectionCommandPort
    {
        /// <summary>ACQUIRE + SNAPSHOT. Asks the selection and the class, then reads the drawing exactly once.</summary>
        RackProjectionSnapshot Capture();

        void Report(IReadOnlyList<string> lines);

        /// <summary>
        /// Called once, after both points and before the first mutation of the drawing (the library import), so the
        /// non-blocking units advisory (I-05) precedes every write of the operation.
        /// </summary>
        void BeforeWrite();

        RackProjectionPick PickBasePoint();
        RackProjectionPick PickTargetPoint(Point3D basePoint);

        /// <summary>
        /// Imports the library blocks of the accepted plan into the drawing and observes the drawing afterwards.
        /// It never writes racks. Importing does not prove a requirement: the caller reads the observation.
        /// </summary>
        RackProjectionLibraryObservation ImportAndObserve(IReadOnlyList<LibraryBlockRequirement> requirements);

        IRackProjectionWriteScopeFactory WriteScopes { get; }
    }

    public enum RackProjectionCommandStatus
    {
        Completed,
        SnapshotFailed,
        Blocked,
        Cancelled,
        PointFailed,
        PrerequisitesFailed,
        PlacementFailed,
        MaterializationFailed
    }

    public sealed class RackProjectionCommandResult
    {
        internal RackProjectionCommandResult(
            RackProjectionCommandStatus status,
            RackGroupPlacementPlanResult planResult,
            RackProjectionMaterializationResult materialization,
            IReadOnlyList<string> lines)
        {
            Status = status;
            PlanResult = planResult;
            Materialization = materialization;
            Lines = lines ?? Array.Empty<string>();
        }

        public RackProjectionCommandStatus Status { get; }
        public RackGroupPlacementPlanResult PlanResult { get; }
        public RackProjectionMaterializationResult Materialization { get; }
        public IReadOnlyList<string> Lines { get; }
        public bool IsCompleted => Status == RackProjectionCommandStatus.Completed;
    }

    /// <summary>
    /// ID19 as one operation: SNAPSHOT, the pure G14 plan (every blocking failure before any point), warnings, PICK,
    /// import + verification, ONE common transform, ONE caller-owned write, final report. It owns no AutoCAD type, no
    /// identity, no policy of its own and no partial-batch state: cancelling or failing anywhere writes nothing.
    /// </summary>
    public static class RackProjectionCommandRun
    {
        public static RackProjectionCommandResult Execute(IRackProjectionCommandPort port)
        {
            if (port == null) throw new ArgumentNullException(nameof(port));

            // SNAPSHOT: one read of the drawing.
            var snapshot = port.Capture();
            if (snapshot == null || !snapshot.IsAvailable)
            {
                var lines = RackProjectionReport.Snapshot(snapshot);
                port.Report(lines);
                return new RackProjectionCommandResult(
                    RackProjectionCommandStatus.SnapshotFailed, null, null, lines);
            }

            if (snapshot.Notices.Count > 0)
            {
                port.Report(snapshot.Notices);
            }

            // G14: CLASSIFY .. PLANS, entirely before the first point.
            var planResult = RackGroupPlacementPlan.Create(snapshot.Request);
            if (!planResult.IsAvailable)
            {
                var lines = RackProjectionReport.Blocked(planResult);
                port.Report(lines);
                return new RackProjectionCommandResult(RackProjectionCommandStatus.Blocked, planResult, null, lines);
            }

            var plan = planResult.Plan;

            // Warnings (overlap, near the window limit, optional visuals) show BEFORE the point and never block.
            var warnings = RackProjectionReport.Warnings(planResult.Warnings);
            if (warnings.Count > 0)
            {
                port.Report(warnings);
            }

            // PICK: OK continues; None (Enter) and Cancel (Esc) stop with nothing written; Error is a failure.
            var basePick = port.PickBasePoint();
            var stop = StopAfter(basePick, planResult);
            if (stop != null)
            {
                port.Report(stop.Lines);
                return stop;
            }

            var targetPick = port.PickTargetPoint(basePick.Point);
            stop = StopAfter(targetPick, planResult);
            if (stop != null)
            {
                port.Report(stop.Lines);
                return stop;
            }

            port.BeforeWrite();

            // IMPORT + verification of the final prerequisites, still before anything of a rack is written.
            var required = RackProjectionPrerequisites.Required(plan);
            if (required.Count > 0)
            {
                var observation = port.ImportAndObserve(RackProjectionPrerequisites.Keys(required));
                var missing = RackProjectionPrerequisites.Evaluate(required, observation);
                if (missing.Count > 0)
                {
                    var lines = RackProjectionReport.Prerequisites(missing, observation);
                    port.Report(lines);
                    return new RackProjectionCommandResult(
                        RackProjectionCommandStatus.PrerequisitesFailed, planResult, null, lines);
                }
            }

            // PLACE: one CommonTransform2D for the whole operation.
            var placed = plan.Place(basePick.Point, targetPick.Point);
            if (!placed.IsAvailable)
            {
                var lines = RackProjectionReport.Placement(placed.Failure);
                port.Report(lines);
                return new RackProjectionCommandResult(
                    RackProjectionCommandStatus.PlacementFailed, planResult, null, lines);
            }

            var units = RackProjectionUnits.Build(plan, placed);
            if (units == null)
            {
                var lines = RackProjectionReport.Placement(CommonTransformFailure.NotRigid);
                port.Report(lines);
                return new RackProjectionCommandResult(
                    RackProjectionCommandStatus.PlacementFailed, planResult, null, lines);
            }

            // MATERIALIZE: ONE caller-owned transaction, ONE commit; any failure rolls the whole write back.
            var materialization = RackProjectionMaterializationRun.Execute(port.WriteScopes, units);
            if (!materialization.IsCommitted)
            {
                var lines = RackProjectionReport.Materialization(materialization);
                port.Report(lines);
                return new RackProjectionCommandResult(
                    RackProjectionCommandStatus.MaterializationFailed, planResult, materialization, lines);
            }

            var done = RackProjectionReport.Completed(materialization, plan.Orientation);
            port.Report(done);
            return new RackProjectionCommandResult(
                RackProjectionCommandStatus.Completed, planResult, materialization, done);
        }

        private static RackProjectionCommandResult StopAfter(RackProjectionPick pick, RackGroupPlacementPlanResult planResult)
        {
            switch (pick.Status)
            {
                case RackProjectionPointStatus.Ok:
                    return null;
                case RackProjectionPointStatus.Error:
                    return new RackProjectionCommandResult(
                        RackProjectionCommandStatus.PointFailed,
                        planResult,
                        null,
                        RackProjectionReport.PointFailed(pick.Diagnostic));
                default:
                    return new RackProjectionCommandResult(
                        RackProjectionCommandStatus.Cancelled, planResult, null, RackProjectionReport.Cancelled());
            }
        }
    }

    /// <summary>The library requirements of the accepted plan and the strict reading of their final observation.</summary>
    public static class RackProjectionPrerequisites
    {
        /// <summary>Every requirement of a piece that needs a library block, in plan order.</summary>
        public static IReadOnlyList<LibraryPieceRequirement> Required(RackGroupPlacementPlan plan)
        {
            if (plan == null) throw new ArgumentNullException(nameof(plan));

            return plan.Groups
                .SelectMany(group => group.Preparation.PieceFacts)
                .Select(fact => fact?.Requirement)
                .Where(requirement => requirement != null
                    && requirement.Role != RequirementRole.NotApplicable
                    && requirement.KeyState == RequirementKeyState.Present)
                .ToList();
        }

        public static IReadOnlyList<LibraryBlockRequirement> Keys(IReadOnlyList<LibraryPieceRequirement> requirements)
            => LibraryPieceKeyProjection.Project(requirements);

        /// <summary>
        /// Required pieces whose block the DRAWING still lacks after the import. An optional visual never counts; a key
        /// the observation does not mention is missing, never assumed present.
        /// </summary>
        public static IReadOnlyList<LibraryPieceRequirement> Evaluate(
            IReadOnlyList<LibraryPieceRequirement> requirements, RackProjectionLibraryObservation observation)
        {
            var found = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (observation != null)
            {
                foreach (var fact in observation.Facts)
                {
                    if (fact.Availability == LibraryBlockAvailability.Found)
                    {
                        found.Add(fact.Requirement.Key);
                    }
                }
            }

            return requirements
                .Where(requirement => requirement.Role == RequirementRole.Required
                    && !found.Contains(requirement.LibraryKey))
                .ToList();
        }
    }

    /// <summary>Builds the per-RackId units of the write from the accepted plan and its single placement result.</summary>
    internal static class RackProjectionUnits
    {
        internal static IReadOnlyList<RackProjectionMaterializationUnit> Build(
            RackGroupPlacementPlan plan, RackGroupPlacementResult placed)
        {
            var units = new List<RackProjectionMaterializationUnit>(plan.Groups.Count);
            foreach (var group in plan.Groups)
            {
                var view = group.Preparation?.PreparedView;
                if (view == null
                    || !string.Equals(view.RackId, group.RackId, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                var placements = placed.Placements
                    .Where(placement => string.Equals(placement.RackId, group.RackId, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                units.Add(new RackProjectionMaterializationUnit(group, view, placements));
            }

            return units;
        }
    }
}
