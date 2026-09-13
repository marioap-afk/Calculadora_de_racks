using System;
using System.Collections.Generic;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Dynamic;

namespace RackCad.Application.Systems.Dynamic
{
    /// <summary>
    /// Why a Dinamico cabecera batch ended BEFORE PREPARE (I-53; contract §3.3, §3.8). It is not a rejection code: a gesture
    /// that ends here has no plan and no outcome. The Dinamico recomposes synchronously, so a resolved system is the whole
    /// precondition.
    /// </summary>
    public enum DynamicHeaderPreconditionFailure
    {
        /// <summary>There is no resolved system to read: none was built, or the last build failed.</summary>
        NoResolvedSystem = 1,
    }

    /// <summary>
    /// The result of <see cref="DynamicHeaderBatch.Prepare"/> (I-53): the shared plan of the Dinamico plus what only the
    /// Dinamico needs in order to apply it.
    /// <para>
    /// <see cref="Plan"/> is the shared <see cref="HeaderBatchPlan{TAddress}"/> —addresses, omissions and signature— and it is
    /// null only when the gesture ended before PREPARE (<see cref="PreconditionFailure"/>). The prepared COPIES live here, not
    /// in the shared plan, which carries no configuration: copy <c>i</c> belongs to <c>Plan.Targets[i]</c>, one independent
    /// materialization per destination, NOT normalized — the Dinamico normalizes in its recompute. An EDIT also carries the
    /// manual length its rule precomputed for that destination.
    /// </para>
    /// <para>
    /// A preparation lives ONE gesture (contract §3.9): it belongs to the state and to the resolved system it was prepared on,
    /// and the first MUTATE consumes it whatever the outcome. Another gesture prepares again.
    /// </para>
    /// </summary>
    public sealed class DynamicHeaderBatchPreparation
    {
        private bool consumed;

        private DynamicHeaderBatchPreparation(
            DynamicHeaderBatchState state,
            DynamicRackSystem system,
            DynamicHeaderBatchOperation operation,
            DynamicHeaderPreconditionFailure? preconditionFailure,
            HeaderBatchPlan<DynamicHeaderAddress> plan,
            IReadOnlyList<RackFrameConfiguration> preparedCopies,
            IReadOnlyList<double?> editLengths)
        {
            State = state;
            System = system;
            Operation = operation;
            PreconditionFailure = preconditionFailure;
            Plan = plan;
            PreparedCopies = preparedCopies;
            EditLengths = editLengths;
        }

        /// <summary>Why the gesture ended before PREPARE; null whenever there is a plan.</summary>
        public DynamicHeaderPreconditionFailure? PreconditionFailure { get; }

        /// <summary>The shared plan: <c>Rejected</c> or <c>Prepared</c>. Null only when <see cref="PreconditionFailure"/> is set.</summary>
        public HeaderBatchPlan<DynamicHeaderAddress> Plan { get; }

        public DynamicHeaderBatchOperation Operation { get; }

        /// <summary>The state this batch was prepared against, and the only one it may be applied with.</summary>
        internal DynamicHeaderBatchState State { get; }

        /// <summary>The resolved system this batch read, and the only one it may be applied to: a plan does not cross a recompute.</summary>
        internal DynamicRackSystem System { get; }

        /// <summary>The copies, aligned 1:1 with the prepared plan's targets; empty for any other plan.</summary>
        internal IReadOnlyList<RackFrameConfiguration> PreparedCopies { get; }

        /// <summary>
        /// Aligned with the prepared plan's targets: the manual length an EDIT writes at MUTATE, or null to leave the length and
        /// its flags as they are. Always null in a DISTRIBUTE; empty for any other plan.
        /// </summary>
        internal IReadOnlyList<double?> EditLengths { get; }

        /// <summary>Marks the preparation as used by a MUTATE; a second use is a defect.</summary>
        internal void Consume()
        {
            if (consumed)
            {
                throw new InvalidOperationException("Este plan ya se uso: un plan vive un solo gesto.");
            }

            consumed = true;
        }

        internal static DynamicHeaderBatchPreparation Ended(
            DynamicHeaderBatchState state, DynamicHeaderBatchOperation operation, DynamicHeaderPreconditionFailure failure)
            => new DynamicHeaderBatchPreparation(
                state, null, operation, failure, null, Array.Empty<RackFrameConfiguration>(), Array.Empty<double?>());

        internal static DynamicHeaderBatchPreparation Rejected(
            DynamicHeaderBatchState state, DynamicRackSystem system, DynamicHeaderBatchOperation operation, HeaderRejectionCode code)
            => new DynamicHeaderBatchPreparation(
                state,
                system,
                operation,
                null,
                new HeaderBatchPlan<DynamicHeaderAddress>.Rejected(code),
                Array.Empty<RackFrameConfiguration>(),
                Array.Empty<double?>());

        internal static DynamicHeaderBatchPreparation Prepared(
            DynamicHeaderBatchState state,
            DynamicRackSystem system,
            DynamicHeaderBatchOperation operation,
            HeaderBatchPlan<DynamicHeaderAddress>.Prepared plan,
            IReadOnlyList<RackFrameConfiguration> preparedCopies,
            IReadOnlyList<double?> editLengths)
            => new DynamicHeaderBatchPreparation(state, system, operation, null, plan, preparedCopies, editLengths);
    }
}
