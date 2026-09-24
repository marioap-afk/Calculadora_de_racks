using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Drawing;

namespace RackCad.Application.Systems.Shared
{
    public enum RequirementRole
    {
        Required,
        OptionalVisual,
        NotApplicable
    }

    public enum RackHeaderBlockRequirementClassificationOutcome
    {
        Classified,
        UnknownSourceRole
    }

    public readonly struct RackHeaderBlockRequirementRoleResult
    {
        private RackHeaderBlockRequirementRoleResult(
            RackHeaderBlockRequirementClassificationOutcome outcome,
            RequirementRole role)
        {
            Outcome = outcome;
            Role = role;
        }

        public RackHeaderBlockRequirementClassificationOutcome Outcome { get; }
        public bool HasRole => Outcome == RackHeaderBlockRequirementClassificationOutcome.Classified;
        public RequirementRole Role { get; }

        internal static RackHeaderBlockRequirementRoleResult Classified(RequirementRole role)
            => new RackHeaderBlockRequirementRoleResult(RackHeaderBlockRequirementClassificationOutcome.Classified, role);

        internal static RackHeaderBlockRequirementRoleResult Unknown()
            => new RackHeaderBlockRequirementRoleResult(RackHeaderBlockRequirementClassificationOutcome.UnknownSourceRole, default);
    }

    public static class RackHeaderBlockRequirementRoleClassifier
    {
        public static RackHeaderBlockRequirementRoleResult Classify(HeaderBlockRole source)
        {
            switch (source)
            {
                case HeaderBlockRole.BasePlate:
                    return RackHeaderBlockRequirementRoleResult.Classified(RequirementRole.Required);
                case HeaderBlockRole.Post:
                    return RackHeaderBlockRequirementRoleResult.Classified(RequirementRole.Required);
                case HeaderBlockRole.Horizontal:
                    return RackHeaderBlockRequirementRoleResult.Classified(RequirementRole.Required);
                case HeaderBlockRole.Diagonal:
                    return RackHeaderBlockRequirementRoleResult.Classified(RequirementRole.Required);
                case HeaderBlockRole.ClosingHorizontal:
                    return RackHeaderBlockRequirementRoleResult.Classified(RequirementRole.Required);
                case HeaderBlockRole.Separator:
                    return RackHeaderBlockRequirementRoleResult.Classified(RequirementRole.Required);
                case HeaderBlockRole.Rail:
                    return RackHeaderBlockRequirementRoleResult.Classified(RequirementRole.Required);
                case HeaderBlockRole.Roller:
                    return RackHeaderBlockRequirementRoleResult.Classified(RequirementRole.Required);
                case HeaderBlockRole.Brake:
                    return RackHeaderBlockRequirementRoleResult.Classified(RequirementRole.Required);
                case HeaderBlockRole.Stop:
                    return RackHeaderBlockRequirementRoleResult.Classified(RequirementRole.Required);
                case HeaderBlockRole.Beam:
                    return RackHeaderBlockRequirementRoleResult.Classified(RequirementRole.Required);
                case HeaderBlockRole.Annotation:
                    return RackHeaderBlockRequirementRoleResult.Classified(RequirementRole.NotApplicable);
                case HeaderBlockRole.Dimension:
                    return RackHeaderBlockRequirementRoleResult.Classified(RequirementRole.NotApplicable);
                case HeaderBlockRole.Safety:
                    return RackHeaderBlockRequirementRoleResult.Classified(RequirementRole.Required);
                case HeaderBlockRole.Tope:
                    return RackHeaderBlockRequirementRoleResult.Classified(RequirementRole.Required);
                case HeaderBlockRole.Pallet:
                    return RackHeaderBlockRequirementRoleResult.Classified(RequirementRole.OptionalVisual);
                default:
                    return RackHeaderBlockRequirementRoleResult.Unknown();
            }
        }
    }

    public enum RequirementKeyState
    {
        Present,
        KeyMissing
    }

