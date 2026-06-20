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

    public sealed class BillOfMaterials
    {
        public BillOfMaterials(IReadOnlyList<BomLine> lines)
        {
            Lines = lines ?? new List<BomLine>();
        }

        public IReadOnlyList<BomLine> Lines { get; }

        public int TotalPieces => Lines.Sum(line => line.Quantity);
    }
}
