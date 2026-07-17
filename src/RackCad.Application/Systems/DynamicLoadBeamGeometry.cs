using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
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
    /// level has a low exit and an entrance raised by the full lane slope. Both mates use the block origin at the
    /// front's resolved boundaries. Legacy/non-sectioned calls use exit X=0 and entrance X=TotalLength.
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
            double troquelPitch = 0.0)
        {
            var result = new List<DynamicLoadBeamLevel>();
            if (pallet == null || loadLevels < 1 || beamDepth <= 0.0)
            {
                return result;
            }

            var step = beamDepth + pallet.Height + DynamicHeaderHeightCalculator.ClearAllowance;
            for (var index = 0; index < loadLevels; index++)
            {
                var rawExit = firstLevelHeight + index * step;
                var exit = SnapToNearestTroquel(rawExit, troquelGridBase, troquelPitch);
                var entrance = SnapToNearestTroquel(
                    rawExit + Math.Max(0.0, slope),
                    troquelGridBase,
                    troquelPitch);
                result.Add(new DynamicLoadBeamLevel(index + 1, exit, entrance));
            }

            return result;
        }

        /// <summary>Resolves elevations when pallet height, clear and beam depth vary by level.</summary>
        public static IReadOnlyList<DynamicLoadBeamLevel> ResolveLevels(
            IReadOnlyList<DynamicRackLevel> configurations,
            double firstLevelHeight,
            double slope,
            double troquelGridBase = double.NaN,
            double troquelPitch = 0.0)
        {
            var result = new List<DynamicLoadBeamLevel>();
            if (configurations == null || configurations.Count == 0)
            {
                return result;
            }

            var rawExit = Math.Max(0.0, firstLevelHeight);
            foreach (var configuration in configurations.OrderBy(level => level.LevelNumber))
            {
                var exit = SnapToNearestTroquel(rawExit, troquelGridBase, troquelPitch);
                var entrance = SnapToNearestTroquel(
                    rawExit + Math.Max(0.0, slope),
                    troquelGridBase,
                    troquelPitch);
                result.Add(new DynamicLoadBeamLevel(configuration.LevelNumber, exit, entrance));
                rawExit += configuration.InOutBeamDepth
                           + configuration.Pallet.Height
                           + configuration.ClearHeight;
            }

            return result;
        }

        private static double SnapToNearestTroquel(double value, double gridBase, double pitch)
        {
            if (double.IsNaN(gridBase) || double.IsInfinity(gridBase) || pitch <= 0.0)
            {
                return value;
            }

            var steps = Math.Round((value - gridBase) / pitch, MidpointRounding.AwayFromZero);
            return gridBase + steps * pitch;
        }

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
