using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RackCad.Application.Bom
{
    /// <summary>One rack's contribution to a drawing-wide BOM: its name, kind label, how many copies it has, and its
    /// own bill of materials (a component tree for a system, or flat pieces for a cama).</summary>
    public sealed class ConsolidatedRackBom
    {
        public string Name { get; set; }
        public string Kind { get; set; }
        public int Copies { get; set; } = 1;
        public BillOfMaterials Bom { get; set; }

        /// <summary>Piece count of this rack (for the breakdown grid).</summary>
        public int Piezas => Bom?.TotalPieces ?? 0;

        /// <summary>Component count of this rack (0 for a flat/cama BOM).</summary>
        public int Componentes => Bom?.TotalComponents ?? 0;
    }

    /// <summary>
    /// The whole drawing's bill of materials: a per-rack breakdown plus a GRAND TOTAL. The grand total is a COMPONENT
    /// BOM — every rack's components (cabeceras, largueros, camas) merged ×copies across the drawing — matching how each
    /// rack's own BOM reads. Its <see cref="BillOfMaterials.Lines"/> still exposes the flattened piece total.
    /// </summary>
    public sealed class ConsolidatedBom
    {
        public ConsolidatedBom(IReadOnlyList<ConsolidatedRackBom> racks, BillOfMaterials grandTotal)
        {
            Racks = racks ?? new List<ConsolidatedRackBom>();
            GrandTotal = grandTotal ?? new BillOfMaterials(new List<BomLine>());
        }

        public IReadOnlyList<ConsolidatedRackBom> Racks { get; }
        public BillOfMaterials GrandTotal { get; }

        public int RackCount => Racks.Count;
        public int TotalCopies => Racks.Sum(r => Math.Max(1, r.Copies));
    }

    public static class ConsolidatedBomBuilder
    {
        /// <summary>Assembles the per-rack list + a COMPONENT grand total (every rack's components merged ×copies). Racks
        /// with a null BOM are skipped. Identical components (same category + piece recipe) collapse across racks.</summary>
        public static ConsolidatedBom Build(IEnumerable<ConsolidatedRackBom> racks)
        {
            var list = (racks ?? Enumerable.Empty<ConsolidatedRackBom>())
                .Where(rack => rack != null && rack.Bom != null)
                .ToList();

            var byKey = new Dictionary<string, BomComponent>(StringComparer.Ordinal);
            var order = new List<string>();

            foreach (var rack in list)
            {
                var copies = Math.Max(1, rack.Copies);
                foreach (var component in ComponentsOf(rack))
                {
                    var key = (component.Category ?? string.Empty) + ""
                        + Math.Round(component.Length, 4).ToString("R", CultureInfo.InvariantCulture) + ""
                        + Signature(component.Pieces);
                    if (!byKey.TryGetValue(key, out var total))
                    {
                        total = new BomComponent
                        {
                            Category = component.Category,
                            ProfileId = component.ProfileId,
                            Description = component.Description,
                            Length = component.Length,
                            Quantity = 0,
                            Pieces = component.Pieces
                        };
                        byKey[key] = total;
                        order.Add(key);
                    }

                    total.Quantity += component.Quantity * copies;
                }
            }

            return new ConsolidatedBom(list, new BillOfMaterials(order.Select(k => byKey[k]).ToList()));
        }

        /// <summary>A rack's components: its own for a component BOM; for a flat BOM (cama, standalone cabecera) the whole
        /// rack is ONE component (kind-labelled) whose pieces are its lines.</summary>
        private static IReadOnlyList<BomComponent> ComponentsOf(ConsolidatedRackBom rack)
        {
            if (rack.Bom.IsComponentBased)
            {
                return rack.Bom.Components;
            }

            return new List<BomComponent>
            {
                new BomComponent
                {
                    Category = string.IsNullOrWhiteSpace(rack.Kind) ? "Rack" : rack.Kind,
                    ProfileId = string.Empty,
                    Description = rack.Name,
                    Length = 0.0,
                    Quantity = 1,
                    Pieces = rack.Bom.Lines
                }
            };
        }

        private static string Signature(IReadOnlyList<BomLine> pieces)
            => string.Join("|", (pieces ?? new List<BomLine>())
                .OrderBy(p => p.Category, StringComparer.Ordinal)
                .ThenBy(p => p.ProfileId, StringComparer.Ordinal)
                .ThenBy(p => p.Length)
                .Select(p => string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2:0.##}~{3}", p.Category, p.ProfileId, p.Length, p.Quantity)));
    }
}
