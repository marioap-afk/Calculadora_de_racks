using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Physical entrance-guide plan shared by every view and the BOM. One enabled front x level produces two pieces,
    /// one at each transverse post; plan/lateral may collapse coincident projections but never redefine the inventory.
    /// </summary>
    public static class DynamicEntranceGuidePlan
    {
        public const double HeightAboveEntranceBeam = 8.0;

        public static IReadOnlyList<DynamicEntranceGuidePlacement> Build(
            DynamicRackSystem system,
            SelectiveSafetySelection selection)
        {
            var result = new List<DynamicEntranceGuidePlacement>();
            if (system == null || selection == null)
            {
                return result;
            }

            foreach (var front in system.Fronts.Where(front => front != null))
            {
                var length = EntranceSegmentLength(system, front);
                if (length <= 0.0)
                {
                    continue;
                }

                foreach (var level in DynamicFrontGeometry.LoadBeamLevels(system, front))
                {
                    var levelIndex = Math.Max(0, level.LevelNumber - 1);
                    if (!selection.GuiaEntradaAt(front.Index, levelIndex))
                    {
                        continue;
                    }

                    var y = level.EntranceElevation + HeightAboveEntranceBeam;
                    result.Add(new DynamicEntranceGuidePlacement(
                        front.Index, levelIndex, front.Index, false, length, y));
                    result.Add(new DynamicEntranceGuidePlacement(
                        front.Index, levelIndex, front.Index + 1, true, length, y));
                }
            }

            return result;
        }

        public static double EntranceSegmentLength(DynamicRackSystem system, DynamicRackFront front)
        {
            if (system == null || front == null)
            {
                return 0.0;
            }

            var lastPosition = front.DepthStartPosition + front.PalletsDeep - 1;
            var module = system.Modules.FirstOrDefault(item => item != null && item.Index + 1 == lastPosition);
            return Math.Max(0.0, module?.Length ?? 0.0);
        }
    }

    public readonly struct DynamicEntranceGuidePlacement
    {
        public DynamicEntranceGuidePlacement(
            int frontIndex,
            int levelIndex,
            int postIndex,
            bool mirroredAcrossFront,
            double length,
            double elevation)
        {
            FrontIndex = frontIndex;
            LevelIndex = levelIndex;
            PostIndex = postIndex;
            MirroredAcrossFront = mirroredAcrossFront;
            Length = length;
            Elevation = elevation;
        }

        public int FrontIndex { get; }
        public int LevelIndex { get; }
        public int PostIndex { get; }
        public bool MirroredAcrossFront { get; }
        public double Length { get; }
        public double Elevation { get; }
    }
}
