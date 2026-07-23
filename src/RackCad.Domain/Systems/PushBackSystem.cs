using System.Collections.Generic;

namespace RackCad.Domain.Systems
{
    /// <summary>
    /// Resolved Push Back system: the shared dynamic structure (fronts, modules, separators, derived posts, and the
    /// per-front load-beam elevations already computed with the 7/16"/ft slope and the 2" troquel snap) PLUS Push
    /// Back's own resolved bits — the high-end (rear) beam id/peralte and the rear pallet-stop configuration.
    /// Longitudinal coordinates and elevations live on the composed <see cref="DynamicRackSystem"/>, so drawing and
    /// BOM consume one resolved model without recomputing the structure.
    /// </summary>
    public sealed class PushBackSystem
    {
        /// <summary>The resolved shared structure (identical to the dynamic system's resolved aggregate). Never null.</summary>
        public DynamicRackSystem Structure { get; set; } = new DynamicRackSystem();

        /// <summary>Resolved high-end (rear) load beam id.</summary>
        public string HighEndBeamCatalogId { get; set; } = PushBackDefaults.HighEndBeamCatalogId;

        /// <summary>Resolved high-end (rear) load beam PERALTE (in), validated against the catalog (default 3.5).</summary>
        public double HighEndBeamPeralte { get; set; } = PushBackDefaults.HighEndBeamDefaultPeralte;

        /// <summary>Resolved rear pallet-stop configuration (active by default; drawing and BOM consume an independent copy).</summary>
        public PushBackRearTopeConfig RearTope { get; set; } = new PushBackRearTopeConfig();

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
    }
}
