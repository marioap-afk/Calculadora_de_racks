using System;
using System.Collections.Generic;
using RackCad.Domain.Systems;

namespace RackCad.Application.Persistence
{
    /// <summary>How the inner <c>Design</c> of a linked view-block resolved when a redraw tried to preserve its metadata.</summary>
    public enum InnerSourceOutcome
    {
        /// <summary>The block's own inner design was read and matches the expected kind — use it as the source.</summary>
        Success,

        /// <summary>The inner design could not be read for a BENIGN reason (missing/malformed/legacy) — fall back to the
        /// initiating block's project rather than fabricating metadata.</summary>
        BenignFallback,

        /// <summary>The inner design declares a schema MAJOR this build cannot read — BLOCKING: the edit must abort, never
        /// overwrite it with the current schema.</summary>
        IncompatibleMajor,

        /// <summary>The inner design is a different rack kind than expected (corruption / foreign block) — BLOCKING.</summary>
        WrongKind,
    }

    /// <summary>
    /// The discriminated result of resolving a view-block's inner source project (I-11). Replaces an ambiguous null return
    /// so a caller can PREFLIGHT every linked view and abort the whole edit on a blocking outcome — never silently
    /// overwriting an incompatible or foreign inner design with a fresh one.
    /// </summary>
    public sealed class InnerSourceResolution
    {
        private InnerSourceResolution(InnerSourceOutcome outcome, RackProject project)
        {
            Outcome = outcome;
            Project = project;
        }

        public InnerSourceOutcome Outcome { get; }

        /// <summary>The source project to attach on re-serialize: the block's own (Success) or the initiating project
        /// (BenignFallback). Null for the blocking outcomes.</summary>
        public RackProject Project { get; }

        /// <summary>True when the edit must abort without touching any block (incompatible major or wrong kind).</summary>
        public bool IsBlocking => Outcome == InnerSourceOutcome.IncompatibleMajor || Outcome == InnerSourceOutcome.WrongKind;

        public static InnerSourceResolution Success(RackProject project) => new InnerSourceResolution(InnerSourceOutcome.Success, project);
        public static InnerSourceResolution BenignFallback(RackProject initiating) => new InnerSourceResolution(InnerSourceOutcome.BenignFallback, initiating);
        public static InnerSourceResolution IncompatibleMajor() => new InnerSourceResolution(InnerSourceOutcome.IncompatibleMajor, null);
        public static InnerSourceResolution WrongKind() => new InnerSourceResolution(InnerSourceOutcome.WrongKind, null);
    }

    /// <summary>
    /// The outcome of preflighting the inner designs of ALL linked view-blocks at once (I-11): either an abort (with the
    /// blocking reason and NO resolved sources, so the caller touches no block) or the per-view resolved source projects
    /// aligned with the input order. Pure so the "abort before serializing any view" rule is unit-testable without AutoCAD.
    /// </summary>
    public sealed class InnerSourcePreflightResult
    {
        private InnerSourcePreflightResult(bool aborted, InnerSourceOutcome blockingOutcome, IReadOnlyList<RackProject> resolvedSources)
        {
            Aborted = aborted;
            BlockingOutcome = blockingOutcome;
            ResolvedSources = resolvedSources;
        }

        public bool Aborted { get; }

        /// <summary>The blocking outcome (IncompatibleMajor or WrongKind) — meaningful only when <see cref="Aborted"/>.</summary>
        public InnerSourceOutcome BlockingOutcome { get; }

        /// <summary>The resolved source project per input design, in order; empty when <see cref="Aborted"/>.</summary>
        public IReadOnlyList<RackProject> ResolvedSources { get; }

        internal static InnerSourcePreflightResult Abort(InnerSourceOutcome outcome) => new InnerSourcePreflightResult(true, outcome, Array.Empty<RackProject>());
        internal static InnerSourcePreflightResult Ok(IReadOnlyList<RackProject> resolved) => new InnerSourcePreflightResult(false, default, resolved);
    }
}