    public sealed class LibraryPieceRequirement
    {
        public LibraryPieceRequirement(
            string pieceId,
            RackViewAddress viewAddress,
            RequirementRole role,
            string libraryKey)
        {
            PieceId = pieceId;
            ViewAddress = viewAddress;
            Role = role;
            LibraryKey = libraryKey;
            KeyState = string.IsNullOrWhiteSpace(libraryKey)
                ? RequirementKeyState.KeyMissing
                : RequirementKeyState.Present;
        }

        public string PieceId { get; }
        public RackViewAddress ViewAddress { get; }
        public RequirementRole Role { get; }
        public string LibraryKey { get; }
        public RequirementKeyState KeyState { get; }
    }

    public enum PieceRequirementExtractionOutcome
    {
        Extracted,
        UnknownSourceRole
    }

    public sealed class PieceRequirementExtractionResult
    {
        private PieceRequirementExtractionResult(
            PieceRequirementExtractionOutcome outcome,
            IReadOnlyList<LibraryPieceRequirement> requirements)
        {
            Outcome = outcome;
            Requirements = requirements;
        }

        public PieceRequirementExtractionOutcome Outcome { get; }
        public bool IsSuccess => Outcome == PieceRequirementExtractionOutcome.Extracted;
        public IReadOnlyList<LibraryPieceRequirement> Requirements { get; }

        internal static PieceRequirementExtractionResult Success(IReadOnlyList<LibraryPieceRequirement> requirements)
            => new PieceRequirementExtractionResult(PieceRequirementExtractionOutcome.Extracted, requirements);

        internal static PieceRequirementExtractionResult UnknownSourceRole()
            => new PieceRequirementExtractionResult(
                PieceRequirementExtractionOutcome.UnknownSourceRole,
                Array.Empty<LibraryPieceRequirement>());
    }

    public interface IRackPieceRequirementExtractor<in TPayload>
    {
        PieceRequirementExtractionResult Extract(TPayload payload, RackViewAddress address);
    }

    /// <summary>V2 extraction preserves every header piece and receives view authority from preparation.</summary>
    public sealed class RackHeaderPieceRequirementExtractorV2 : IRackPieceRequirementExtractor<HeaderRunPlan>
    {
        public static RackHeaderPieceRequirementExtractorV2 Instance { get; } = new RackHeaderPieceRequirementExtractorV2();

        private RackHeaderPieceRequirementExtractorV2()
        {
        }

