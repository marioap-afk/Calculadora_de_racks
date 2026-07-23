using System;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>Canonical resolvers for the Push Back high-end (rear) beam PERALTE across views.</summary>
    public static class PushBackHighEndBeamGeometry
    {
        /// <summary>
        /// The ENVELOPING high-end beam PERALTE for a front's planta line. The planta collapses every load level of a
        /// front onto one plan line, so it must carry the enveloping (maximum) rear peralte among the front's levels —
        /// the same enveloping principle the existing plantas use — not just level 0.
        /// </summary>
        public static double PlantaPeralte(PushBackSystem system, int frontIndex)
        {
            var fronts = system?.Structure?.Fronts;
            if (fronts == null || frontIndex < 0 || frontIndex >= fronts.Count)
            {
                return PushBackDefaults.HighEndBeamDefaultPeralte;
            }

            var levelCount = Math.Max(1, fronts[frontIndex].LoadLevels);
            var max = 0.0;
            for (var level = 0; level < levelCount; level++)
            {
                max = Math.Max(max, system.HighEndBeamPeralteAt(frontIndex, level));
            }

            return max > 0.0 ? max : PushBackDefaults.HighEndBeamDefaultPeralte;
        }
    }
}
