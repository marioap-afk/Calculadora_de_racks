#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace RackCad.Application.Workspace.Drafts
{
    /// <summary>
    /// Classification of how an Update ended, by frontier (I-64 D-13). These are values the caller reports: the core
    /// reads nothing and writes nothing.
    /// </summary>
    public enum DraftApplyOutcomeKind
    {
        /// <summary>Failed before the commit: the re-read inside the write found an external change. The draft goes to Conflict.</summary>
        NotAppliedConflict,

        /// <summary>Failed before the commit: the admission no longer holds. The draft keeps its previous status.</summary>
        NotAppliedAdmission,

        /// <summary>Failed before the commit: a preflight check refused. The draft keeps its previous status.</summary>
        NotAppliedPreflight,

        /// <summary>Failed before the commit for any other reason. The draft keeps its previous status.</summary>
        NotAppliedFailure,

        /// <summary>The commit threw: nothing is known about what was written. The draft is Unknown (origin CommitThrew).</summary>
        CommitThrew,

        /// <summary>The commit returned and the re-read of POST succeeded: rebased and Clean.</summary>
        Applied,

        /// <summary>As Applied, but POST failed or reported missing pieces: rebased and Clean, with the warnings kept.</summary>
        AppliedWithWarnings,

        /// <summary>The commit returned and the re-read of POST failed: Unknown (origin PostReread).</summary>
        AppliedPostRereadFailed,
    }

    /// <summary>The outcome of one Update, built only through its factories.</summary>
    public sealed class DraftApplyOutcome<TState>
    {
        private DraftApplyOutcome(
            DraftApplyOutcomeKind kind,
            string? cause,
            IReadOnlyList<DraftDifference> differences,
            IReadOnlyList<string> warnings,
            DraftIntentRead<TState>? postRead)
        {
            Kind = kind;
            Cause = cause;
            Differences = differences;
            Warnings = warnings;
            PostRead = postRead;
        }

        public DraftApplyOutcomeKind Kind { get; }

        public string? Cause { get; }

        /// <summary>What changed, for NotAppliedConflict.</summary>
        public IReadOnlyList<DraftDifference> Differences { get; }

        /// <summary>The warnings of AppliedWithWarnings.</summary>
        public IReadOnlyList<string> Warnings { get; }

        /// <summary>The re-read of POST, which Applied and AppliedWithWarnings require.</summary>
        public DraftIntentRead<TState>? PostRead { get; }

        public static DraftApplyOutcome<TState> NotAppliedConflict(IReadOnlyList<DraftDifference> differences)
        {
            if (differences == null) throw new ArgumentNullException(nameof(differences));
            // A copy taken on receipt: changing the caller's list afterwards changes nothing here.
            return new(DraftApplyOutcomeKind.NotAppliedConflict, null, differences.ToArray(), Array.Empty<string>(), null);
        }

        public static DraftApplyOutcome<TState> NotAppliedAdmission(string cause) => WithCause(DraftApplyOutcomeKind.NotAppliedAdmission, cause);

        public static DraftApplyOutcome<TState> NotAppliedPreflight(string cause) => WithCause(DraftApplyOutcomeKind.NotAppliedPreflight, cause);

        public static DraftApplyOutcome<TState> NotAppliedFailure(string cause) => WithCause(DraftApplyOutcomeKind.NotAppliedFailure, cause);

        public static DraftApplyOutcome<TState> CommitThrew(string cause) => WithCause(DraftApplyOutcomeKind.CommitThrew, cause);

        public static DraftApplyOutcome<TState> AppliedPostRereadFailed(string cause) => WithCause(DraftApplyOutcomeKind.AppliedPostRereadFailed, cause);

        /// <summary>Applied: the re-read of POST is required, since nothing is assumed applied without it.</summary>
        public static DraftApplyOutcome<TState> Applied(DraftIntentRead<TState> postRead)
        {
            if (postRead == null) throw new ArgumentNullException(nameof(postRead));
            return new(DraftApplyOutcomeKind.Applied, null, Array.Empty<DraftDifference>(), Array.Empty<string>(), postRead);
        }

        public static DraftApplyOutcome<TState> AppliedWithWarnings(DraftIntentRead<TState> postRead, IReadOnlyList<string> warnings)
        {
            if (postRead == null) throw new ArgumentNullException(nameof(postRead));
            if (warnings == null) throw new ArgumentNullException(nameof(warnings));
            // A copy taken on receipt: changing the caller's list afterwards changes nothing here.
            return new(DraftApplyOutcomeKind.AppliedWithWarnings, null, Array.Empty<DraftDifference>(), warnings.ToArray(), postRead);
        }

        private static DraftApplyOutcome<TState> WithCause(DraftApplyOutcomeKind kind, string cause)
        {
            if (string.IsNullOrEmpty(cause)) throw new ArgumentException("A cause is required.", nameof(cause));
            return new(kind, cause, Array.Empty<DraftDifference>(), Array.Empty<string>(), null);
        }
    }

    public enum DraftApplyRefusal
    {
        None,

        /// <summary>The draft was discarded or its session retired.</summary>
        NotRegistered,

        /// <summary>The draft is not Clean or Dirty (Conflict, Applying, Unknown or Error).</summary>
        InvalidStatus,

        /// <summary>There is pending text: the Update boundary must commit it first.</summary>
        PendingText,

        /// <summary>Another draft of the registry is already Applying.</summary>
        AnotherUpdateInProgress,
    }

    /// <summary>The answer to TryBeginApply: whether it started, and the status the draft had before.</summary>
    public readonly record struct DraftApplyStart(bool IsStarted, DraftStatus PreviousStatus, DraftApplyRefusal Refusal)
    {
        internal static DraftApplyStart Started(DraftStatus previous) => new(true, previous, DraftApplyRefusal.None);

        internal static DraftApplyStart Refused(DraftStatus previous, DraftApplyRefusal refusal) => new(false, previous, refusal);
    }

    /// <summary>
    /// What CompleteApply did: whether the outcome was accepted, the effective kind, the status afterwards, whether the
    /// draft was rebased and whether the draft was still registered. An outcome is accepted (and frees the Applying slot)
    /// even for a draft that was discarded or whose session was retired meanwhile: then <see cref="DraftWasRegistered"/>
    /// is false and nothing else changed.
    /// </summary>
    public readonly record struct DraftApplyResult(bool Accepted, DraftApplyOutcomeKind Kind, DraftStatus Status, bool Rebased, bool DraftWasRegistered)
    {
        internal static DraftApplyResult Closed(DraftApplyOutcomeKind kind, DraftStatus status, bool rebased) => new(true, kind, status, rebased, true);

        internal static DraftApplyResult ClosedDetached(DraftApplyOutcomeKind kind, DraftStatus status) => new(true, kind, status, false, false);

        internal static DraftApplyResult NotApplying(DraftStatus status, DraftApplyOutcomeKind kind) => new(false, kind, status, false, true);
    }
}