        public static PieceRequirementExtractionResult Extract(HeaderRunPlan payload, RackViewAddress address)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));

            var instances = payload.LooseInstances
                .Where(instance => instance != null)
                .Concat(payload.Headers
                    .Where(group => group != null)
                    .SelectMany(group => group.Instances)
                    .Where(instance => instance != null));
            var requirements = new List<LibraryPieceRequirement>();
            foreach (var instance in instances)
            {
                var classification = RackHeaderBlockRequirementRoleClassifier.Classify(instance.Role);
                if (!classification.HasRole)
                {
                    return PieceRequirementExtractionResult.UnknownSourceRole();
                }

                requirements.Add(new LibraryPieceRequirement(
                    instance.PieceId,
                    address,
                    classification.Role,
                    instance.BlockName));
            }

            return PieceRequirementExtractionResult.Success(requirements);
        }

        PieceRequirementExtractionResult IRackPieceRequirementExtractor<HeaderRunPlan>.Extract(
            HeaderRunPlan payload,
            RackViewAddress address)
            => Extract(payload, address);
    }

    public static class LibraryPieceKeyProjection
    {
        public static IReadOnlyList<LibraryBlockRequirement> Project(
            IReadOnlyList<LibraryPieceRequirement> requirements)
        {
            if (requirements == null) throw new ArgumentNullException(nameof(requirements));

            var keys = new List<LibraryBlockRequirement>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var requirement in requirements)
            {
                if (requirement == null
                    || requirement.Role == RequirementRole.NotApplicable
                    || requirement.KeyState == RequirementKeyState.KeyMissing
                    || !seen.Add(requirement.LibraryKey))
                {
                    continue;
                }

                keys.Add(new LibraryBlockRequirement(requirement.LibraryKey));
            }

            return keys;
        }
    }

    public enum LibraryAvailability
    {
        Ok,
        FileMissing,
        Unknown
    }

    public enum LibraryBlockPresence
    {
        Present,
        BlockMissing,
        Unknown
    }

    public readonly struct LibraryKeyAvailabilityObservation
    {
        public LibraryKeyAvailabilityObservation(
            string key,
            LibraryAvailability libraryAvailability,
            LibraryBlockPresence blockPresence)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("An observation needs a key.", nameof(key));
            Key = key;
            LibraryAvailability = libraryAvailability;
            BlockPresence = blockPresence;
        }

        public string Key { get; }
        public LibraryAvailability LibraryAvailability { get; }
        public LibraryBlockPresence BlockPresence { get; }
    }

    public interface ILibraryPieceAvailabilityQuery
    {
        IReadOnlyList<LibraryKeyAvailabilityObservation> Query(
            IReadOnlyList<LibraryBlockRequirement> requirements);
    }

    public sealed class LibraryPieceAvailabilityFact
    {
        internal LibraryPieceAvailabilityFact(
            LibraryPieceRequirement requirement,
            LibraryAvailability libraryAvailability,
            LibraryBlockPresence blockPresence)
        {
            Requirement = requirement;
            LibraryAvailability = libraryAvailability;
            BlockPresence = blockPresence;
        }

        public LibraryPieceRequirement Requirement { get; }
        public LibraryAvailability LibraryAvailability { get; }
        public LibraryBlockPresence BlockPresence { get; }
    }

    public sealed class LibraryPieceAvailabilityFlowResultV2
    {
        internal LibraryPieceAvailabilityFlowResultV2(
            IReadOnlyList<LibraryPieceAvailabilityFact> facts,
            LibraryBlockImportResult import)
        {
            Facts = facts;
            Import = import;
        }

        public IReadOnlyList<LibraryPieceAvailabilityFact> Facts { get; }
        public LibraryBlockImportResult Import { get; }
    }

    public static class LibraryPieceAvailabilityFlowV2
    {
        public static LibraryPieceAvailabilityFlowResultV2 Observe(
            IReadOnlyList<LibraryPieceRequirement> requirements,
            bool allowImport,
            ILibraryPieceAvailabilityQuery query,
            ILibraryBlockImporter importer)
        {
            if (requirements == null) throw new ArgumentNullException(nameof(requirements));
            if (query == null) throw new ArgumentNullException(nameof(query));
            if (allowImport && importer == null) throw new ArgumentNullException(nameof(importer));

            var projected = LibraryPieceKeyProjection.Project(requirements);
            var import = projected.Count > 0 && allowImport
                ? importer.Ensure(projected)
                : new LibraryBlockImportResult(false, 0);
            var observations = projected.Count > 0
                ? query.Query(projected) ?? Array.Empty<LibraryKeyAvailabilityObservation>()
                : Array.Empty<LibraryKeyAvailabilityObservation>();
            var byKey = observations
                .GroupBy(item => item.Key, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.Last(), StringComparer.OrdinalIgnoreCase);
            var facts = new List<LibraryPieceAvailabilityFact>(requirements.Count);
            foreach (var requirement in requirements)
            {
                if (requirement != null
                    && requirement.Role != RequirementRole.NotApplicable
                    && requirement.KeyState == RequirementKeyState.Present
                    && byKey.TryGetValue(requirement.LibraryKey, out var observation))
                {
                    facts.Add(new LibraryPieceAvailabilityFact(
                        requirement,
                        observation.LibraryAvailability,
                        observation.BlockPresence));
                }
                else
                {
                    facts.Add(new LibraryPieceAvailabilityFact(
                        requirement,
                        LibraryAvailability.Unknown,
                        LibraryBlockPresence.Unknown));
                }
            }

            return new LibraryPieceAvailabilityFlowResultV2(facts, import);
        }
    }
}
