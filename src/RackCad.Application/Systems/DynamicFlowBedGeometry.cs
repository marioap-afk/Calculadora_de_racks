using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>The physical bed geometry resolved between the complete exit and entrance beam mates.</summary>
    public readonly struct DynamicFlowBedAxis
    {
        public DynamicFlowBedAxis(
            int levelNumber,
            Point2D exitMate,
            Point2D entranceMate,
            Point2D railLocalMate)
        {
            LevelNumber = levelNumber;
            ExitMate = exitMate;
            EntranceMate = entranceMate;
            RailLocalMate = railLocalMate;
        }

        public int LevelNumber { get; }
        public Point2D ExitMate { get; }
        public Point2D EntranceMate { get; }
        public Point2D RailLocalMate { get; }

        public double Rise => EntranceMate.Y - ExitMate.Y;
        public double Run => EntranceMate.X - ExitMate.X;
        public double Length => Math.Sqrt(Run * Run + Rise * Rise);
        public double AngleRadians => Math.Atan2(Rise, Run);

        /// <summary>World origin of the rail block after TROQUEL_IN is bolted to the exit beam.</summary>
        public Point2D RailOrigin
        {
            get
            {
                var cos = Math.Cos(AngleRadians);
                var sin = Math.Sin(AngleRadians);
                return new Point2D(
                    ExitMate.X - RailLocalMate.X * cos + RailLocalMate.Y * sin,
                    ExitMate.Y - RailLocalMate.X * sin - RailLocalMate.Y * cos);
            }
        }

        /// <summary>Height of the rail ORIGIN line at a world X. Intermediate supports touch this line, not the
        /// parallel TROQUEL_IN/TROQUEL_CAMA mate line. Their vertical-slot bracket does not snap to a post hole.</summary>
        public double RailOriginYAt(double worldX)
            => Math.Abs(Run) < 1e-9
                ? RailOrigin.Y
                : RailOrigin.Y + (worldX - RailOrigin.X) * Rise / Run;
    }

    /// <summary>
    /// Single source of truth for the pallet-flow bed line. The complete bed and every intermediate support consume
    /// the same end-beam mates, preventing their slope or elevation from drifting apart.
    /// </summary>
    public static class DynamicFlowBedGeometry
    {
        /// <summary>
        /// Physical LONGITUD of the complete bed. The slope and catalog mates only place and rotate the block;
        /// they must not change its commercial cut length.
        /// </summary>
        public static double ResolveBedLength(DynamicRackSystem system, DynamicRackFront front = null)
        {
            if (system == null)
            {
                return 0.0;
            }

            var totalLength = front != null && front.EndX > front.StartX
                ? front.EndX - front.StartX
                : system.TotalLength;
            return Math.Max(0.0, totalLength - DynamicRackDefaults.FlowBedLengthClearance);
        }

        public static IReadOnlyList<DynamicFlowBedAxis> Resolve(DynamicRackSystem system, RackCatalog catalog)
            => Resolve(system, catalog, null);

        public static IReadOnlyList<DynamicFlowBedAxis> Resolve(
            DynamicRackSystem system,
            RackCatalog catalog,
            DynamicRackFront front)
        {
            var result = new List<DynamicFlowBedAxis>();
            var levels = DynamicFrontGeometry.LoadBeamLevels(system, front);
            if (system == null || levels.Count == 0 || system.TotalLength <= 0.0)
            {
                return result;
            }

            var railMateEntry = catalog?.ConnectionLayout.FindConnectionLayout(
                FlowBedDefaults.RailId,
                FlowBedDefaults.RailInOutMatePoint,
                FlowBedDefaults.View);

            // A missing mate is a missing physical contract: never fall back silently to the block origin.
            if (railMateEntry == null)
            {
                return result;
            }

            var railLocalMate = new Point2D(railMateEntry.LocalX, railMateEntry.LocalY);
            var placements = DynamicLoadBeamGeometry.Placements(system, front);
            foreach (var level in levels)
            {
                var exitBeam = placements.FirstOrDefault(placement =>
                    placement.LevelNumber == level.LevelNumber && !placement.IsEntrance);
                var entranceBeam = placements.FirstOrDefault(placement =>
                    placement.LevelNumber == level.LevelNumber && placement.IsEntrance);
                if (exitBeam == null || entranceBeam == null)
                {
                    continue;
                }

                var beamId = string.IsNullOrWhiteSpace(exitBeam.BeamCatalogId)
                    ? DynamicRackDefaults.InOutBeamCatalogId
                    : exitBeam.BeamCatalogId;
                var beamMateEntry = catalog?.ConnectionLayout.FindConnectionLayout(
                    beamId,
                    DynamicRackDefaults.InOutBeamBedMatePoint,
                    DynamicRackDefaults.InOutBeamView);
                if (beamMateEntry == null)
                {
                    continue;
                }

                var beamLocalMate = new Point2D(beamMateEntry.LocalX, beamMateEntry.LocalY);

                var exitMate = BeamMate(exitBeam, beamLocalMate);
                var entranceMate = BeamMate(entranceBeam, beamLocalMate);
                if (entranceMate.X - exitMate.X <= 0.0)
                {
                    continue;
                }

                result.Add(new DynamicFlowBedAxis(level.LevelNumber, exitMate, entranceMate, railLocalMate));
            }

            return result;
        }

        private static Point2D BeamMate(DynamicLoadBeamPlacement placement, Point2D localMate)
        {
            var localX = placement.MirroredX ? -localMate.X : localMate.X;
            return new Point2D(placement.X + localX, placement.Y + localMate.Y);
        }
    }
}
