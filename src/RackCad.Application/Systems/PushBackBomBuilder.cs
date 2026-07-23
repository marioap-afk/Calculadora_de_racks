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
    /// The Push Back component BOM. It reuses <see cref="SystemBomBuilder.Build"/> as a BLACK BOX for the shared structural
    /// components (cabeceras, separators, derived/reinforced posts, plates, intermediate beams, and the GUIA-free safety),
    /// then SUBSTITUTES the pallet-flow-specific categories: it drops the dynamic IN/OUT-beam category (two beams per level)
    /// and the dynamic bed (length − 4"), and adds — per front and level — ONE low IN/OUT beam and ONE high
    /// <c>LARGUERO_ESCALON_TROQUEL_REDONDO</c>, one OPAQUE bed per lane and level at the FULL structural span, and one rear
    /// tope per ACTIVE cell. No second IN/OUT, no −4" bed, no brakes, no guides, and no double counting between views.
    /// </summary>
    public static class PushBackBomBuilder
    {
        public const string HighEndBeam = "Larguero troquel redondo";
        public const string RearTope = "Tope posterior";

        public static BillOfMaterials Build(PushBackSystem system, RackCatalog catalog)
        {
            var structure = system?.Structure;
            if (structure == null)
            {
                return new BillOfMaterials(new List<BomComponent>());
            }

            // Shared structure from the dynamic BOM, minus the dynamic-flow-specific categories we replace.
            var components = SystemBomBuilder.Build(structure, catalog).Components
                .Where(component => component != null
                    && component.Category != SystemBomBuilder.InOutBeam
                    && component.Category != SystemBomBuilder.Cama)
                .Select(Clone)
                .ToList();

            AddEndBeams(components, system, catalog, SystemBomBuilder.InOutBeam, isHighEnd: false);
            AddEndBeams(components, system, catalog, HighEndBeam, isHighEnd: true);
            AddBeds(components, system);
            AddRearTopes(components, system, catalog);

            return new BillOfMaterials(components);
        }

        /// <summary>
        /// Low IN/OUT (one per front x level, resolved PER CELL via <see cref="DynamicRackLevelGeometry.At"/> — id, peralte
        /// and length can differ by cell) or high TROQUEL_REDONDO (one per front x level, peralte per cell, transverse
        /// LONGITUD = the corresponding IN/OUT's). Grouped by ProfileId, length and peralte.
        /// </summary>
        private static void AddEndBeams(ICollection<BomComponent> components, PushBackSystem system, RackCatalog catalog, string category, bool isHighEnd)
        {
            var structure = system.Structure;
            var highId = string.IsNullOrWhiteSpace(system.HighEndBeamCatalogId) ? PushBackDefaults.HighEndBeamCatalogId : system.HighEndBeamCatalogId;

            var grouped = new Dictionary<(string BeamId, double Length, double Peralte), int>();
            for (var frontIndex = 0; frontIndex < structure.Fronts.Count; frontIndex++)
            {
                var front = structure.Fronts[frontIndex];
                for (var level = 0; level < Math.Max(1, front.LoadLevels); level++)
                {
                    string beamId;
                    double peralte;
                    double length;
                    // Both the low IN/OUT and the high TROQUEL_REDONDO of a cell share the SAME transverse length,
                    // resolved per front and level (never front.BeamLength directly for every level).
                    length = PushBackLoadBeamGeometry.CellBeamLength(structure, front, level + 1);
                    if (isHighEnd)
                    {
                        beamId = highId;
                        peralte = system.HighEndBeamPeralteAt(frontIndex, level);
                    }
                    else
                    {
                        var configuration = DynamicRackLevelGeometry.At(structure, front, level + 1);
                        beamId = string.IsNullOrWhiteSpace(configuration.InOutBeamCatalogId)
                            ? (string.IsNullOrWhiteSpace(structure.InOutBeamCatalogId) ? DynamicRackDefaults.InOutBeamCatalogId : structure.InOutBeamCatalogId)
                            : configuration.InOutBeamCatalogId;
                        peralte = configuration.InOutBeamDepth > 0.0 ? configuration.InOutBeamDepth : structure.InOutBeamDepth;
                    }

                    var key = (beamId, Round(length), Round(peralte));
                    grouped[key] = grouped.TryGetValue(key, out var current) ? current + 1 : 1;
                }
            }

            foreach (var group in grouped.OrderBy(g => g.Key.BeamId, StringComparer.OrdinalIgnoreCase).ThenBy(g => g.Key.Length).ThenBy(g => g.Key.Peralte))
            {
                var label = catalog?.BeamProfiles?.FirstOrDefault(entry => string.Equals(entry?.Id, group.Key.BeamId, StringComparison.OrdinalIgnoreCase))?.Label ?? group.Key.BeamId;
                var description = string.Format(CultureInfo.InvariantCulture, "{0} · Peralte {1:0.##}\"", label, group.Key.Peralte);
                components.Add(new BomComponent
                {
                    Category = category,
                    ProfileId = group.Key.BeamId,
                    Description = description,
                    Length = group.Key.Length,
                    Quantity = group.Value,
                    Pieces = new List<BomLine> { new BomLine { Category = category, ProfileId = group.Key.BeamId, Description = description, Length = group.Key.Length, Quantity = 1 } }
                });
            }
        }

        /// <summary>One OPAQUE bed per lane and level (its rail/roller recipe is not exploded), length = the full structural span.</summary>
        private static void AddBeds(ICollection<BomComponent> components, PushBackSystem system)
        {
            var structure = system.Structure;
            var grouped = new Dictionary<double, int>();
            foreach (var front in structure.Fronts)
            {
                var length = Round(PushBackFlowBedLateralBuilder.ResolveBedLength(system, front));
                if (length <= 0.0)
                {
                    continue;
                }

                var perLane = Math.Max(1, front.PalletCount) * Math.Max(1, front.LoadLevels);
                grouped[length] = grouped.TryGetValue(length, out var current) ? current + perLane : perLane;
            }

            foreach (var group in grouped.OrderBy(g => g.Key))
            {
                components.Add(new BomComponent
                {
                    Category = SystemBomBuilder.Cama,
                    ProfileId = SystemBomBuilder.Cama,
                    Description = SystemBomBuilder.Cama,
                    Length = group.Key,
                    Quantity = group.Value,
                    Pieces = new List<BomLine>() // opaque: no rail/roller explosion
                });
            }
        }

        /// <summary>One rear tope (LARGUERO_ESCALON_TOPE_DE_3) per ACTIVE cell.</summary>
        private static void AddRearTopes(ICollection<BomComponent> components, PushBackSystem system, RackCatalog catalog)
        {
            var structure = system.Structure;
            var rearTope = system.RearTope ?? new PushBackRearTopeConfig();
            var topeId = PushBackRearTopeBuilder.TopePieceId;
            var label = catalog?.SafetyElements?.FirstOrDefault(entry => string.Equals(entry?.Id, topeId, StringComparison.OrdinalIgnoreCase))?.Label ?? topeId;

            var grouped = new Dictionary<double, int>();
            for (var frontIndex = 0; frontIndex < structure.Fronts.Count; frontIndex++)
            {
                var front = structure.Fronts[frontIndex];
                for (var level = 0; level < Math.Max(1, front.LoadLevels); level++)
                {
                    if (!rearTope.At(frontIndex, level))
                    {
                        continue;
                    }

                    // Commercial LONGITUD = the cell's transverse beam length (per front x level) + the allowance —
                    // exactly what the lateral/frontal/planta tope blocks carry.
                    var length = Round(PushBackLoadBeamGeometry.CellBeamLength(structure, front, level + 1) + SelectiveTopePlacement.LengthAllowance);
                    grouped[length] = grouped.TryGetValue(length, out var current) ? current + 1 : 1;
                }
            }

            foreach (var group in grouped.OrderBy(g => g.Key))
            {
                components.Add(new BomComponent
                {
                    Category = RearTope,
                    ProfileId = topeId,
                    Description = label,
                    Length = group.Key,
                    Quantity = group.Value,
                    Pieces = new List<BomLine> { new BomLine { Category = RearTope, ProfileId = topeId, Description = label, Length = group.Key, Quantity = 1 } }
                });
            }
        }

        private static BomComponent Clone(BomComponent source)
            => new BomComponent
            {
                Category = source.Category,
                ProfileId = source.ProfileId,
                Description = source.Description,
                Length = source.Length,
                Quantity = source.Quantity,
                Pieces = source.Pieces.Select(piece => new BomLine
                {
                    Category = piece.Category,
                    ProfileId = piece.ProfileId,
                    Description = piece.Description,
                    Length = piece.Length,
                    Quantity = piece.Quantity
                }).ToList()
            };

        private static double Round(double value) => Math.Round(value, 4);
    }
}
