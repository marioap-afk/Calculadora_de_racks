using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Result of the dynamic (pallet flow) header-height calculation.
    /// <see cref="TheoreticalHeight"/> is the raw required height; <see cref="HeaderHeight"/> is that value
    /// rounded up to the next commercial foot (the height actually built).
    /// </summary>
    public sealed class DynamicHeaderHeightResult
    {
        /// <summary>Required height before the commercial round-up (in).</summary>
        public double TheoreticalHeight { get; init; }

        /// <summary>Commercial height: <see cref="TheoreticalHeight"/> rounded up to the next full foot (in).</summary>
        public double HeaderHeight { get; init; }

        /// <summary>System slope over the full depth (in).</summary>
        public double Slope { get; init; }

        /// <summary>Clear span required above each load: load height + 6" (in).</summary>
        public double ClearSpace { get; init; }
    }

    /// <summary>
    /// Pure rule for the required height of a dynamic (pallet flow) header. The height stacks, from the
    /// floor up: the first-level elevation, the beam depth of every level, a full clear span between
    /// levels, a one-third clear span over the top level, plus the slope the flow lane drops over its
    /// depth. The total is then rounded up to the next commercial foot. No UI/AutoCAD.
    /// </summary>
    public static class DynamicHeaderHeightCalculator
    {
        /// <summary>Clear span required above a load, over its own height (in).</summary>
        public const double ClearAllowance = DynamicRackDefaults.DefaultClearHeight;

        /// <summary>Fraction of a clear span kept over the top level (no full span needed there).</summary>
        public const double TopFinishFraction = 1.0 / 3.0;

        /// <summary>Slope rise per <see cref="SlopeRun"/> of run, applied per foot of depth.</summary>
        public const double SlopeRise = 7.0;

        /// <summary>Slope run that <see cref="SlopeRise"/> is measured against.</summary>
        public const double SlopeRun = 16.0;

        /// <summary>Inches in a commercial foot (heights are sold in whole feet).</summary>
        public const double CommercialFoot = 12.0;

        /// <summary>
        /// Computes the required and commercial header height.
        /// </summary>
        /// <param name="loadHeight">Height of the load on a level (in).</param>
        /// <param name="levels">Number of load levels in the header (>= 1).</param>
        /// <param name="firstLevelHeight">Elevation of the first beam from the floor (in).</param>
        /// <param name="beamDepth">Vertical depth (peralte) of each level's beam (in).</param>
        /// <param name="totalDepth">Total depth of the flow lane the slope runs over (in).</param>
        public static DynamicHeaderHeightResult Calculate(
            double loadHeight,
            int levels,
            double firstLevelHeight,
            double beamDepth,
            double totalDepth)
        {
            if (levels < 1)
            {
                levels = 1;
            }

            var clearSpace = loadHeight + ClearAllowance;
            var betweenLevels = (levels - 1) * clearSpace;   // a full clear span between each pair of levels
            var topFinish = clearSpace * TopFinishFraction;  // only a third over the top level
            var beamsHeight = levels * beamDepth;            // the beams themselves take vertical room
            var slope = CalculateSlope(totalDepth);

            var theoretical = firstLevelHeight + beamsHeight + betweenLevels + topFinish + slope;

            return new DynamicHeaderHeightResult
            {
                ClearSpace = clearSpace,
                Slope = slope,
                TheoreticalHeight = theoretical,
                HeaderHeight = RoundUpToCommercialFoot(theoretical)
            };
        }

        /// <summary>
        /// Computes the commercial height from already-resolved beam mates. This keeps the header above the actual
        /// snapped top entrance instead of repeating the nominal first-level and slope arithmetic.
        /// </summary>
        public static DynamicHeaderHeightResult CalculateResolved(
            double loadHeight,
            IReadOnlyList<DynamicLoadBeamLevel> levels,
            double beamDepth)
        {
            if (levels == null || levels.Count == 0)
            {
                return Calculate(loadHeight, 1, 0.0, beamDepth, 0.0);
            }

            var ordered = levels.OrderBy(level => level.LevelNumber).ToList();
            var first = ordered[0];
            var top = ordered[ordered.Count - 1];
            var clearSpace = loadHeight + ClearAllowance;
            var slope = Math.Max(0.0, first.EntranceElevation - first.ExitElevation);
            var theoretical = top.EntranceElevation + beamDepth + clearSpace * TopFinishFraction;
            return new DynamicHeaderHeightResult
            {
                ClearSpace = clearSpace,
                Slope = slope,
                TheoreticalHeight = theoretical,
                HeaderHeight = RoundUpToCommercialFoot(theoretical)
            };
        }

        /// <summary>Computes the header height from a front whose pallet, clear and beam depth may vary by level.</summary>
        public static DynamicHeaderHeightResult CalculateResolved(DynamicRackFront front)
        {
            if (front?.LoadBeamLevels == null || front.LoadBeamLevels.Count == 0)
            {
                return Calculate(0.0, 1, 0.0, 0.0, 0.0);
            }

            var ordered = front.LoadBeamLevels.OrderBy(level => level.LevelNumber).ToList();
            var first = ordered[0];
            var top = ordered[ordered.Count - 1];
            var topCell = DynamicRackLevelGeometry.At(null, front, top.LevelNumber);
            var clearSpace = topCell.Pallet.Height + topCell.ClearHeight;
            var slope = Math.Max(0.0, first.EntranceElevation - first.ExitElevation);
            var theoretical = top.EntranceElevation
                              + topCell.InOutBeamDepth
                              + clearSpace * TopFinishFraction;
            return new DynamicHeaderHeightResult
            {
                ClearSpace = clearSpace,
                Slope = slope,
                TheoreticalHeight = theoretical,
                HeaderHeight = RoundUpToCommercialFoot(theoretical)
            };
        }

        public static double CalculateSlope(double totalDepth)
            => (Math.Max(0.0, totalDepth) / CommercialFoot) * (SlopeRise / SlopeRun);

        /// <summary>Rounds a height up to the next whole commercial foot (e.g. 145" → 156", 144" → 144").</summary>
        public static double RoundUpToCommercialFoot(double height)
        {
            if (height <= 0.0)
            {
                return 0.0;
            }

            return Math.Ceiling(height / CommercialFoot) * CommercialFoot;
        }
    }
}
