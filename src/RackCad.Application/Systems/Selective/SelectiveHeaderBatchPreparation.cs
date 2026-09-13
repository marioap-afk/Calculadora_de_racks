using System;
using System.Collections.Generic;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.RackFrames;

namespace RackCad.Application.Systems.Selective
{
    /// <summary>
    /// Why a Selectivo cabecera batch ended BEFORE PREPARE (I-53, RR-01; contract §3.3, §3.11). It is not a rejection code:
    /// a gesture that ends here has no plan and no outcome.
    /// </summary>
    public enum SelectiveHeaderPreconditionFailure
    {
        /// <summary>There is no resolved system to read: none was built, or the last build failed.</summary>
        NoResolvedSystem = 1,

        /// <summary>A recompute is pending or deferred, so the resolved system may not describe the committed state.</summary>
        ResolutionNotCurrent = 2,
    }

    /// <summary>
    /// The result of <see cref="SelectiveHeaderBatchPlanner.Prepare"/> (I-53): the shared plan of the Selectivo plus what only
    /// the Selectivo needs in order to commit it.
    /// <para>
    /// <see cref="Plan"/> is the shared <see cref="HeaderBatchPlan{TAddress}"/> — addresses, omissions, warnings and signature
    /// — and it is null only when the gesture ended before PREPARE (<see cref="PreconditionFailure"/>). The prepared COPIES
    /// live here, not in the shared plan, which carries no configuration: copy <c>i</c> belongs to <c>Plan.Targets[i]</c>, one
    /// independent materialization per destination, already normalized, handed to
    /// <see cref="SelectiveEditorState.ApplyHeaderBatch"/> exactly as it is.
    /// </para>
    /// <para>
    /// A preparation lives ONE gesture (contract §3.9): it belongs to the state that prepared it, and the first MUTATE or
    /// cancellation consumes it, whatever the outcome. Another gesture prepares again.
    /// </para>
    /// </summary>
    public sealed class SelectiveHeaderBatchPreparation
    {
        private bool consumed;

        private SelectiveHeaderBatchPreparation(
            SelectiveEditorState state,
            SelectiveHeaderBatchOperation operation,
            SelectiveHeaderPreconditionFailure? preconditionFailure,
            HeaderBatchPlan<SelectiveHeaderAddress> plan,
            IReadOnlyList<RackFrameConfiguration> preparedCopies,
            int editPost,
            double editPostPeralte)
        {
            State = state;
            Operation = operation;
            PreconditionFailure = preconditionFailure;
            Plan = plan;
            PreparedCopies = preparedCopies;
            EditPost = editPost;
            EditPostPeralte = editPostPeralte;
        }

        /// <summary>Why the gesture ended before PREPARE; null whenever there is a plan.</summary>
        public SelectiveHeaderPreconditionFailure? PreconditionFailure { get; }

        /// <summary>The shared plan: <c>Rejected</c> or <c>Prepared</c>. Null only when <see cref="PreconditionFailure"/> is set.</summary>
        public HeaderBatchPlan<SelectiveHeaderAddress> Plan { get; }

        public SelectiveHeaderBatchOperation Operation { get; }

        /// <summary>The state this batch was prepared against, and the only one it may be applied to.</summary>
        internal SelectiveEditorState State { get; }

        /// <summary>The normalized copies, aligned 1:1 with the prepared plan's targets; empty for any other plan.</summary>
        internal IReadOnlyList<RackFrameConfiguration> PreparedCopies { get; }

        /// <summary>The visible post of an EDIT; -1 for a DISTRIBUTE.</summary>
        internal int EditPost { get; }

        /// <summary>The per-post peralte an EDIT writes at MUTATE (0 = inherit the run peralte).</summary>
        internal double EditPostPeralte { get; }

        /// <summary>
        /// The user did not confirm a plan whose severe warning asked for confirmation: <c>Cancelled</c>, with zero mutation.
        /// Only a prepared plan can be cancelled, and only one that required confirmation (the shared outcome enforces it).
        /// </summary>
        public HeaderBatchOutcome<SelectiveHeaderAddress> Cancel()
        {
            EnsureNotConsumed();
            if (!(Plan is HeaderBatchPlan<SelectiveHeaderAddress>.Prepared prepared))
            {
                throw new InvalidOperationException("Solo puede cancelarse un plan preparado.");
            }

            var outcome = new HeaderBatchOutcome<SelectiveHeaderAddress>.Cancelled(prepared);
            consumed = true;
            return outcome;
        }

        /// <summary>Marks the preparation as used by a MUTATE; a second use is a defect.</summary>
        internal void Consume()
        {
            EnsureNotConsumed();
            consumed = true;
        }

        private void EnsureNotConsumed()
        {
            if (consumed)
            {
                throw new InvalidOperationException("Este plan ya se uso: un plan vive un solo gesto.");
            }
        }

        internal static SelectiveHeaderBatchPreparation Ended(
            SelectiveEditorState state, SelectiveHeaderBatchOperation operation, SelectiveHeaderPreconditionFailure failure)
            => new SelectiveHeaderBatchPreparation(state, operation, failure, null, Array.Empty<RackFrameConfiguration>(), -1, 0.0);

        internal static SelectiveHeaderBatchPreparation Rejected(
            SelectiveEditorState state, SelectiveHeaderBatchOperation operation, HeaderRejectionCode code)
            => new SelectiveHeaderBatchPreparation(
                state,
                operation,
                null,
                new HeaderBatchPlan<SelectiveHeaderAddress>.Rejected(code),
                Array.Empty<RackFrameConfiguration>(),
                -1,
                0.0);

        internal static SelectiveHeaderBatchPreparation Prepared(
            SelectiveEditorState state,
            SelectiveHeaderBatchOperation operation,
            HeaderBatchPlan<SelectiveHeaderAddress>.Prepared plan,
            IReadOnlyList<RackFrameConfiguration> preparedCopies,
            int editPost,
            double editPostPeralte)
            => new SelectiveHeaderBatchPreparation(state, operation, null, plan, preparedCopies, editPost, editPostPeralte);
    }
}
