using System;
using System.Collections.Generic;
using System.Linq;

namespace RackCad.Application.Systems.Selective
{
    /// <summary>How the target posts were chosen (I-53, contract §6.3): the post counterpart of <see cref="SelectiveTargetMode"/>.</summary>
    public enum SelectivePostTargetMode
    {
        /// <summary>"Actual": the post selected in the editor, and it keeps following that selection.</summary>
        FollowCurrent,

        /// <summary>A deliberate set of main posts, distinct and ascending.</summary>
        Explicit,

        /// <summary>"Todos": every main post of the master grid, re-expanded against the grid in force.</summary>
        All
    }

    /// <summary>
    /// The POST axis of a cabecera distribution (I-53, contract §6.3): the posts the next DISTRIBUTE is aimed at, crossed
    /// with <see cref="SelectiveEditorState.TargetFondos"/>, whose I-43 contract is reused exactly as it is.
    /// <para>
    /// The grammar is the one the fondos already teach. The universe is the MAIN posts of the master grid,
    /// <c>0..MaxFrenteCount()</c>; the intermediate post of a medio frente is not addressable. "Todos" is a mode rather than
    /// a snapshot, so it re-expands when the grid grows. An explicit set keeps the posts the grid has; after a structural
    /// change (<see cref="SyncPostTargets"/>) it loses the ones the grid no longer has for good — growing back does not
    /// resurrect them — and it falls back to "Actual" when nothing is left. Nothing is clamped and nothing is created.
    /// </para>
    /// <para>
    /// It is runtime state of the editor and nothing else: not persisted, not part of the design or its document, and not
    /// remembered between sessions — a new instance always opens on "Actual". Whether a post exists in each target fondo
    /// is decided when the batch is prepared, where a post a fondo lacks is omitted and reported.
    /// </para>
    /// </summary>
    public sealed class SelectivePostTargets
    {
        private int[] explicitPosts = Array.Empty<int>();

        public SelectivePostTargetMode Mode { get; private set; } = SelectivePostTargetMode.FollowCurrent;

        /// <summary>The explicit posts, distinct and ascending; empty unless <see cref="Mode"/> is Explicit.</summary>
        public IReadOnlyList<int> ExplicitPosts => explicitPosts;

        /// <summary>Choose "Actual": the target is the selected post, and it keeps following it.</summary>
        public void FollowCurrentPost()
        {
            Mode = SelectivePostTargetMode.FollowCurrent;
            explicitPosts = Array.Empty<int>();
        }

        /// <summary>Choose "Todos": every main post of the master grid, whatever the grid is when it is resolved.</summary>
        public void FollowAllPosts()
        {
            Mode = SelectivePostTargetMode.All;
            explicitPosts = Array.Empty<int>();
        }

        /// <summary>
        /// Choose specific posts. A post outside the universe of <paramref name="state"/> is dropped, and a choice that
        /// leaves nothing falls back to "Actual": an operation with no post would silently do nothing.
        /// </summary>
        public void SetTargetPosts(IEnumerable<int> posts, SelectiveEditorState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            var last = state.MaxFrenteCount();
            var valid = (posts ?? Enumerable.Empty<int>())
                .Where(post => post >= 0 && post <= last)
                .Distinct()
                .OrderBy(post => post)
                .ToArray();

            if (valid.Length == 0)
            {
                FollowCurrentPost();
                return;
            }

            Mode = SelectivePostTargetMode.Explicit;
            explicitPosts = valid;
        }

        /// <summary>
        /// Reconcile the choice with the grid after a structural change. An explicit set is PRUNED to the posts the grid
        /// still has, destructively, and becomes "Actual" when nothing survives. "Actual" and "Todos" have nothing to
        /// prune: the first follows the selection and the second is expanded when it is resolved.
        /// </summary>
        public void SyncPostTargets(SelectiveEditorState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (Mode != SelectivePostTargetMode.Explicit)
            {
                return;
            }

            var last = state.MaxFrenteCount();
            var kept = explicitPosts.Where(post => post <= last).ToArray();
            if (kept.Length == 0)
            {
                FollowCurrentPost();
                return;
            }

            explicitPosts = kept;
        }

        /// <summary>
        /// The posts this choice names now: <paramref name="currentPost"/> for "Actual", the master grid of
        /// <paramref name="state"/> for "Todos", the explicit set as it stands for an explicit choice. It is the POST half of
        /// the intent; it does not decide which fondo has which post.
        /// </summary>
        public IReadOnlyList<int> Resolve(SelectiveEditorState state, int currentPost)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            switch (Mode)
            {
                case SelectivePostTargetMode.All:
                    return Enumerable.Range(0, state.MaxFrenteCount() + 1).ToArray();
                case SelectivePostTargetMode.Explicit:
                    return explicitPosts.ToArray();
                default:
                    return new[] { currentPost };
            }
        }
    }
}
