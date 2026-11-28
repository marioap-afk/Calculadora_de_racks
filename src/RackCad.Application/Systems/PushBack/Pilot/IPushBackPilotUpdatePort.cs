#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Views.Redraw;

namespace RackCad.Application.Systems.PushBack.Pilot
{
    /// <summary>
    /// I-64 F4-T4B (D-T4B-02) — the port of the update of the bounded Push Back pilot. It works by SCOPES, one per run: the port only
    /// opens a scope for one request, and everything that touches the drawing lives inside the scope that the host adapter hands back.
    /// The port itself keeps nothing between runs. It only RETURNS VALUES read from the drawing: it never chooses the origin, never
    /// counts survivors and never decides a warning. All of that is decided, in pure code, by <see cref="PushBackPilotUpdateRun"/>.
    /// </summary>
    public interface IPushBackPilotUpdatePort
    {
        /// <summary>Opens the scope of ONE run. Every call gives a NEW scope.</summary>
        IPushBackPilotUpdateScope Begin(PushBackPilotUpdateRequest request);
    }

    /// <summary>
    /// The scope of one run: PREPARE (the read and the import), the start of MUTATE and POST. The run always releases it, in a
    /// <c>finally</c> of its own, after the scope of MUTATE.
    /// </summary>
    public interface IPushBackPilotUpdateScope : IDisposable
    {
        /// <summary>The read of PREPARE: the facts and the raw text of the whole drawing without an origin, and the catalog.</summary>
        PushBackPilotScanValues ReadForPrepare();

        /// <summary>The import of what the plan draws, BEFORE the scope of MUTATE exists.</summary>
        PushBackPilotImportResult PrepareUnits(PushBackPilotUpdatePlan plan);

        /// <summary>Starts the ONE owned scope of MUTATE, or says why it cannot start. A rejection is a value, never an exception.</summary>
        PushBackPilotMutationStart BeginMutation();

        /// <summary>POST: the tidy-up after the write and the facts of the drawing afterwards. It only returns facts, never warnings.</summary>
        PushBackPilotPostValues Post(PushBackPilotPostRequest request);
    }

    /// <summary>The ONE owned scope of MUTATE. Releasing it without a <see cref="Seal"/> is an abort.</summary>
    public interface IPushBackPilotMutationScope : IDisposable
    {
        /// <summary>The re-read inside the scope that is the authority of MUTATE: it opens nothing and closes nothing.</summary>
        PushBackPilotScanValues Reread();

        /// <summary>Redraws ONE member with the text that the plan holds for that key.</summary>
        PushBackPilotUnitResult WriteUnit(string memberKey, string envelopeText);

        /// <summary>Confirms everything that the units wrote, in one step.</summary>
        void Seal();
    }

    /// <summary>One run asked of the port: the identity of the document that the gate authorized and the rack.</summary>
    public sealed record PushBackPilotUpdateRequest(long AuthorizedDocumentNativeId, string RackId);

    /// <summary>
    /// What one scan of the drawing gave, as plain values. <see cref="Catalog"/> is only set by the read of PREPARE and
    /// <see cref="PreparedBlockMatches"/> only by the re-read of MUTATE.
    /// </summary>
    public sealed class PushBackPilotScanValues
    {
        public PushBackPilotScanValues(
            bool documentMatches,
            IReadOnlyList<RackSiblingScanFact> facts,
            IReadOnlyDictionary<string, string> rawEnvelopes,
            IReadOnlyCollection<string>? lockedLayerKeys = null,
            RackCatalog? catalog = null,
            IReadOnlyDictionary<string, bool>? preparedBlockMatches = null)
        {
            DocumentMatches = documentMatches;
            Facts = Array.AsReadOnly((facts ?? throw new ArgumentNullException(nameof(facts))).ToArray());
            RawEnvelopes = new Dictionary<string, string>(rawEnvelopes ?? throw new ArgumentNullException(nameof(rawEnvelopes)), StringComparer.Ordinal);
            LockedLayerKeys = Array.AsReadOnly((lockedLayerKeys ?? Array.Empty<string>()).ToArray());
            Catalog = catalog;
            PreparedBlockMatches = preparedBlockMatches == null ? null : new Dictionary<string, bool>(preparedBlockMatches, StringComparer.Ordinal);
        }

        /// <summary>False when the document that the scan found is not the one that the request authorized.</summary>
        public bool DocumentMatches { get; }

