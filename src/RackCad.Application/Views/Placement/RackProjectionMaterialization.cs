using System;
using System.Collections.Generic;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Cantilever;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Views.Placement
{
    /// <summary>
    /// Which AUTH-15 family materialises a plan. Dispatch is by plan FAMILY, never by rack kind: the header-run
    /// family serves Selective, Dynamic, Push Back and Cabecera; the Cantilever family serves Cantilever. Flow Bed is
    /// not exposed to ID19 and has no family.
    /// </summary>
    public enum RackProjectionMaterializationFamily
    {
        Unsupported,
        HeaderRun,
        Cantilever
    }

    public static class RackProjectionMaterializationFamilies
    {
        public static RackProjectionMaterializationFamily Of(RackSystemKind kind)
        {
            switch (kind)
            {
                case RackSystemKind.SelectiveRack:
                case RackSystemKind.PalletFlow:
                case RackSystemKind.PushBack:
                case RackSystemKind.Selective:
                    return RackProjectionMaterializationFamily.HeaderRun;
                case RackSystemKind.Cantilever:
                    return RackProjectionMaterializationFamily.Cantilever;
                default:
                    return RackProjectionMaterializationFamily.Unsupported;
            }
        }
    }

    /// <summary>
    /// What the product already prepared for one target view of one RackId: the typed plan, the base name and the
    /// envelope composed with the EXISTING RackId. AUTH-15 validates and writes it; nothing downstream recomposes it.
    /// </summary>
    public sealed class RackProjectionPreparedView
    {
        public RackProjectionPreparedView(
            string rackId,
            RackSystemKind systemKind,
            RackViewAddress targetAddress,
            string baseName,
            RackEmbedDocument envelope,
            HeaderRunPlan headerPlan,
            CantileverViewPlan cantileverPlan)
        {
            if (string.IsNullOrWhiteSpace(rackId))
                throw new ArgumentException("A projected view keeps the RackId of its source.", nameof(rackId));
            if (envelope == null) throw new ArgumentNullException(nameof(envelope));
            if ((headerPlan == null) == (cantileverPlan == null))
                throw new ArgumentException("Exactly one typed plan is required.");

            var family = RackProjectionMaterializationFamilies.Of(systemKind);
            var payloadFamily = headerPlan != null
                ? RackProjectionMaterializationFamily.HeaderRun
                : RackProjectionMaterializationFamily.Cantilever;
            if (family == RackProjectionMaterializationFamily.Unsupported || family != payloadFamily)
                throw new ArgumentException("The plan family does not serve this rack kind.", nameof(systemKind));
            if (!string.Equals(envelope.Id, rackId, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("The envelope must carry the RackId of the source rack.", nameof(envelope));

            RackId = rackId;
            SystemKind = systemKind;
            TargetAddress = targetAddress;
            BaseName = baseName;
            Envelope = envelope;
            HeaderPlan = headerPlan;
            CantileverPlan = cantileverPlan;
            Family = family;
        }

        public string RackId { get; }
        public RackSystemKind SystemKind { get; }
        public RackViewAddress TargetAddress { get; }
        public string BaseName { get; }
        public RackEmbedDocument Envelope { get; }
        public HeaderRunPlan HeaderPlan { get; }
        public CantileverViewPlan CantileverPlan { get; }
        public RackProjectionMaterializationFamily Family { get; }
    }

    public enum RackProjectionWriteFailure
    {
        None,
        ScopeUnavailable,
        DefinitionCreationFailed,
        DefinitionIncomplete,
        ReferencePlacementFailed,
        CommitFailed,
        UnexpectedException
    }

    /// <summary>Outcome of AUTH-15 for one definition, inside the caller-owned transaction.</summary>
    public readonly struct RackProjectionDefinitionResult
    {
        private RackProjectionDefinitionResult(
            bool isSuccess, string definitionName, object handle, RackProjectionWriteFailure failure, string diagnostic)
        {
            IsSuccess = isSuccess;
            DefinitionName = definitionName;
            Handle = handle;
            Failure = failure;
            Diagnostic = diagnostic;
        }

        public bool IsSuccess { get; }
        public string DefinitionName { get; }

        /// <summary>Opaque to Application: the Plugin keeps the AutoCAD identity of the new definition here.</summary>
        public object Handle { get; }

        public RackProjectionWriteFailure Failure { get; }
        public string Diagnostic { get; }

        public static RackProjectionDefinitionResult Created(string definitionName, object handle)
            => new RackProjectionDefinitionResult(true, definitionName, handle, RackProjectionWriteFailure.None, null);

        public static RackProjectionDefinitionResult Failed(RackProjectionWriteFailure failure, string diagnostic)
            => new RackProjectionDefinitionResult(false, null, null, failure, diagnostic);
    }

    public readonly struct RackProjectionReferenceResult
    {
        private RackProjectionReferenceResult(bool isSuccess, string diagnostic)
        {
            IsSuccess = isSuccess;
            Diagnostic = diagnostic;
        }

        public bool IsSuccess { get; }
        public string Diagnostic { get; }

        public static RackProjectionReferenceResult Placed() => new RackProjectionReferenceResult(true, null);

        public static RackProjectionReferenceResult Failed(string diagnostic)
            => new RackProjectionReferenceResult(false, diagnostic);
    }

    /// <summary>
    /// The caller-owned write scope of ONE RACKPROYECTAR operation: it holds the document lock and ONE transaction.
    /// Disposing it without <see cref="Commit"/> discards everything (there is deliberately no Abort and no cleanup).
    /// </summary>
    public interface IRackProjectionWriteScope : IDisposable
    {
        /// <summary>AUTH-15 seam: create ONE enveloped definition inside the scope's transaction. Never commits.</summary>
        RackProjectionDefinitionResult CreateDefinition(RackProjectionPreparedView view);

        /// <summary>G15 owns the reference: it is appended to the target space under the same transaction.</summary>
        RackProjectionReferenceResult PlaceReference(
            RackProjectionDefinitionResult definition, RackProjectedPlacement placement);

        void Commit();
    }

    public interface IRackProjectionWriteScopeFactory
    {
        IRackProjectionWriteScope Begin();
    }

    /// <summary>The accepted plan of one RackId: its prepared view plus one placement per source reference.</summary>
    public sealed class RackProjectionMaterializationUnit
    {
        public RackProjectionMaterializationUnit(
            RackProjectionGroup group,
            RackProjectionPreparedView view,
            IReadOnlyList<RackProjectedPlacement> placements)
        {
            Group = group ?? throw new ArgumentNullException(nameof(group));
            View = view ?? throw new ArgumentNullException(nameof(view));
            Placements = placements ?? throw new ArgumentNullException(nameof(placements));
        }

        public RackProjectionGroup Group { get; }
        public RackProjectionPreparedView View { get; }
        public IReadOnlyList<RackProjectedPlacement> Placements { get; }
    }

    public enum RackProjectionMaterializationStatus
    {
        Committed,
        RolledBack
    }

    public sealed class RackProjectionMaterializationResult
    {
        internal RackProjectionMaterializationResult(
            RackProjectionMaterializationStatus status,
            RackProjectionWriteFailure failure,
            string rackId,
            string diagnostic,
            int definitions,
            int references)
        {
            Status = status;
            Failure = failure;
            RackId = rackId;
            Diagnostic = diagnostic;
            Definitions = definitions;
            References = references;
        }

        public RackProjectionMaterializationStatus Status { get; }
        public RackProjectionWriteFailure Failure { get; }
        public string RackId { get; }
        public string Diagnostic { get; }
        public int Definitions { get; }
        public int References { get; }
        public bool IsCommitted => Status == RackProjectionMaterializationStatus.Committed;
    }

    /// <summary>
    /// The single write of RACKPROYECTAR. One scope, one transaction, one Commit: definitions are created (AUTH-15)
    /// before their references, every failure returns without committing so the whole scope rolls back, and the run
    /// never cleans up by hand, never continues past a failure and never commits per rack or per view.
    /// </summary>
    public static class RackProjectionMaterializationRun
    {
        public static RackProjectionMaterializationResult Execute(
            IRackProjectionWriteScopeFactory scopes,
            IReadOnlyList<RackProjectionMaterializationUnit> units)
        {
            if (scopes == null) throw new ArgumentNullException(nameof(scopes));
            if (units == null) throw new ArgumentNullException(nameof(units));

            IRackProjectionWriteScope scope;
            try
            {
                scope = scopes.Begin();
            }
            catch (Exception ex)
            {
                return RolledBack(RackProjectionWriteFailure.ScopeUnavailable, null, ex.Message, 0, 0);
            }

            if (scope == null)
                return RolledBack(RackProjectionWriteFailure.ScopeUnavailable, null, "No write scope.", 0, 0);

            var definitions = 0;
            var references = 0;
            using (scope)
            {
                try
                {
                    foreach (var unit in units)
                    {
                        var definition = scope.CreateDefinition(unit.View);
                        if (!definition.IsSuccess)
                        {
                            return RolledBack(
                                definition.Failure == RackProjectionWriteFailure.None
                                    ? RackProjectionWriteFailure.DefinitionCreationFailed
                                    : definition.Failure,
                                unit.Group.RackId,
                                definition.Diagnostic,
                                definitions,
                                references);
                        }

                        definitions++;
                        foreach (var placement in unit.Placements)
                        {
                            var reference = scope.PlaceReference(definition, placement);
                            if (!reference.IsSuccess)
                            {
                                return RolledBack(
                                    RackProjectionWriteFailure.ReferencePlacementFailed,
                                    unit.Group.RackId,
                                    reference.Diagnostic,
                                    definitions,
                                    references);
                            }

                            references++;
                        }
                    }

                    scope.Commit();
                }
                catch (Exception ex)
                {
                    return RolledBack(
                        RackProjectionWriteFailure.UnexpectedException, null, ex.Message, definitions, references);
                }
            }

            return new RackProjectionMaterializationResult(
                RackProjectionMaterializationStatus.Committed,
                RackProjectionWriteFailure.None,
                null,
                null,
                definitions,
                references);
        }

        private static RackProjectionMaterializationResult RolledBack(
            RackProjectionWriteFailure failure, string rackId, string diagnostic, int definitions, int references)
            => new RackProjectionMaterializationResult(
                RackProjectionMaterializationStatus.RolledBack, failure, rackId, diagnostic, definitions, references);
    }
}
