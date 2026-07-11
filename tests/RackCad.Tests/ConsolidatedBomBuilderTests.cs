using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Bom;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>The whole-drawing consolidated BOM: per-rack breakdown + a grand total merged ×copies.</summary>
    public class ConsolidatedBomBuilderTests
    {
        private static BomLine Line(string category, string id, int qty, double length = 0.0)
            => new BomLine { Category = category, ProfileId = id, Quantity = qty, Length = length };

        private static BomComponent Comp(string category, string id, int qty, params BomLine[] pieces)
            => new BomComponent { Category = category, ProfileId = id, Description = category, Length = 0.0, Quantity = qty, Pieces = pieces.ToList() };

        [Fact]
        public void Build_MergesPieceTotalsAcrossRacks_TimesCopies()
        {
            // Rack A (×2 copies): 3 largueros, each = 1 Perfil + 2 Ménsulas → Lines: Perfil ×3, Ménsula ×6.
            var rackA = new ConsolidatedRackBom
            {
                Name = "A", Kind = "Selectivo", Copies = 2,
                Bom = new BillOfMaterials(new List<BomComponent> { Comp("Larguero", "L", 3, Line("Perfil", "L", 1), Line("Ménsula", "M", 2)) })
            };
            // Rack B (×1): a flat cama BOM.
            var rackB = new ConsolidatedRackBom
            {
                Name = "B", Kind = "Cama", Copies = 1,
                Bom = new BillOfMaterials(new List<BomLine> { Line("Rodillo", "R", 10) })
            };

            var consolidated = ConsolidatedBomBuilder.Build(new[] { rackA, rackB });

            Assert.Equal(2, consolidated.RackCount);
            Assert.Equal(3, consolidated.TotalCopies); // 2 + 1

            // The grand total is BY COMPONENT: Larguero ×(3×2)=6 (from rack A), Cama ×1 (rack B wrapped as one component).
            Assert.True(consolidated.GrandTotal.IsComponentBased);
            Assert.Contains(consolidated.GrandTotal.Components, c => c.Category == "Larguero" && c.Quantity == 6);
            Assert.Contains(consolidated.GrandTotal.Components, c => c.Category == "Cama" && c.Quantity == 1);

            // Its flattened piece total still adds up: Perfil 1×6, Ménsula 2×6, Rodillo 10×1.
            int Total(string category) => consolidated.GrandTotal.Lines.Where(l => l.Category == category).Sum(l => l.Quantity);
            Assert.Equal(6, Total("Perfil"));   // 3 × 2 copies
            Assert.Equal(12, Total("Ménsula")); // 6 × 2 copies
            Assert.Equal(10, Total("Rodillo")); // 10 × 1 copy
        }

        [Fact]
        public void Csv_HasPerRackRowsAndGrandTotal()
        {
            var rack = new ConsolidatedRackBom
            {
                Name = "A", Kind = "Selectivo", Copies = 1,
                Bom = new BillOfMaterials(new List<BomComponent> { Comp("Larguero", "L", 2, Line("Perfil", "L", 1), Line("Ménsula", "M", 2)) })
            };

            var csv = ConsolidatedBomCsvExporter.ToCsv(ConsolidatedBomBuilder.Build(new[] { rack }));

            Assert.Contains("TOTAL DEL DIBUJO", csv);
            Assert.Contains("Componente", csv);
            Assert.Contains("Pieza", csv);
        }
    }
}
