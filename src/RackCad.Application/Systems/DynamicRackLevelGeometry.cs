using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Single source of truth for one dynamic front x level cell. Legacy rack/front fields are defaults only;
    /// resolved drawing, BOM and UI values consume the explicit level objects returned here.
    /// </summary>
    public static class DynamicRackLevelGeometry
    {
        public static IReadOnlyList<DynamicRackLevel> Resolve(
            DynamicRackDesign design,
            DynamicRackFrontDesign frontDesign,
            DynamicRackFront front,
            RackCatalog catalog)
        {
            var result = new List<DynamicRackLevel>();
            if (design?.Pallet == null || front == null)
            {
                return result;
            }

            var source = frontDesign?.Levels ?? Array.Empty<DynamicRackLevelDesign>();
            var legacyIntermediate = frontDesign?.IntermediateBeamDepths?.Count > 0
                ? frontDesign.IntermediateBeamDepths
                : design.IntermediateBeamDepths;
            var defaultInOutId = ResolveBeamId(catalog, design.InOutBeamCatalogId, DynamicRackDefaults.InOutBeamCatalogId);

            for (var index = 0; index < Math.Max(1, front.LoadLevels); index++)
            {
                var requested = index < source.Count ? source[index] : null;
                var palletFront = Positive(requested?.PalletFront, design.Pallet.Front);
                var palletHeight = Positive(requested?.PalletHeight, design.Pallet.Height);
                var palletWeight = NonNegative(requested?.PalletWeight, design.Pallet.Weight);
                var clearHeight = NonNegative(requested?.ClearHeight, DynamicRackDefaults.DefaultClearHeight);
                var inOutId = ResolveBeamId(catalog, requested?.InOutBeamCatalogId, defaultInOutId);
                var inOutDepth = DynamicLoadBeamGeometry.ResolveBeamDepth(
                    catalog,
                    inOutId,
                    Positive(requested?.InOutBeamDepth, design.BeamDepth));
                var intermediateId = ResolveBeamId(
                    catalog,
                    requested?.IntermediateBeamCatalogId,
                    DynamicRackDefaults.IntermediateBeamCatalogId);
                var legacyDepth = index < (legacyIntermediate?.Count ?? 0)
                    ? legacyIntermediate[index]
                    : DynamicRackDefaults.DefaultIntermediateBeamDepth;
                var intermediateDepth = ResolvePeralte(
                    catalog,
                    intermediateId,
                    Positive(requested?.IntermediateBeamDepth, legacyDepth),
                    DynamicRackDefaults.DefaultIntermediateBeamDepth);
                var manualLength = PositiveNullable(requested?.BeamLengthOverride)
                                   ?? PositiveNullable(frontDesign?.BeamLengthOverride);
                var bfr = DynamicFrontGeometry.Bfr(palletFront);
                var beamLength = manualLength
                                 ?? DynamicFrontGeometry.AutoBeamLength(
                                     palletFront,
                                     Math.Max(1, front.PalletCount),
                                     design.PalletTolerance);

                result.Add(new DynamicRackLevel
                {
                    LevelNumber = index + 1,
                    Pallet = new PalletSpecification(
                        palletFront,
                        design.Pallet.Depth,
                        palletHeight,
                        palletWeight,
                        string.IsNullOrWhiteSpace(design.Pallet.WeightUnit) ? "kg" : design.Pallet.WeightUnit),
                    ClearHeight = clearHeight,
                    InOutBeamCatalogId = inOutId,
                    InOutBeamDepth = inOutDepth,
                    BeamLengthOverride = manualLength,
                    Bfr = bfr,
                    BeamLength = beamLength,
                    IntermediateBeamCatalogId = intermediateId,
                    IntermediateBeamDepth = intermediateDepth
                });
            }

            front.Levels.Clear();
            foreach (var level in result)
            {
                front.Levels.Add(level);
            }

            front.FirstLevelHeight = NonNegative(
                frontDesign?.FirstLevelHeight,
                Math.Max(0.0, design.FirstLevelHeight));
            front.Bfr = result.Select(level => level.Bfr).DefaultIfEmpty(DynamicFrontGeometry.Bfr(design.Pallet.Front)).Max();
            front.BeamLength = result.Select(level => level.BeamLength)
                .DefaultIfEmpty(DynamicFrontGeometry.AutoBeamLength(
                    design.Pallet.Front,
                    Math.Max(1, front.PalletCount),
                    design.PalletTolerance))
                .Max();
            front.IntermediateBeamDepths.Clear();
            foreach (var level in result)
            {
                front.IntermediateBeamDepths.Add(level.IntermediateBeamDepth);
            }

            return result;
        }

        public static DynamicRackLevel At(DynamicRackSystem system, DynamicRackFront front, int levelNumber)
        {
            if (front != null && levelNumber > 0 && levelNumber <= front.Levels.Count)
            {
                return front.Levels[levelNumber - 1];
            }

            var pallet = system?.Pallet ?? new PalletSpecification();
            var palletCount = Math.Max(1, front?.PalletCount ?? DynamicRackDefaults.DefaultPalletsWide);
            var beamLength = front?.BeamLength > 0.0
                ? front.BeamLength
                : DynamicFrontGeometry.AutoBeamLength(
                    pallet.Front,
                    palletCount,
                    system?.PalletTolerance ?? DynamicRackDefaults.DefaultPalletTolerance);
            return new DynamicRackLevel
            {
                LevelNumber = Math.Max(1, levelNumber),
                Pallet = new PalletSpecification(
                    pallet.Front,
                    pallet.Depth,
                    pallet.Height,
                    pallet.Weight,
                    string.IsNullOrWhiteSpace(pallet.WeightUnit) ? "kg" : pallet.WeightUnit),
                ClearHeight = DynamicRackDefaults.DefaultClearHeight,
                InOutBeamCatalogId = string.IsNullOrWhiteSpace(system?.InOutBeamCatalogId)
                    ? DynamicRackDefaults.InOutBeamCatalogId
                    : system.InOutBeamCatalogId,
                InOutBeamDepth = system?.InOutBeamDepth > 0.0
                    ? system.InOutBeamDepth
                    : DynamicRackDefaults.DefaultBeamDepth,
                BeamLengthOverride = front?.BeamLengthOverride,
                Bfr = front?.Bfr > 0.0 ? front.Bfr : DynamicFrontGeometry.Bfr(pallet.Front),
                BeamLength = beamLength,
                IntermediateBeamCatalogId = DynamicRackDefaults.IntermediateBeamCatalogId,
                IntermediateBeamDepth = DynamicIntermediateBeamGeometry.PeralteAt(front, levelNumber)
            };
        }

        public static DynamicRackLevel Envelope(DynamicRackSystem system, DynamicRackFront front)
            => front?.Levels?.OrderByDescending(level => level.BeamLength).FirstOrDefault()
               ?? At(system, front, 1);

        public static IReadOnlyList<BeamProfileCatalogEntry> CompatibleInOutBeams(RackCatalog catalog)
            => CompatibleBeams(
                catalog,
                DynamicRackDefaults.InOutBeamCatalogId,
                DynamicRackDefaults.InOutBeamBedMatePoint,
                DynamicRackDefaults.InOutBeamView);

        public static IReadOnlyList<BeamProfileCatalogEntry> CompatibleIntermediateBeams(RackCatalog catalog)
            => (catalog?.BeamProfiles ?? Array.Empty<BeamProfileCatalogEntry>())
                .Where(entry => entry != null
                                && catalog.ConnectionLayout.FindConnectionLayout(
                                    entry.Id,
                                    DynamicRackDefaults.IntermediateBeamLeftBedMatePoint,
                                    DynamicRackDefaults.IntermediateBeamView) != null
                                && catalog.ConnectionLayout.FindConnectionLayout(
                                    entry.Id,
                                    DynamicRackDefaults.IntermediateBeamRightBedMatePoint,
                                    DynamicRackDefaults.IntermediateBeamView) != null
                                && catalog.Blocks.FindBlock(entry.Id, DynamicRackDefaults.IntermediateBeamView) != null)
                .ToList();

        public static IReadOnlyList<double> AllowedPeraltes(RackCatalog catalog, string beamId)
            => PeralteList.Parse((catalog?.BeamProfiles ?? Array.Empty<BeamProfileCatalogEntry>())
                .FirstOrDefault(entry => string.Equals(entry?.Id, beamId, StringComparison.OrdinalIgnoreCase))?.Peraltes);

        private static IReadOnlyList<BeamProfileCatalogEntry> CompatibleBeams(
            RackCatalog catalog,
            string fallbackId,
            string matePoint,
            string view)
        {
            var result = (catalog?.BeamProfiles ?? Array.Empty<BeamProfileCatalogEntry>())
                .Where(entry => entry != null
                                && catalog.ConnectionLayout.FindConnectionLayout(entry.Id, matePoint, view) != null
                                && catalog.Blocks.FindBlock(entry.Id, view) != null)
                .ToList();
            if (result.Count == 0)
            {
                var fallback = (catalog?.BeamProfiles ?? Array.Empty<BeamProfileCatalogEntry>())
                    .FirstOrDefault(entry => string.Equals(entry?.Id, fallbackId, StringComparison.OrdinalIgnoreCase));
                if (fallback != null)
                {
                    result.Add(fallback);
                }
            }

            return result;
        }

        private static string ResolveBeamId(RackCatalog catalog, string requested, string fallback)
        {
            var exists = (catalog?.BeamProfiles ?? Array.Empty<BeamProfileCatalogEntry>())
                .Any(entry => string.Equals(entry?.Id, requested, StringComparison.OrdinalIgnoreCase));
            return exists && !string.IsNullOrWhiteSpace(requested) ? requested : fallback;
        }

        private static double ResolvePeralte(
            RackCatalog catalog,
            string beamId,
            double requested,
            double preferredFallback)
        {
            var allowed = AllowedPeraltes(catalog, beamId);
            if (allowed.Any(value => Math.Abs(value - requested) < 1e-6))
            {
                return requested;
            }

            var fallback = allowed.FirstOrDefault(value => Math.Abs(value - preferredFallback) < 1e-6);
            return fallback > 0.0
                ? fallback
                : allowed.FirstOrDefault() > 0.0 ? allowed[0] : Math.Max(preferredFallback, requested);
        }

        private static double Positive(double? value, double fallback)
            => value.HasValue && value.Value > 0.0 ? value.Value : fallback;

        private static double NonNegative(double? value, double fallback)
            => value.HasValue && value.Value >= 0.0 ? value.Value : fallback;

        private static double? PositiveNullable(double? value)
            => value.HasValue && value.Value > 0.0 ? value : null;
    }
}
