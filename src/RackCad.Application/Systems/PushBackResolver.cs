using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Pure design→system boundary for Push Back. It REUSES <see cref="DynamicRackSystemResolver"/> for the shared
    /// structure (headers, separators, derived posts, per-front depth ranges, and the load-beam elevations already
    /// computed with the 7/16"/ft slope and the 2" troquel snap) and then resolves Push Back's OWN bits: the explicit
    /// high-end beam PERALTE (default 3.5, validated against the catalog — NOT "the first catalog row") and the rear
    /// pallet-stop selection. No dynamic behaviour is altered: the dynamic resolver is composed, not modified.
    /// </summary>
    public sealed class PushBackResolver
    {
        private readonly RackCatalog catalog;
        private readonly DynamicRackSystemResolver structureResolver;

        public PushBackResolver(RackCatalog catalog)
        {
            this.catalog = catalog ?? new RackCatalog();
            structureResolver = new DynamicRackSystemResolver(this.catalog);
        }

        public PushBackSystem Resolve(PushBackDesign design)
        {
            if (design == null)
            {
                throw new ArgumentNullException(nameof(design));
            }

            var structure = structureResolver.Resolve(design.Structure ?? new DynamicRackDesign()).System;

            return new PushBackSystem
            {
                Structure = structure,
                HighEndBeamCatalogId = PushBackDefaults.HighEndBeamCatalogId,
                HighEndBeamPeralte = ResolveHighEndBeamPeralte(design.HighEndBeamPeralte),
                RearTope = design.RearTope?.DeepCopy() ?? new PushBackRearTopeConfig()
            };
        }

        /// <summary>
        /// The high-end beam PERALTE: the requested value if the catalog allows it, otherwise the EXPLICIT default 3.5
        /// (when the catalog allows it), otherwise the first catalog value. 3.5 is a rule, never silently "allowed[0]".
        /// </summary>
        public double ResolveHighEndBeamPeralte(double requested)
        {
            var allowed = AllowedHighEndPeraltes();
            bool InList(double value) => allowed.Any(candidate => Math.Abs(candidate - value) < 1e-6);

            if (requested > 0.0 && InList(requested))
            {
                return requested;
            }

            if (InList(PushBackDefaults.HighEndBeamDefaultPeralte))
            {
                return PushBackDefaults.HighEndBeamDefaultPeralte;
            }

            return allowed.Count > 0 ? allowed[0] : PushBackDefaults.HighEndBeamDefaultPeralte;
        }

        /// <summary>Catalog-allowed peraltes of the high-end beam (LARGUERO_ESCALON_TROQUEL_REDONDO), read like the intermediate beam.</summary>
        public IReadOnlyList<double> AllowedHighEndPeraltes()
        {
            var profile = catalog?.BeamProfiles?.FirstOrDefault(entry => string.Equals(
                entry?.Id,
                PushBackDefaults.HighEndBeamCatalogId,
                StringComparison.OrdinalIgnoreCase));
            return PeralteList.Parse(profile?.Peraltes);
        }
    }
}
