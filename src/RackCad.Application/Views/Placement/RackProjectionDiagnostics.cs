using System;
using System.Collections.Generic;
using RackCad.Application.Systems.Shared;

namespace RackCad.Application.Views.Placement
{
    /// <summary>Frozen ID19 pipeline stages (Proposal V5 section 3.4). Order is contractual.</summary>
    public enum RackProjectionStage
    {
        Classify,
        Group,
        AuthorityGates,
        Representative,
        Resolve,
        EditPreflight,
        Available,
        Frames,
        Validate,
        Plans
    }

    /// <summary>Deterministic blocking causes. Every one of them fails before any point is requested.</summary>
    public enum RackProjectionFailureCode
    {
        None,
        BlankRackId,
        UnsupportedMember,
        MixedSourceTypes,
        MultipleSourceDefinitions,
        AuthoredDivergent,
        AuthoredUnreadable,
        PropertiesDivergent,
        PropertiesUnreadable,
        ResolveFailed,
        ResolveBrokenReference,
        ResolveDependencyUnavailable,
        ResolveLegacyUnsupported,
        ResolveUnreadable,
        ResolveOutputBlocking,
        EditPreflightRejected,
        SourceAddressUnavailable,
        TargetAddressUnavailable,
        TargetNotExposed,
        PairNotExposed,
        MixedFamilies,
        FrameUnavailable,
        FrameAxisMissing,
        NonParallelSources,
        SourceFactsNonFinite,
        SourceFactsDegenerate,
        SourceFactsInvalidTolerance,
        ReflectionNotAllowed,
        NonUnitScale,
        NonUniformScale,
        NegativeZScale,
        NormalNotWorldZ,
        PlanUnavailable,
        UnknownSourceRole,
        RequiredKeyMissing,
        RequiredBlockMissing,
        RequiredLibraryMissing,
        RequiredUnknownAvailability
    }

    /// <summary>Non blocking observations. A warning never hides a later blocking failure.</summary>
    public enum RackProjectionWarningCode
    {
        Overlap,
        OptionalVisualMissing,
        DirectionWindowNearLimit
    }

    /// <summary>
    /// One blocking diagnostic. Foundation facts travel untouched; the product layer only adds the remedy.
    /// </summary>
    public sealed class RackProjectionDiagnostic
    {
        public RackProjectionDiagnostic(
            RackProjectionStage stage,
            RackProjectionFailureCode code,
            string rackId,
            string physicalKey,
            RackViewAddress? address,
            RackProjectionRemedy remedy,
            RackProjectionPieceEvidence piece = null)
        {
            Stage = stage;
            Code = code;
            RackId = rackId;
            PhysicalKey = physicalKey;
            Address = address;
            Remedy = remedy;
            Piece = piece;
        }

        public RackProjectionStage Stage { get; }
        public RackProjectionFailureCode Code { get; }
        public string RackId { get; }
        public string PhysicalKey { get; }
        public RackViewAddress? Address { get; }
        public RackProjectionRemedy Remedy { get; }
        public RackProjectionPieceEvidence Piece { get; }
    }

    /// <summary>AUTH-12 V2 facts preserved verbatim inside a diagnostic or warning.</summary>
    public sealed class RackProjectionPieceEvidence
    {
        public RackProjectionPieceEvidence(
            string pieceId,
            RackViewAddress viewAddress,
            RequirementRole role,
            string libraryKey,
            RequirementKeyState keyState,
            LibraryAvailability libraryAvailability,
            LibraryBlockPresence blockPresence)
        {
            PieceId = pieceId;
            ViewAddress = viewAddress;
            Role = role;
            LibraryKey = libraryKey;
            KeyState = keyState;
            LibraryAvailability = libraryAvailability;
            BlockPresence = blockPresence;
        }

        public string PieceId { get; }
        public RackViewAddress ViewAddress { get; }
        public RequirementRole Role { get; }
        public string LibraryKey { get; }
        public RequirementKeyState KeyState { get; }
        public LibraryAvailability LibraryAvailability { get; }
        public LibraryBlockPresence BlockPresence { get; }
    }

    public sealed class RackProjectionWarning
    {
        public RackProjectionWarning(
            RackProjectionWarningCode code,
            string rackId,
            string physicalKey,
            string detail,
            RackProjectionPieceEvidence piece = null)
        {
            Code = code;
            RackId = rackId;
            PhysicalKey = physicalKey;
            Detail = detail;
            Piece = piece;
        }

        public RackProjectionWarningCode Code { get; }
        public string RackId { get; }
        public string PhysicalKey { get; }
        public string Detail { get; }
        public RackProjectionPieceEvidence Piece { get; }
    }

    internal static class RackProjectionOrder
    {
        internal static int CompareKeys(string left, string right) =>
            string.CompareOrdinal(left ?? string.Empty, right ?? string.Empty);

        internal static IReadOnlyList<T> Sorted<T>(IEnumerable<T> items, Func<T, string> key)
        {
            var list = new List<T>(items);
            list.Sort((a, b) => CompareKeys(key(a), key(b)));
            return list;
        }
    }
}
