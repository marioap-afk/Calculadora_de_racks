using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Bill of materials for a standalone LARGUERO component (its profile + 2 ménsulas). Also the single home for the
    /// "one larguero = perfil + 2 ménsulas" recipe, reused by <see cref="SelectiveBomBuilder"/> so both agree. Pure.
    /// </summary>
    public static class LargueroBomBuilder
    {
        public const string Larguero = "Larguero";
        public const string Perfil = "Perfil";
        public const string Mensula = "Ménsula";

        /// <summary>The BOM of one larguero design: a single "Larguero" component expanding to its profile + ménsulas.</summary>
        public static BillOfMaterials Build(LargueroDesign design, RackCatalog catalog)
        {
            if (design == null || string.IsNullOrWhiteSpace(design.BeamProfileId))
            {
                return new BillOfMaterials(new List<BomComponent>());
            }

            var component = Component(catalog, design.BeamProfileId, design.Length, design.Peralte, design.MensulaOverride, quantity: 1);
            return new BillOfMaterials(new List<BomComponent> { component });
        }

        /// <summary>One larguero component present <paramref name="quantity"/> times: its profile (×1) + ménsulas (×2 per
        /// unit). The ménsula is <paramref name="mensulaOverride"/> when set, else the beam profile's catalog default.</summary>
        public static BomComponent Component(RackCatalog catalog, string beamId, double length, double peralte, string mensulaOverride, int quantity)
        {
            var beamLabel = DescribeBeam(catalog, beamId, peralte);

            var pieces = new List<BomLine>
            {
                new BomLine { Category = Perfil, ProfileId = beamId, Length = Math.Round(length, 2), Quantity = 1, Description = beamLabel }
            };

            var mensula = !string.IsNullOrWhiteSpace(mensulaOverride)
                ? mensulaOverride
                : catalog?.BeamProfiles.FirstOrDefault(b => string.Equals(b?.Id, beamId, StringComparison.OrdinalIgnoreCase))?.Mensula;
            if (!string.IsNullOrWhiteSpace(mensula))
            {
                pieces.Add(new BomLine { Category = Mensula, ProfileId = mensula, Length = 0.0, Quantity = 2, Description = DescribeMensula(catalog, mensula) });
            }

            return new BomComponent
            {
                Category = Larguero,
                ProfileId = beamId,
                Description = beamLabel,
                Length = Math.Round(length, 2),
                Quantity = quantity,
                Pieces = pieces
            };
        }

        public static string DescribeBeam(RackCatalog catalog, string id, double peralte)
        {
            var label = catalog?.BeamProfiles.FirstOrDefault(b => string.Equals(b?.Id, id, StringComparison.OrdinalIgnoreCase))?.Label ?? id;
            return peralte > 0.0 ? label + " · P" + peralte.ToString("0.###", CultureInfo.InvariantCulture) : label;
        }

        public static string DescribeMensula(RackCatalog catalog, string id)
            => catalog?.Mensulas.FirstOrDefault(m => string.Equals(m?.Id, id, StringComparison.OrdinalIgnoreCase))?.Label ?? id;
    }
}
