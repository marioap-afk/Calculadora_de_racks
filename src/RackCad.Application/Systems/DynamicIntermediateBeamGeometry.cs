using System.Collections.Generic;
using System.Linq;
using System;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>One logical internal post that receives exactly one intermediate support beam.</summary>
    public readonly struct DynamicIntermediateBeamSupport
    {
        public DynamicIntermediateBeamSupport(double postAxisX, bool mirrored)
        {
            PostAxisX = postAxisX;
            Mirrored = mirrored;
        }

        public double PostAxisX { get; }
        public bool Mirrored { get; }
    }

    /// <summary>
    /// Resolves one support at every internal module boundary. The two system ends are excluded because IN/OUT
    /// beams occupy them. A boundary at a header's end is its mirrored second post; a boundary at a header's start
    /// is its normal first post. The separator-separator boundary is one reinforced logical post, so it keeps only
    /// the normal beam on the primary profile; the reinforcement must not reverse the bracket toward the other side.
    /// </summary>
    public static class DynamicIntermediateBeamGeometry
    {
        public static IReadOnlyList<double> AllowedPeraltes(RackCatalog catalog)
        {
            var profile = catalog?.BeamProfiles?.FirstOrDefault(entry => string.Equals(
                entry?.Id,
                DynamicRackDefaults.IntermediateBeamCatalogId,
                StringComparison.OrdinalIgnoreCase));
            return PeralteList.Parse(profile?.Peraltes);
        }

        /// <summary>
        /// Normalizes one editable PERALTE per load level against the section catalog. Missing or invalid values use
        /// 6 in when allowed, otherwise the first catalog value. This keeps legacy documents explicit and deterministic.
        /// </summary>
        public static IReadOnlyList<double> ResolvePeraltes(
            RackCatalog catalog,
            IEnumerable<double> requested,
            int levelCount)
        {
            var result = new List<double>();
            if (levelCount < 1)
            {
                return result;
            }

            var allowed = AllowedPeraltes(catalog);
            var values = requested?.ToList() ?? new List<double>();
            var fallback = allowed.FirstOrDefault(value => Math.Abs(value - DynamicRackDefaults.DefaultIntermediateBeamDepth) < 1e-6);
            if (fallback <= 0.0)
            {
                fallback = allowed.Count > 0 ? allowed[0] : DynamicRackDefaults.DefaultIntermediateBeamDepth;
            }

            for (var index = 0; index < levelCount; index++)
            {
                var candidate = index < values.Count ? values[index] : 0.0;
                if (allowed.Count > 0 && !allowed.Any(value => Math.Abs(value - candidate) < 1e-6))
                {
                    candidate = fallback;
                }
                else if (candidate <= 0.0)
                {
                    candidate = fallback;
                }

                result.Add(candidate);
            }

            return result;
        }

        public static double PeralteAt(DynamicRackFront front, int levelNumber)
            => front != null && levelNumber > 0 && levelNumber <= front.Levels.Count
                ? front.Levels[levelNumber - 1].IntermediateBeamDepth
                : front != null && levelNumber > 0 && levelNumber <= front.IntermediateBeamDepths.Count
                ? front.IntermediateBeamDepths[levelNumber - 1]
                : DynamicRackDefaults.DefaultIntermediateBeamDepth;

        public static string BeamIdAt(DynamicRackFront front, int levelNumber)
            => front != null && levelNumber > 0 && levelNumber <= front.Levels.Count
               && !string.IsNullOrWhiteSpace(front.Levels[levelNumber - 1].IntermediateBeamCatalogId)
                ? front.Levels[levelNumber - 1].IntermediateBeamCatalogId
                : DynamicRackDefaults.IntermediateBeamCatalogId;

        /// <summary>The lateral projection overlaps all transverse fronts, so its visible support is the largest
        /// configured PERALTE among fronts that actually reach this level.</summary>
        public static double PeralteAt(DynamicRackSystem system, int levelNumber)
        {
            if (system == null || levelNumber < 1)
            {
                return DynamicRackDefaults.DefaultIntermediateBeamDepth;
            }

            var projected = system.Fronts
                .Where(front => front != null && front.LoadLevels >= levelNumber)
                .Select(front => PeralteAt(front, levelNumber))
                .DefaultIfEmpty(0.0)
                .Max();
            if (projected > 0.0)
            {
                return projected;
            }

            return levelNumber <= system.IntermediateBeamDepths.Count
                ? system.IntermediateBeamDepths[levelNumber - 1]
                : DynamicRackDefaults.DefaultIntermediateBeamDepth;
        }

        /// <summary>Peralte visible in one lateral post section: only fronts adjacent to that post participate.</summary>
        public static double PeralteAtPost(DynamicRackSystem system, int postIndex, int levelNumber)
        {
            if (system == null || levelNumber < 1)
            {
                return DynamicRackDefaults.DefaultIntermediateBeamDepth;
            }

            var projected = DynamicFrontGeometry.AdjacentFronts(system, postIndex)
                .Where(front => front.LoadLevels >= levelNumber)
                .Select(front => PeralteAt(front, levelNumber))
                .DefaultIfEmpty(0.0)
                .Max();
            return projected > 0.0 ? projected : PeralteAt(system, levelNumber);
        }

        /// <summary>At a shared post the visible support follows the adjacent cell with the largest PERALTE.</summary>
        public static string BeamIdAtPost(DynamicRackSystem system, int postIndex, int levelNumber)
        {
            var selected = DynamicFrontGeometry.AdjacentFronts(system, postIndex)
                .Where(front => front.LoadLevels >= levelNumber)
                .OrderByDescending(front => PeralteAt(front, levelNumber))
                .FirstOrDefault();
            return BeamIdAt(selected, levelNumber);
        }

        public static double PlantaPeralte(DynamicRackFront front)
            => front?.Levels?.Take(Math.Max(1, front.LoadLevels))
                   .Select(level => level.IntermediateBeamDepth)
                   .Where(value => value > 0.0)
                   .DefaultIfEmpty(DynamicRackDefaults.DefaultIntermediateBeamDepth)
                   .Max()
               ?? front?.IntermediateBeamDepths?.Take(Math.Max(1, front.LoadLevels))
                   .Where(value => value > 0.0)
                   .DefaultIfEmpty(DynamicRackDefaults.DefaultIntermediateBeamDepth)
                   .Max()
               ?? DynamicRackDefaults.DefaultIntermediateBeamDepth;

        public static IReadOnlyList<DynamicIntermediateBeamSupport> Supports(DynamicRackSystem system, Point2D finPoste)
            => Supports(system, finPoste, null);

        public static IReadOnlyList<DynamicIntermediateBeamSupport> Supports(
            DynamicRackSystem system,
            Point2D finPoste,
            DynamicRackFront front)
        {
            var result = new List<DynamicIntermediateBeamSupport>();
            if (system == null)
            {
                return result;
            }

            var modules = system.Modules.Where(module => module.Length > 0.0
                && (front == null
                    || (module.Index + 1 >= front.DepthStartPosition
                        && module.Index + 1 < front.DepthStartPosition + front.PalletsDeep))).ToList();
            for (var index = 1; index < modules.Count; index++)
            {
                var previous = modules[index - 1];
                var current = modules[index];
                var mirrored = previous.IsHeader;
                var postAxisX = !previous.IsHeader && !current.IsHeader
                    ? DynamicDerivedPostGeometry.Resolve(
                        current.StartX,
                        system.DerivedPostReinforced,
                        finPoste).PrimaryOrigin.X
                    : current.StartX;
                result.Add(new DynamicIntermediateBeamSupport(postAxisX, mirrored));
            }

            return result;
        }
    }
}
