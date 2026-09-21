using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Cantilever;

namespace RackCad.Application.Systems.Shared
{
    /// <summary>A reusable piece definition required by a typed view plan.</summary>
    public sealed class LibraryBlockRequirement : IEquatable<LibraryBlockRequirement>
    {
        public LibraryBlockRequirement(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("A library block requirement needs a key.", nameof(key));
            }

            Key = key;
        }

        public string Key { get; }

        public bool Equals(LibraryBlockRequirement other)
            => other != null && string.Equals(Key, other.Key, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object obj)
            => Equals(obj as LibraryBlockRequirement);

        public override int GetHashCode()
            => StringComparer.OrdinalIgnoreCase.GetHashCode(Key);

        public override string ToString() => Key;
    }

    public interface IRackBlockRequirementExtractor<in TPayload>
    {
        IReadOnlyList<LibraryBlockRequirement> Extract(TPayload payload);
    }

    /// <summary>
    /// Explicit typed extractors. Header plans expose the real piece instances; Cantilever plans are pure
    /// geometry and therefore require no reusable library definitions.
    /// </summary>
    public static class RackBlockRequirementExtractors
    {
        public static IRackBlockRequirementExtractor<HeaderRunPlan> HeaderRun { get; }
            = new HeaderRunRequirementExtractor();

        public static IRackBlockRequirementExtractor<CantileverViewPlan> Cantilever { get; }
            = new CantileverRequirementExtractor();

        private sealed class HeaderRunRequirementExtractor : IRackBlockRequirementExtractor<HeaderRunPlan>
        {
            public IReadOnlyList<LibraryBlockRequirement> Extract(HeaderRunPlan payload)
            {
                if (payload == null) throw new ArgumentNullException(nameof(payload));

                var keys = payload.LooseInstances
                    .Where(instance => instance != null)
                    .Select(instance => instance.BlockName)
                    .Concat(payload.Headers
                        .Where(group => group != null)
                        .SelectMany(group => group.Instances)
                        .Where(instance => instance != null)
                        .Select(instance => instance.BlockName));

                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var requirements = new List<LibraryBlockRequirement>();
                foreach (var key in keys)
                {
                    // The legacy importer ignores blank instance names. Preserve that behavior while ensuring
                    // that no blank value becomes a library identity.
                    if (string.IsNullOrWhiteSpace(key) || !seen.Add(key)) continue;
                    requirements.Add(new LibraryBlockRequirement(key));
                }

                return requirements;
            }
        }

        private sealed class CantileverRequirementExtractor : IRackBlockRequirementExtractor<CantileverViewPlan>
        {
            public IReadOnlyList<LibraryBlockRequirement> Extract(CantileverViewPlan payload)
            {
                if (payload == null) throw new ArgumentNullException(nameof(payload));
                return Array.Empty<LibraryBlockRequirement>();
            }
        }
    }

    public enum LibraryBlockAvailability
    {
        Found = 1,
        Missing = 2,
    }

    public readonly struct LibraryBlockAvailabilityFact
    {
        public LibraryBlockAvailabilityFact(
            LibraryBlockRequirement requirement,
            LibraryBlockAvailability availability)
        {
            Requirement = requirement ?? throw new ArgumentNullException(nameof(requirement));
            Availability = availability;
        }

        public LibraryBlockRequirement Requirement { get; }
        public LibraryBlockAvailability Availability { get; }
    }

    /// <summary>Pure observation port. Implementations must not import, repair, create or rename definitions.</summary>
    public interface ILibraryBlockQuery
    {
        IReadOnlyList<LibraryBlockAvailabilityFact> Query(
            IReadOnlyList<LibraryBlockRequirement> requirements);
    }

    /// <summary>Mutation port implemented by the Plugin importer, separate from the query.</summary>
    public interface ILibraryBlockImporter
    {
        LibraryBlockImportResult Ensure(IReadOnlyList<LibraryBlockRequirement> requirements);
    }

    public readonly struct LibraryBlockImportResult
    {
        public LibraryBlockImportResult(bool wasAttempted, int importedCount)
        {
            if (importedCount < 0) throw new ArgumentOutOfRangeException(nameof(importedCount));
            WasAttempted = wasAttempted;
            ImportedCount = importedCount;
        }

        public bool WasAttempted { get; }
        public int ImportedCount { get; }
    }

    public sealed class LibraryBlockAvailabilityFlowResult
    {
        internal LibraryBlockAvailabilityFlowResult(
            IReadOnlyList<LibraryBlockAvailabilityFact> facts,
            LibraryBlockImportResult import)
        {
            Facts = facts;
            Import = import;
        }

        public IReadOnlyList<LibraryBlockAvailabilityFact> Facts { get; }
        public LibraryBlockImportResult Import { get; }
    }

    /// <summary>
    /// Orders the two separate ports. When import is allowed, the only authoritative availability is the query
    /// after Ensure. With no import permission, the direct query is final.
    /// </summary>
    public static class LibraryBlockAvailabilityFlow
    {
        public static LibraryBlockAvailabilityFlowResult Observe(
            IReadOnlyList<LibraryBlockRequirement> requirements,
            bool allowImport,
            ILibraryBlockQuery query,
            ILibraryBlockImporter importer)
        {
            if (requirements == null) throw new ArgumentNullException(nameof(requirements));
            if (query == null) throw new ArgumentNullException(nameof(query));
            if (allowImport && importer == null) throw new ArgumentNullException(nameof(importer));

            if (requirements.Count == 0)
            {
                return new LibraryBlockAvailabilityFlowResult(
                    Array.Empty<LibraryBlockAvailabilityFact>(),
                    new LibraryBlockImportResult(false, 0));
            }

            var import = allowImport
                ? importer.Ensure(requirements)
                : new LibraryBlockImportResult(false, 0);
            var finalFacts = query.Query(requirements);
            return new LibraryBlockAvailabilityFlowResult(finalFacts, import);
        }
    }
}
