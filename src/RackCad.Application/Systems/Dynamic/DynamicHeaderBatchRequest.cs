using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.RackFrames;

namespace RackCad.Application.Systems.Dynamic
{
    /// <summary>The two operations of the one Dinamico protocol (I-53, contract §3.2).</summary>
    public enum DynamicHeaderBatchOperation
    {
        /// <summary>ID6 + ID7: the custom cabecera an address designates, copied to the chosen modules.</summary>
        Distribute = 1,

        /// <summary>«Editar cabecera»: the configurator result on the selected module.</summary>
        Edit = 2,
    }

    /// <summary>
    /// What the user asked for, before anything is resolved (I-53, contract §3.1-§3.3, §7.4, §7.5). It carries addresses and
    /// the signatures they were taken with, never a live module. Only an EDIT carries a configuration: the configurator
    /// result, handed over as Application data — opening the configurator is the window's job.
    /// <para>
    /// EDIT is the degenerate case of DISTRIBUTE: the same sequence, copy and outcome, with a source that has no address and
    /// therefore no <c>IsSource</c> omission, and the selected module as its only destination.
    /// </para>
    /// </summary>
    public sealed class DynamicHeaderBatchRequest
    {
        private DynamicHeaderBatchRequest(
            DynamicHeaderBatchOperation operation,
            DynamicHeaderSource source,
            RackFrameConfiguration editResult,
            DynamicModuleTargetMode targetMode,
            string[] targetModuleIds,
            HeaderBatchSignature targetSignature,
            string selectedModuleId)
        {
            Operation = operation;
            Source = source;
            EditResult = editResult;
            TargetMode = targetMode;
            TargetModuleIds = Array.AsReadOnly(targetModuleIds);
            TargetSignature = targetSignature;
            SelectedModuleId = selectedModuleId;
        }

        /// <summary>
        /// DISTRIBUTE from the remembered <paramref name="source"/> to <paramref name="targets"/> as they stand now: their mode,
        /// their explicit ids and the signature those were chosen on. <paramref name="selectedModuleId"/> is the editor's
        /// selection, the destination of «Actual».
        /// </summary>
        public static DynamicHeaderBatchRequest Distribute(DynamicHeaderSource source, DynamicModuleTargets targets, string selectedModuleId)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            return new DynamicHeaderBatchRequest(
                DynamicHeaderBatchOperation.Distribute,
                source,
                null,
                targets.Mode,
                targets.ExplicitModuleIds.ToArray(),
                targets.ExplicitSignature,
                selectedModuleId);
        }

        /// <summary>
        /// EDIT with the configurator <paramref name="result"/> on the selected module. The result is captured when the batch
        /// is prepared; later changes to that instance do not reach the copy.
        /// </summary>
        public static DynamicHeaderBatchRequest Edit(RackFrameConfiguration result, string selectedModuleId)
            => new DynamicHeaderBatchRequest(
                DynamicHeaderBatchOperation.Edit,
                null,
                result,
                DynamicModuleTargetMode.FollowCurrent,
                Array.Empty<string>(),
                null,
                selectedModuleId);

        public DynamicHeaderBatchOperation Operation { get; }

        /// <summary>The remembered source of a DISTRIBUTE; null for an EDIT.</summary>
        public DynamicHeaderSource Source { get; }

        public DynamicModuleTargetMode TargetMode { get; }

        /// <summary>The explicit ids as chosen; empty unless <see cref="TargetMode"/> is Explicit.</summary>
        public IReadOnlyList<string> TargetModuleIds { get; }

        /// <summary>The signature the explicit ids were chosen on; null unless <see cref="TargetMode"/> is Explicit.</summary>
        public HeaderBatchSignature TargetSignature { get; }

        /// <summary>The module selected in the editor: the destination of «Actual» and of an EDIT.</summary>
        public string SelectedModuleId { get; }

        /// <summary>The configurator result of an EDIT; null for a DISTRIBUTE.</summary>
        internal RackFrameConfiguration EditResult { get; }
    }
}
