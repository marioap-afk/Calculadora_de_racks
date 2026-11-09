using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Systems.Dynamic
{
    /// <summary>One resolved complete IN/OUT beam placement in the lateral system view.</summary>
    public sealed class DynamicLoadBeamPlacement
    {
        public DynamicLoadBeamPlacement(
            int levelNumber,
            bool isEntrance,
            double x,
            double y,
            bool mirroredX,
            string beamCatalogId = null,
            double beamDepth = 0.0,
            double beamLength = 0.0)
        {
            LevelNumber = levelNumber;
            IsEntrance = isEntrance;
            X = x;
            Y = y;
            MirroredX = mirroredX;
            BeamCatalogId = beamCatalogId;
            BeamDepth = beamDepth;
            BeamLength = beamLength;
        }

        public int LevelNumber { get; }
        public bool IsEntrance { get; }
        public double X { get; }
        public double Y { get; }
        public bool MirroredX { get; }
        public string BeamCatalogId { get; }
        public double BeamDepth { get; }
        public double BeamLength { get; }
    }

    /// <summary>
    /// Single source of truth for pallet-flow entrance/exit beams. The catalog supplies the fixed beam depth; every
    /// level has a low exit and an entrance raised by the rise the first level resolves from the lane slope (I-67).
    /// Both mates use the block origin at the front's resolved boundaries. Legacy/non-sectioned calls use exit X=0 and
    /// entrance X=TotalLength.
    /// </summary>
    public static class DynamicLoadBeamGeometry
    {
        public static double ResolveBeamDepth(RackCatalog catalog, string beamId, double requestedDepth)
        {
            var profile = catalog?.BeamProfiles?.FirstOrDefault(entry => string.Equals(
                entry?.Id, beamId, StringComparison.OrdinalIgnoreCase));
            var allowed = PeralteList.Parse(profile?.Peraltes);

            if (allowed.Count == 0)
            {
                return requestedDepth > 0.0 ? requestedDepth : DynamicRackDefaults.DefaultBeamDepth;
            }

            if (requestedDepth > 0.0 && allowed.Any(value => Math.Abs(value - requestedDepth) < 1e-6))
            {
                return requestedDepth;
            }

            // The first implementation enables one fixed C6 profile. Keeping this catalog-driven lets another
            // single-peralte IN/OUT profile replace it later without changing geometry code.
            return allowed[0];
        }

        public static IReadOnlyList<DynamicLoadBeamLevel> ResolveLevels(
            PalletSpecification pallet,
            int loadLevels,
            double firstLevelHeight,
            double beamDepth,
            double slope,
            double troquelGridBase = double.NaN,
            double troquelPitch = 0.0,
            RackFirstLevelDatumMode datum = RackFirstLevelDatumMode.LegacyAbsolute)
        {
            if (pallet == null || loadLevels < 1 || beamDepth <= 0.0)
            {
                return new List<DynamicLoadBeamLevel>();
            }

            var requirement = beamDepth + pallet.Height + DynamicHeaderHeightCalculator.ClearAllowance;
            return ResolveElevations(
                Enumerable.Range(1, loadLevels).Select(number => (number, requirement)).ToList(),
                RackFirstLevelDatum.RawElevation(firstLevelHeight, datum, troquelGridBase, troquelPitch),
                slope,
                troquelGridBase,
                troquelPitch);
        }

        /// <summary>Resolves elevations when pallet height, clear and beam depth vary by level.</summary>
        /// <param name="datum">
        /// I-42 — desde donde se mide <paramref name="firstLevelHeight"/>. Por omision, la lectura HISTORICA: el
        /// numero es una elevacion absoluta. Ningun llamador que no lo pase cambia de comportamiento.
        /// </param>
        public static IReadOnlyList<DynamicLoadBeamLevel> ResolveLevels(
            IReadOnlyList<DynamicRackLevel> configurations,
            double firstLevelHeight,
            double slope,
            double troquelGridBase = double.NaN,
            double troquelPitch = 0.0,
            RackFirstLevelDatumMode datum = RackFirstLevelDatumMode.LegacyAbsolute)
        {
            if (configurations == null || configurations.Count == 0)
            {
                return new List<DynamicLoadBeamLevel>();
            }

            return ResolveElevations(
                configurations
                    .OrderBy(level => level.LevelNumber)
                    .Select(level => (level.LevelNumber, level.InOutBeamDepth + level.Pallet.Height + level.ClearHeight))
                    .ToList(),
                RackFirstLevelDatum.RawElevation(firstLevelHeight, datum, troquelGridBase, troquelPitch),
                slope,
                troquelGridBase,
                troquelPitch);
        }

        /// <summary>
        /// I-67 (H58) — the one routine behind both overloads. Each interval climbs to the first real punch that meets its
        /// own requirement, measured from the RESOLVED exit of the level below: no theoretical sum survives, so a rounding
        /// can never eat into the clearance of the next interval. The first level is not an interval, so its exit and its
        /// entrance keep their historical nearest-punch reading (ADR-0031). Every level keeps the rise the first level
        /// resolved (Owner decision D-PEND), which keeps the beds parallel and the entrance side as clear as the exit side.
        /// </summary>
        private static IReadOnlyList<DynamicLoadBeamLevel> ResolveElevations(
            IReadOnlyList<(int LevelNumber, double Requirement)> levels,
            double rawFirst,
            double slope,
            double gridBase,
            double pitch)
        {
            var result = new List<DynamicLoadBeamLevel>(levels.Count);
            var exit = SnapToNearestTroquel(rawFirst, gridBase, pitch);
            var rise = SnapToNearestTroquel(rawFirst + Math.Max(0.0, slope), gridBase, pitch) - exit;

            foreach (var level in levels)
            {
                result.Add(new DynamicLoadBeamLevel(
                    level.LevelNumber,
                    exit,
                    SnapToNearestTroquel(exit + rise, gridBase, pitch)));
                exit = NextExit(exit, level.Requirement, gridBase, pitch);
            }

            return result;
        }

        private static double NextExit(double exit, double requirement, double gridBase, double pitch)
            => HasMeasuredGrid(gridBase, pitch)
                // Floored at one punch, like the selective separation: a degenerate requirement cannot make levels coincide.
                ? FirstTroquelAtOrAbove(exit + Math.Max(pitch, requirement), gridBase, pitch)
                : exit + requirement;

        private static bool HasMeasuredGrid(double gridBase, double pitch)
            => !double.IsNaN(gridBase) && !double.IsInfinity(gridBase) && pitch > 0.0;

        private static double SnapToNearestTroquel(double value, double gridBase, double pitch)
        {
            if (!HasMeasuredGrid(gridBase, pitch))
            {
                return value;
            }

            var steps = Math.Round((value - gridBase) / pitch, MidpointRounding.AwayFromZero);
            return gridBase + steps * pitch;
        }

        /// <summary>
        /// The smallest punch at or above <paramref name="value"/>. The epsilon, in steps, is the one the selective rack
        /// uses: a value already on a punch stays on it instead of climbing another step because of floating-point noise.
        /// </summary>
        private static double FirstTroquelAtOrAbove(double value, double gridBase, double pitch)
            => gridBase + Math.Ceiling((value - gridBase) / pitch - 1e-9) * pitch;

        public static IReadOnlyList<DynamicLoadBeamPlacement> Placements(DynamicRackSystem system)
            => Placements(system, null);

        public static IReadOnlyList<DynamicLoadBeamPlacement> Placements(
            DynamicRackSystem system,
            DynamicRackFront front)
        {
            var result = new List<DynamicLoadBeamPlacement>();
            if (system == null || system.TotalLength <= 0.0)
            {
                return result;
            }

            var exitX = front?.StartX ?? 0.0;
            var entranceX = front?.EndX ?? system.TotalLength;

            foreach (var level in DynamicFrontGeometry.LoadBeamLevels(system, front))
            {
                var configuration = DynamicRackLevelGeometry.At(system, front, level.LevelNumber);
                var beamLength = front?.BeamLength > 0.0 ? front.BeamLength : configuration.BeamLength;
                result.Add(new DynamicLoadBeamPlacement(
                    level.LevelNumber,
                    false,
                    exitX,
                    level.ExitElevation,
                    false,
                    configuration.InOutBeamCatalogId,
                    configuration.InOutBeamDepth,
                    beamLength));
                result.Add(new DynamicLoadBeamPlacement(
                    level.LevelNumber,
                    true,
                    entranceX,
                    level.EntranceElevation,
                    true,
                    configuration.InOutBeamCatalogId,
                    configuration.InOutBeamDepth,
                    beamLength));
            }

            return result;
        }
    }
}
