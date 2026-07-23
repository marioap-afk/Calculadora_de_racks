using System.Collections.Generic;

namespace RackCad.Domain.Systems
{
    /// <summary>
    /// Resolved Push Back system: the shared dynamic structure (fronts, modules, separators, derived posts, and the
    /// per-front load-beam elevations already computed with the 7/16"/ft slope and the 2" troquel snap) PLUS Push
    /// Back's own resolved bits — the high-end (rear) beam id and its PERALTE resolved PER FRONT AND LEVEL, the rear
    /// pallet-stop configuration, and the entrance-side safety selections with entrance GUIDES removed (Push Back has
    /// none). Longitudinal coordinates and elevations live on the composed <see cref="DynamicRackSystem"/>, so drawing
    /// and BOM consume one resolved model without recomputing the structure.
    /// </summary>
    public sealed class PushBackSystem
    {
        /// <summary>The resolved shared structure (identical to the dynamic system's resolved aggregate). Never null.</summary>
        public DynamicRackSystem Structure { get; set; } = new DynamicRackSystem();

        /// <summary>Resolved high-end (rear) load beam id (<c>LARGUERO_ESCALON_TROQUEL_REDONDO</c>).</summary>
        public string HighEndBeamCatalogId { get; set; } = PushBackDefaults.HighEndBeamCatalogId;

        /// <summary>Resolved high-end (rear) beam PERALTE by level, one entry per <see cref="DynamicRackSystem.Fronts"/> (aligned by index).</summary>
        public IList<PushBackResolvedFront> HighEndBeams { get; } = new List<PushBackResolvedFront>();

        /// <summary>Resolved rear pallet-stop configuration (active by default; drawing and BOM consume an independent copy).</summary>
        public PushBackRearTopeConfig RearTope { get; set; } = new PushBackRearTopeConfig();

        /// <summary>
        /// Resolved entrance-side safety selections. GUIA (entrance guides) are EXCLUDED — Push Back has no entrance
        /// guides, so a guide never reaches the plan, the BOM or a snapshot. Drawing/BOM consume independent copies.
        /// </summary>
        public IList<SelectiveSafetySelection> SafetySelections { get; } = new List<SelectiveSafetySelection>();

        /// <summary>Client-facing rack name (supplied by the DWG envelope at drawing time); mirrors the structure's name.</summary>
        public string Name
        {
            get => Structure?.Name;
            set { if (Structure != null) { Structure.Name = value; } }
        }

        /// <summary>Convenience pass-through: the resolved transverse fronts of the shared structure.</summary>
        public IList<DynamicRackFront> Fronts => Structure?.Fronts;

        /// <summary>Convenience pass-through: the shared low-end complete IN/OUT beam id (same as the dynamic system).</summary>
        public string InOutBeamCatalogId => Structure?.InOutBeamCatalogId;

        /// <summary>Convenience pass-through: total longitudinal length of the shared structure.</summary>
        public double TotalLength => Structure?.TotalLength ?? 0.0;

        /// <summary>
        /// The resolved high-end beam PERALTE at (<paramref name="frontIndex"/>, <paramref name="level"/> — 0-based).
        /// Falls back to the last resolved level, then the explicit 3.5 default, so a caller never reads a hole.
        /// </summary>
        public double HighEndBeamPeralteAt(int frontIndex, int level)
        {
            if (frontIndex >= 0 && frontIndex < HighEndBeams.Count)
            {
                var peraltes = HighEndBeams[frontIndex].HighEndBeamPeraltes;
                if (level >= 0 && level < peraltes.Count)
                {
                    return peraltes[level];
                }

                if (peraltes.Count > 0)
                {
                    return peraltes[peraltes.Count - 1];
                }
            }

            return PushBackDefaults.HighEndBeamDefaultPeralte;
        }
    }

    /// <summary>Resolved Push Back high-end (rear) beam PERALTE by load level for one front (index 0 = level 1).</summary>
    public sealed class PushBackResolvedFront
    {
        public IList<double> HighEndBeamPeraltes { get; } = new List<double>();
    }
}
