using System;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Policy;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Views.Preparation
{
    public enum RackProductPrepareFailure
    {
        Unknown,
        None,
        UnsupportedIntent,
        PolicyRejected,
        InvalidIdentity,
        AuthoredDivergent,
        AuthoredUnreadable,
        ResolveFailed,
        PrepareFailed,
        EnvelopeFailed
    }

    public enum RackProductSourceKind
    {
        NewRack,
        ExistingRack
    }

    public sealed class RackPreparedProductView<TPayload>
    {
        internal RackPreparedProductView(
            string rackId,
            RackProductSourceKind sourceKind,
            RackViewProductOperation operation,
            RackSystemKind systemKind,
            RackPreparedView<TPayload> prepared,
            RackEmbedDocument envelope)
        {
            RackId = rackId;
            SourceKind = sourceKind;
            Operation = operation;
            SystemKind = systemKind;
            Prepared = prepared;
            Envelope = envelope;
        }

        public string RackId { get; }
        public RackProductSourceKind SourceKind { get; }
        public RackViewProductOperation Operation { get; }
        public RackSystemKind SystemKind { get; }
        public RackPreparedView<TPayload> Prepared { get; }
        public RackEmbedDocument Envelope { get; }
    }

    public readonly struct RackProductPrepareResult<TPayload>
    {
        internal RackProductPrepareResult(
            RackPreparedProductView<TPayload> product,
            RackProductPrepareFailure failure,
            string causeCode,
            string diagnostic,
            ProjectionPolicyDecision policy)
        {
            Product = product;
            Failure = failure;
            CauseCode = causeCode;
            Diagnostic = diagnostic;
            Policy = policy;
        }

        public bool IsSuccess => Product != null && Failure == RackProductPrepareFailure.None;
        public RackPreparedProductView<TPayload> Product { get; }
        public RackProductPrepareFailure Failure { get; }
        public string CauseCode { get; }
        public string Diagnostic { get; }
        public ProjectionPolicyDecision Policy { get; }
    }

    /// <summary>
    /// Repeatable product composition over an already accepted intent. Acceptance owns policy and RackId lifecycle;
    /// this type calls the shared resolve and preparation ports once each and never generates an identity.
    /// </summary>
    public sealed class RackProductPreparer<TAuthored, TResolved, TPayload>
    {
        private readonly IRackResolvePort<TAuthored, TResolved> resolve;
        private readonly IRackViewPreparationPort<TResolved, TPayload> prepare;
        private readonly Func<TResolved, RackViewAddress, RackViewFrameResult> frameAuthority;
        private readonly Func<TResolved, RackViewAddress, string> baseNameAuthority;

        public RackProductPreparer(
            IRackResolvePort<TAuthored, TResolved> resolve,
            IRackViewPreparationPort<TResolved, TPayload> prepare,
            Func<TResolved, RackViewAddress, RackViewFrameResult> frameAuthority,
            Func<TResolved, RackViewAddress, string> baseNameAuthority)
        {
            this.resolve = resolve ?? throw new ArgumentNullException(nameof(resolve));
            this.prepare = prepare ?? throw new ArgumentNullException(nameof(prepare));
            this.frameAuthority = frameAuthority ?? throw new ArgumentNullException(nameof(frameAuthority));
            this.baseNameAuthority = baseNameAuthority ?? throw new ArgumentNullException(nameof(baseNameAuthority));
        }

        public RackProductPrepareResult<TPayload> Prepare(AcceptedNewRackViewIntent<TAuthored> intent)
        {
            if (intent == null)
                return Failed(RackProductPrepareFailure.UnsupportedIntent, "ACCEPTED_INTENT_MISSING", null, default);

            return PrepareAccepted(
                RackProductSourceKind.NewRack,
                intent.View,
                intent.Policy,
                intent.RackId,
                intent.Creation.RackName,
                intent.Creation.SerializedDesign,
                null,
                intent.Creation.Authored);
        }

        public RackProductPrepareResult<TPayload> PrepareExisting<TComparisonInput>(
            AcceptedExistingRackViewIntent<TComparisonInput> intent,
            IRackAuthoredComparatorPort<TComparisonInput, TAuthored> comparator)
        {
            if (intent?.Existing?.SourceEnvelope == null || comparator == null)
                return Failed(RackProductPrepareFailure.UnsupportedIntent, "ACCEPTED_INTENT_MISSING", null, default);

            var source = intent.Existing.SourceEnvelope;
            var expectedKind = intent.Policy.CanonicalSyntax?.Kind;
            if (!string.Equals(comparator.Kind, expectedKind, StringComparison.OrdinalIgnoreCase))
                return Failed(RackProductPrepareFailure.UnsupportedIntent, "COMPARATOR_KIND_MISMATCH", null, intent.Policy);

            RackAuthoredComparisonResult<TAuthored> comparison;
            try
            {
                comparison = comparator.Compare(intent.Existing.ComparisonInput);
            }
            catch (Exception ex)
            {
                return Failed(RackProductPrepareFailure.AuthoredUnreadable,
                    "AUTHORED_COMPARISON_FAILED", ex.Message, intent.Policy);
            }

            switch (comparison.Outcome)
            {
                case RackAuthoredComparisonOutcome.Single:
                    if (ReferenceEquals(comparison.Authored, null))
                        return Failed(RackProductPrepareFailure.AuthoredUnreadable,
                            "AUTHORED_UNREADABLE", comparison.Diagnostic, intent.Policy);
                    break;
                case RackAuthoredComparisonOutcome.Divergent:
                    return Failed(RackProductPrepareFailure.AuthoredDivergent,
                        "AUTHORED_DIVERGENT", comparison.Diagnostic, intent.Policy);
                default:
                    return Failed(RackProductPrepareFailure.AuthoredUnreadable,
                        "AUTHORED_UNREADABLE", comparison.Diagnostic, intent.Policy);
            }

            return PrepareAccepted(
                RackProductSourceKind.ExistingRack,
                intent.View,
                intent.Policy,
                source.Id,
                source.Name,
                source.Design,
                source,
                comparison.Authored);
        }

        private RackProductPrepareResult<TPayload> PrepareAccepted(
            RackProductSourceKind sourceKind,
            RackProductViewRequest view,
            ProjectionPolicyDecision policy,
            string rackId,
            string rackName,
            string serializedDesign,
            RackEmbedDocument sourceEnvelope,
            TAuthored authored)
        {
            if (!RackProductIntentValidation.Valid(view)
                || policy.Decision == ProjectionPolicyDecisionKind.Reject
                || !policy.CanonicalAddress.HasValue)
                return Failed(RackProductPrepareFailure.UnsupportedIntent, "ACCEPTED_INTENT_INVALID", null, policy);

            var expectedKind = policy.CanonicalSyntax?.Kind;
            if (!string.Equals(resolve.Kind, expectedKind, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(prepare.Kind, expectedKind, StringComparison.OrdinalIgnoreCase))
                return Failed(RackProductPrepareFailure.UnsupportedIntent, "PORT_KIND_MISMATCH", null, policy);

            RackResolveResult<TResolved> resolved;
            try
            {
                resolved = resolve.Resolve(authored);
            }
            catch (Exception ex)
            {
                return Failed(RackProductPrepareFailure.ResolveFailed, "RESOLVE_FAILED", ex.Message, policy);
            }

            if (!resolved.IsSuccess)
                return Failed(RackProductPrepareFailure.ResolveFailed, resolved.Code, resolved.Diagnostic, policy);

            RackViewPreparationResult<TPayload> prepared;
            try
            {
                var address = policy.CanonicalAddress.Value;
                prepared = prepare.Prepare(
                    resolved.Resolved,
                    address,
                    frameAuthority(resolved.Resolved, address),
                    baseNameAuthority(resolved.Resolved, address));
            }
            catch (Exception ex)
            {
                return Failed(RackProductPrepareFailure.PrepareFailed, "PREPARE_FAILED", ex.Message, policy);
            }

            if (!prepared.IsSuccess)
                return Failed(RackProductPrepareFailure.PrepareFailed,
                    prepared.Code, prepared.Diagnostic, policy);

            RackEmbedDocument envelope;
            try
            {
                envelope = RackViewEnvelopeComposition.Compose(
                    sourceEnvelope,
                    rackId,
                    rackName,
                    serializedDesign,
                    view.SystemKind,
                    prepared.Prepared);
            }
            catch (Exception ex)
            {
                return Failed(RackProductPrepareFailure.EnvelopeFailed,
                    "ENVELOPE_COMPOSITION_FAILED", ex.Message, policy);
            }

            return new RackProductPrepareResult<TPayload>(
                new RackPreparedProductView<TPayload>(
                    rackId, sourceKind, view.Operation, view.SystemKind, prepared.Prepared, envelope),
                RackProductPrepareFailure.None,
                null,
                null,
                policy);
        }

        private static RackProductPrepareResult<TPayload> Failed(
            RackProductPrepareFailure failure,
            string causeCode,
            string diagnostic,
            ProjectionPolicyDecision policy)
            => new RackProductPrepareResult<TPayload>(null, failure, causeCode, diagnostic, policy);
    }
}
