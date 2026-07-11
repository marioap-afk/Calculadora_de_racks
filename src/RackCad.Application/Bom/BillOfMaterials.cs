using System;
using System.Collections.Generic;
using System.Linq;

namespace RackCad.Application.Bom
{
    /// <summary>One aggregated material line: a profile of a given length and how many are needed.</summary>
    public sealed class BomLine
    {
        public string Category { get; set; }
        public string ProfileId { get; set; }
        public string Description { get; set; }
        public double Length { get; set; }
        public int Quantity { get; set; }
    }

    /// <summary>
    /// A COMPONENT of a system BOM: a sub-assembly (a cabecera, a larguero, a cama) present <see cref="Quantity"/>
    /// times, plus <see cref="Pieces"/> — the pieces that make up ONE unit of it (its recipe). A system BOM lists
    /// components; each expands to its pieces. Grouped by (Category, ProfileId, Length) like the piece lines.
    /// </summary>
    public sealed class BomComponent
    {
        public string Category { get; set; }
        public string ProfileId { get; set; }
        public string Description { get; set; }
        public double Length { get; set; }
        public int Quantity { get; set; }

        /// <summary>The pieces of ONE unit of this component (per-unit recipe). Flattened ×Quantity for the piece total.</summary>
        public IReadOnlyList<BomLine> Pieces { get; set; } = new List<BomLine>();
    }

    /// <summary>
    /// A bill of materials. A PIECE bom (a cabecera or a cama on its own) carries only flat <see cref="Lines"/>. A
    /// COMPONENT bom (a selective/dynamic system) carries <see cref="Components"/> — each a sub-assembly with its own
    /// piece recipe — AND a <see cref="Lines"/> view flattened to piece totals (Quantity × per-unit) for the CSV and
    /// the piece count. <see cref="IsComponentBased"/> tells the window which to show.
    /// </summary>
    public sealed class BillOfMaterials
    {
        /// <summary>A flat (piece-only) BOM — a single cabecera or cama.</summary>
        public BillOfMaterials(IReadOnlyList<BomLine> lines)
        {
            Lines = lines ?? new List<BomLine>();
            Components = new List<BomComponent>();
        }

        /// <summary>A component BOM — a system (selectivo/dinámico). <see cref="Lines"/> becomes the flattened piece total.</summary>
        public BillOfMaterials(IReadOnlyList<BomComponent> components)
        {
            Components = components ?? new List<BomComponent>();
            Lines = FlattenToPieceTotals(Components);
        }

        /// <summary>The piece lines: the whole BOM for a leaf, or the flattened piece total for a component BOM.</summary>
        public IReadOnlyList<BomLine> Lines { get; }

        /// <summary>The components (sub-assemblies) when this is a system BOM; empty for a piece-only BOM.</summary>
        public IReadOnlyList<BomComponent> Components { get; }

        public bool IsComponentBased => Components.Count > 0;

        public int TotalPieces => Lines.Sum(line => line.Quantity);

        /// <summary>Total number of component units (across all component groups); 0 for a piece-only BOM.</summary>
        public int TotalComponents => Components.Sum(c => c.Quantity);

        /// <summary>Sum each component's per-unit pieces ×Quantity into one flat piece total, grouped by (Category, ProfileId, Length).</summary>
        private static IReadOnlyList<BomLine> FlattenToPieceTotals(IReadOnlyList<BomComponent> components)
        {
            var byKey = new Dictionary<(string Category, string ProfileId, double Length), BomLine>();
            var order = new List<(string, string, double)>();
            foreach (var component in components)
            {
                foreach (var piece in component.Pieces)
                {
                    var key = (piece.Category ?? string.Empty, piece.ProfileId ?? string.Empty, Math.Round(piece.Length, 4));
                    if (!byKey.TryGetValue(key, out var line))
                    {
                        line = new BomLine
                        {
                            Category = piece.Category,
                            ProfileId = piece.ProfileId,
                            Description = piece.Description,
                            Length = piece.Length,
                            Quantity = 0
                        };
                        byKey[key] = line;
                        order.Add(key);
                    }

                    line.Quantity += piece.Quantity * component.Quantity;
                }
            }

            return order.Select(k => byKey[k]).ToList();
        }
    }
}
