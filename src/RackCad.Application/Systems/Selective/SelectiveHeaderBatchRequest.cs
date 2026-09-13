using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.RackFrames;

namespace RackCad.Application.Systems.Selective
{
    /// <summary>The two operations of the one Selectivo protocol (I-53, contract §3.2).</summary>
    public enum SelectiveHeaderBatchOperation
    {
        /// <summary>ID6 + ID7: an existing custom cabecera, designated by its address, copied to posts x target fondos.</summary>
        Distribute = 1,

        /// <summary>«Personalizar»: the configurator result on the visible post x target fondos, plus that post's peralte.</summary>
        Edit = 2,
    }

    /// <summary>
    /// What the user asked for, before anything is resolved (I-53, contract §3.1-§3.3). The fondos are not here: they are
    /// the editor's <see cref="SelectiveEditorState.TargetFondos"/>, read when the batch is prepared, so the editor keeps
    /// ONE grammar of fondos (I-43).
    /// <para>
    /// EDIT is the degenerate case of DISTRIBUTE: the same sequence, copy and outcome, with a source that has no address and
    /// therefore no <c>IsSource</c> omission.
    /// </para>
    /// </summary>
    public sealed class SelectiveHeaderBatchRequest
    {
        private SelectiveHeaderBatchRequest(
            SelectiveHeaderBatchOperation operation,
            SelectiveHeaderAddress? sourceAddress,
            RackFrameConfiguration editResult,
            int[] targetPosts)
        {
            Operation = operation;
            SourceAddress = sourceAddress;
            EditResult = editResult;
            TargetPosts = Array.AsReadOnly(targetPosts);
        }

        /// <summary>
        /// DISTRIBUTE from the cabecera at <paramref name="source"/> to <paramref name="targetPosts"/> of every target fondo.
        /// The posts are the post intent as resolved by <see cref="SelectivePostTargets.Resolve"/>; their order and repetitions
        /// do not matter, and a negative post is malformed.
        /// </summary>
        public static SelectiveHeaderBatchRequest Distribute(SelectiveHeaderAddress source, IEnumerable<int> targetPosts)
            => new SelectiveHeaderBatchRequest(
                SelectiveHeaderBatchOperation.Distribute,
                source,
                null,
                (targetPosts ?? Enumerable.Empty<int>()).ToArray());

        /// <summary>EDIT with the configurator <paramref name="result"/> on <paramref name="visiblePost"/> of every target fondo.
        /// The result is captured when the batch is prepared; later changes to that instance do not reach the copies.</summary>
        public static SelectiveHeaderBatchRequest Edit(RackFrameConfiguration result, int visiblePost)
            => new SelectiveHeaderBatchRequest(SelectiveHeaderBatchOperation.Edit, null, result, new[] { visiblePost });

        public SelectiveHeaderBatchOperation Operation { get; }

        /// <summary>The source address of a DISTRIBUTE; null for an EDIT.</summary>
        public SelectiveHeaderAddress? SourceAddress { get; }

        /// <summary>The requested posts, as given.</summary>
        public IReadOnlyList<int> TargetPosts { get; }

        /// <summary>The configurator result of an EDIT; null for a DISTRIBUTE.</summary>
        internal RackFrameConfiguration EditResult { get; }
    }
}
