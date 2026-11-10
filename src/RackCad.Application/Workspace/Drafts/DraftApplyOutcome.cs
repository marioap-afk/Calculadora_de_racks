#nullable enable
using System;
using System.Collections.Generic;

namespace RackCad.Application.Workspace.Drafts
{
    /// <summary>Classification of how an Update ended, by frontier (I-64 D-13). Values the caller reports.</summary>
    public enum DraftApplyOutcomeKind
    {
        NotAppliedConflict,
        NotAppliedAdmission,
        NotAppliedPreflight,
        NotAppliedFailure,
        CommitThrew,
        Applied,
        AppliedWithWarnings,
        AppliedPostRereadFailed,
    }

    /// <summary>The outcome of one Update, built only through its factories (RED skeleton: no validation).</summary>
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

        public IReadOnlyList<DraftDifference> Differences { get; }

        public IReadOnlyList<string> Warnings { get; }

        public DraftIntentRead<TState>? PostRead { get; }

        public static DraftApplyOutcome<TState> NotAppliedConflict(IReadOnlyList<DraftDifference> differences) =>
            new(DraftApplyOutcomeKind.NotAppliedConflict, null, differences, Array.Empty<string>(), null);

        public static DraftApplyOutcome<TState> NotAppliedAdmission(string cause) => WithCause(DraftApplyOutcomeKind.NotAppliedAdmission, cause);

        public static DraftApplyOutcome<TState> NotAppliedPreflight(string cause) => WithCause(DraftApplyOutcomeKind.NotAppliedPreflight, cause);

        public static DraftApplyOutcome<TState> NotAppliedFailure(string cause) => WithCause(DraftApplyOutcomeKind.NotAppliedFailure, cause);

        public static DraftApplyOutcome<TState> CommitThrew(string cause) => WithCause(DraftApplyOutcomeKind.CommitThrew, cause);

        public static DraftApplyOutcome<TState> AppliedPostRereadFailed(string cause) => WithCause(DraftApplyOutcomeKind.AppliedPostRereadFailed, cause);

        public static DraftApplyOutcome<TState> Applied(DraftIntentRead<TState> postRead) =>
            new(DraftApplyOutcomeKind.Applied, null, Array.Empty<DraftDifference>(), Array.Empty<string>(), postRead);

        public static DraftApplyOutcome<TState> AppliedWithWarnings(DraftIntentRead<TState> postRead, IReadOnlyList<string> warnings) =>
            new(DraftApplyOutcomeKind.AppliedWithWarnings, null, Array.Empty<DraftDifference>(), warnings, postRead);

        private static DraftApplyOutcome<TState> WithCause(DraftApplyOutcomeKind kind, string cause) =>
            new(kind, cause, Array.Empty<DraftDifference>(), Array.Empty<string>(), null);
    }

    public enum DraftApplyRefusal
    {
        None,
        NotRegistered,
        InvalidStatus,
        PendingText,
        AnotherUpdateInProgress,
    }

    /// <summary>The answer to TryBeginApply: whether it started, and the status the draft had before.</summary>
    public readonly record struct DraftApplyStart(bool IsStarted, DraftStatus PreviousStatus, DraftApplyRefusal Refusal);

    /// <summary>What CompleteApply did: the effective kind, the status afterwards and whether the draft was rebased.</summary>
    public readonly record struct DraftApplyResult(bool Accepted, DraftApplyOutcomeKind Kind, DraftStatus Status, bool Rebased);
}