        /// <summary>The facts of the scan, unclassified (the same ones that the read of the intention consumes).</summary>
        public IReadOnlyList<RackSiblingScanFact> Facts { get; }

        /// <summary>The exact raw text of the envelope of each scanned definition that has text, by key (ordinal).</summary>
        public IReadOnlyDictionary<string, string> RawEnvelopes { get; }

        /// <summary>The keys of the definitions that touch a locked layer, as the scan finds them. The run only looks at the members of the plan.</summary>
        public IReadOnlyCollection<string> LockedLayerKeys { get; }

        /// <summary>The catalog; only the read of PREPARE sets it.</summary>
        public RackCatalog? Catalog { get; }

        /// <summary>By key, whether the block that PREPARE imported into is the one that the re-read found; only the re-read of MUTATE sets it.</summary>
        public IReadOnlyDictionary<string, bool>? PreparedBlockMatches { get; }
    }

    /// <summary>The answer of the import of PREPARE.</summary>
    public sealed class PushBackPilotImportResult
    {
        private PushBackPilotImportResult(bool ok, string? cause)
        {
            Ok = ok;
            Cause = cause;
        }

        public bool Ok { get; }

        /// <summary>Why the import did not happen; null when it did.</summary>
        public string? Cause { get; }

        public static PushBackPilotImportResult Succeeded() => new(true, null);

        public static PushBackPilotImportResult Failed(string cause) => new(false, cause);
    }

    /// <summary>
    /// The answer of the start of MUTATE: the owned scope, or the code of a rejection. A rejection is a value and never an exception. The
    /// only code today is <see cref="PushBackPilotUpdateRun.ForeignScopeOpenCause"/>.
    /// </summary>
    public sealed class PushBackPilotMutationStart
    {
        private PushBackPilotMutationStart(IPushBackPilotMutationScope? scope, string? rejectionCode)
        {
            Scope = scope;
            RejectionCode = rejectionCode;
        }

        public IPushBackPilotMutationScope? Scope { get; }

        public string? RejectionCode { get; }

        public static PushBackPilotMutationStart Started(IPushBackPilotMutationScope scope) => new(scope, null);

        public static PushBackPilotMutationStart Rejected(string rejectionCode) => new(null, rejectionCode);
    }

    /// <summary>What one write of a unit found: the pieces that the plan references and the drawing does not have.</summary>
    public sealed class PushBackPilotUnitResult
    {
        public PushBackPilotUnitResult(IReadOnlyList<string> missingPieces)
        {
            MissingPieces = Array.AsReadOnly((missingPieces ?? throw new ArgumentNullException(nameof(missingPieces))).ToArray());
        }

        public IReadOnlyList<string> MissingPieces { get; }
    }

    /// <summary>
    /// What POST needs to be told: the name that each view must carry, by key, as the rule of the classic editor computes it. A key
    /// without a desired name is not renamed and gives no warning.
    /// </summary>
    public sealed class PushBackPilotPostRequest
    {
        public PushBackPilotPostRequest(IReadOnlyDictionary<string, string> desiredNames)
        {
            DesiredNames = new Dictionary<string, string>(desiredNames ?? throw new ArgumentNullException(nameof(desiredNames)), StringComparer.Ordinal);
        }

        public IReadOnlyDictionary<string, string> DesiredNames { get; }
    }

    /// <summary>
    /// The FACTS of POST, never warnings: whether a foreign scope was open and nothing was done, the scan read AFTER the tidy-up, the
    /// name of the definition of each key read AFTER the rename and the failure of the regeneration, when there was one.
    /// </summary>
    public sealed class PushBackPilotPostValues
    {
        public PushBackPilotPostValues(
            bool foreignScopeOpenAtPost,
            PushBackPilotScanValues? scan,
            IReadOnlyDictionary<string, string>? namesByKey,
            string? regenFailure)
        {
            ForeignScopeOpenAtPost = foreignScopeOpenAtPost;
            Scan = scan;
            NamesByKey = new Dictionary<string, string>(namesByKey ?? new Dictionary<string, string>(), StringComparer.Ordinal);
            RegenFailure = regenFailure;
        }

        public bool ForeignScopeOpenAtPost { get; }

        public PushBackPilotScanValues? Scan { get; }

        public IReadOnlyDictionary<string, string> NamesByKey { get; }

        public string? RegenFailure { get; }
    }
}
