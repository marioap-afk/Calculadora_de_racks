#nullable enable
using System;
using System.Collections.Generic;
using RackCad.Application.Catalogs;
using RackCad.Application.Views.Redraw;

namespace RackCad.Application.Systems.PushBack.Pilot
{
    /// <summary>
    /// I-64 F4-T4B (D-T4B-02) — the port of the update of the bounded Push Back pilot. It works by SCOPES, one per run: the port only
    /// opens a scope for one request, and everything that touches the drawing lives inside the scope that the host adapter hands back.
    /// Skeleton of the RED step: the shape exists and every body is inert.
    /// </summary>
    public interface IPushBackPilotUpdatePort
    {
        /// <summary>Opens the scope of ONE run. Every call gives a NEW scope.</summary>
        IPushBackPilotUpdateScope Begin(PushBackPilotUpdateRequest request);
    }

    /// <summary>The scope of one run: PREPARE (the read and the import), the start of MUTATE and POST.</summary>
    public interface IPushBackPilotUpdateScope : IDisposable
    {
        PushBackPilotScanValues ReadForPrepare();

        PushBackPilotImportResult PrepareUnits(PushBackPilotUpdatePlan plan);

        PushBackPilotMutationStart BeginMutation();

        PushBackPilotPostValues Post(PushBackPilotPostRequest request);
    }

    /// <summary>The ONE owned scope of MUTATE. Releasing it without a <see cref="Seal"/> is an abort.</summary>
    public interface IPushBackPilotMutationScope : IDisposable
    {
        PushBackPilotScanValues Reread();

        PushBackPilotUnitResult WriteUnit(string memberKey, string envelopeText);

        void Seal();
    }

    /// <summary>One run asked of the port: the identity of the document that the gate authorized and the rack.</summary>
    public sealed record PushBackPilotUpdateRequest(long AuthorizedDocumentNativeId, string RackId);

    /// <summary>What one scan of the drawing gave, as plain values.</summary>
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
            Facts = facts;
            RawEnvelopes = rawEnvelopes;
            LockedLayerKeys = lockedLayerKeys ?? Array.Empty<string>();
            Catalog = catalog;
            PreparedBlockMatches = preparedBlockMatches;
        }

        public bool DocumentMatches { get; }

        public IReadOnlyList<RackSiblingScanFact> Facts { get; }

        public IReadOnlyDictionary<string, string> RawEnvelopes { get; }

        public IReadOnlyCollection<string> LockedLayerKeys { get; }

        public RackCatalog? Catalog { get; }

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

        public string? Cause { get; }

        public static PushBackPilotImportResult Succeeded() => new(false, null);

        public static PushBackPilotImportResult Failed(string cause) => new(false, null);
    }

    /// <summary>The answer of the start of MUTATE: the owned scope, or the code of a rejection.</summary>
    public sealed class PushBackPilotMutationStart
    {
        private PushBackPilotMutationStart(IPushBackPilotMutationScope? scope, string? rejectionCode)
        {
            Scope = scope;
            RejectionCode = rejectionCode;
        }

        public IPushBackPilotMutationScope? Scope { get; }

        public string? RejectionCode { get; }

        public static PushBackPilotMutationStart Started(IPushBackPilotMutationScope scope) => new(null, null);

        public static PushBackPilotMutationStart Rejected(string rejectionCode) => new(null, null);
    }

    /// <summary>What one write of a unit found: the pieces that the plan references and the drawing does not have.</summary>
    public sealed class PushBackPilotUnitResult
    {
        public PushBackPilotUnitResult(IReadOnlyList<string> missingPieces)
        {
            MissingPieces = missingPieces;
        }

        public IReadOnlyList<string> MissingPieces { get; }
    }

    /// <summary>What POST needs to be told: the name that each view must carry, by key.</summary>
    public sealed class PushBackPilotPostRequest
    {
        public PushBackPilotPostRequest(IReadOnlyDictionary<string, string> desiredNames)
        {
            DesiredNames = desiredNames;
        }

        public IReadOnlyDictionary<string, string> DesiredNames { get; }
    }

    /// <summary>The FACTS of POST, never warnings.</summary>
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
            NamesByKey = namesByKey ?? new Dictionary<string, string>();
            RegenFailure = regenFailure;
        }

        public bool ForeignScopeOpenAtPost { get; }

        public PushBackPilotScanValues? Scan { get; }

        public IReadOnlyDictionary<string, string> NamesByKey { get; }

        public string? RegenFailure { get; }
    }
}
