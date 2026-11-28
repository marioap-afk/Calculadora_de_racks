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
                    var key = KeyOf(component);
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

        /// <summary>
        /// T4 bom (BM-08): where each component of the grand total comes from, aligned with <see cref="ConsolidatedBom.GrandTotal"/>: the
        /// racks that add to it (each with its quantity in ONE copy and its copies) and the distinct descriptions that the merge joins (the
        /// key is the category, the length and the recipe, without profile or description, and the total keeps the first description:
        /// finding of I-68). It recomputes nothing of the sum; the page says what the total already does.
        /// </summary>
        public static IReadOnlyList<ConsolidatedComponentProvenance> Provenance(ConsolidatedBom consolidated)
        {
            var result = new List<ConsolidatedComponentProvenance>();
            if (consolidated == null) return result;

            var index = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var rack in consolidated.Racks.Where(rack => rack != null && rack.Bom != null))
            {
                var copies = Math.Max(1, rack.Copies);
                foreach (var component in ComponentsOf(rack))
                {
                    var key = KeyOf(component);
                    if (!index.TryGetValue(key, out var at))
                    {
                        at = result.Count;
                        index[key] = at;
                        result.Add(new ConsolidatedComponentProvenance(new List<ConsolidatedComponentSource>(), new List<string>()));
                    }

                    var provenance = result[at];
                    ((List<ConsolidatedComponentSource>)provenance.Sources).Add(new ConsolidatedComponentSource(rack.Name, component.Quantity, copies));
                    var description = component.Description ?? string.Empty;
                    if (!provenance.Descriptions.Contains(description, StringComparer.Ordinal)) ((List<string>)provenance.Descriptions).Add(description);
                }
            }

            return result;
        }

        // The key of the merge of the grand total: category, length and recipe (the separator is the control character U+0001).
        private static string KeyOf(BomComponent component)
            => (component.Category ?? string.Empty) + KeySeparator
               + Math.Round(component.Length, 4).ToString("R", CultureInfo.InvariantCulture) + KeySeparator
               + Signature(component.Pieces);

        private static readonly string KeySeparator = ((char)0x01).ToString();

        private static string Signature(IReadOnlyList<BomLine> pieces)
            => string.Join("|", (pieces ?? new List<BomLine>())
                .OrderBy(p => p.Category, StringComparer.Ordinal)
                .ThenBy(p => p.ProfileId, StringComparer.Ordinal)
                .ThenBy(p => p.Length)
                .Select(p => string.Format(CultureInfo.InvariantCulture, "{0}~{1}~{2:0.##}~{3}", p.Category, p.ProfileId, p.Length, p.Quantity)));
    }

    /// <summary>One rack that adds to a component of the grand total: its name, its quantity in ONE copy and its copies (T4 bom, BM-08).</summary>
    public sealed record ConsolidatedComponentSource(string RackName, int QuantityPerCopy, int Copies);

    /// <summary>Where one component of the grand total comes from, and the distinct descriptions it joins (T4 bom, BM-08).</summary>
    public sealed record ConsolidatedComponentProvenance(IReadOnlyList<ConsolidatedComponentSource> Sources, IReadOnlyList<string> Descriptions)
    {
        /// <summary>The merge joined components with different descriptions (finding of I-68): the row says so; the sum does not change.</summary>
        public bool JoinsDescriptions => Descriptions.Count > 1;
    }
}
