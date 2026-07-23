using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-18a — PushBackBomBuilder (item 5). Composes the dynamic BOM as a black box for the shared structure and
    /// substitutes the pallet-flow-specific categories: one low IN/OUT + one high TROQUEL_REDONDO per front x level, one
    /// opaque full-span bed per lane x level, and one rear tope per active cell — with NO second dynamic IN/OUT, NO −4"
    /// bed, NO brakes, NO guides, and no double counting.
    /// </summary>
    public class PushBackBomTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackDesign Design()
            => new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 4,
                    LoadLevels = 3,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            };

        private static PushBackSystem System(RackCatalog catalog) => new PushBackResolver(catalog).Resolve(Design());

        private static int Cells(PushBackSystem system) => system.Structure.Fronts.Sum(front => Math.Max(1, front.LoadLevels));

        [Fact]
        public void Bom_OneInOutAndOneRedondoPerCell_NoSecondInOut()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var bom = PushBackBomBuilder.Build(system, catalog);

            var inOut = bom.Components.Where(c => c.Category == SystemBomBuilder.InOutBeam).Sum(c => c.Quantity);
            var redondo = bom.Components.Where(c => c.Category == PushBackBomBuilder.HighEndBeam).Sum(c => c.Quantity);

            Assert.Equal(Cells(system), inOut);       // one IN/OUT per cell (NOT the dynamic two-per-level)
            Assert.Equal(Cells(system), redondo);     // one high TROQUEL_REDONDO per cell
        }

        [Fact]
        public void Bom_Cama_IsFullSpan_NotMinus4_OnePerLaneAndLevel()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var bom = PushBackBomBuilder.Build(system, catalog);

            var cama = bom.Components.Where(c => c.Category == SystemBomBuilder.Cama).ToList();
            Assert.NotEmpty(cama);
            Assert.All(cama, c => Assert.Equal(system.TotalLength, c.Length, 3));   // full span, NOT span − 4"
            Assert.All(cama, c => Assert.Empty(c.Pieces));                          // opaque

            var lanes = system.Structure.Fronts.Sum(f => Math.Max(1, f.PalletCount) * Math.Max(1, f.LoadLevels));
            Assert.Equal(lanes, cama.Sum(c => c.Quantity));
        }

        [Fact]
        public void Bom_HasNoBrakesNoGuides()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var bom = PushBackBomBuilder.Build(system, catalog);

            Assert.DoesNotContain(bom.Lines, l => (l.ProfileId ?? string.Empty).Contains("FRENO"));
            Assert.DoesNotContain(bom.Lines, l => (l.ProfileId ?? string.Empty).Contains("RODILLO"));   // opaque bed hides the recipe
            Assert.DoesNotContain(bom.Lines, l => (l.ProfileId ?? string.Empty).Contains("GUIA"));
        }

        [Fact]
        public void Bom_RearTopes_ActiveByDefault_DropWhenDeactivated()
        {
            var catalog = Catalog;
            var system = System(catalog);

            var before = PushBackBomBuilder.Build(system, catalog).Components
                .Where(c => c.Category == PushBackBomBuilder.RearTope).Sum(c => c.Quantity);
            Assert.Equal(Cells(system), before);   // active by default, one per cell

            system.RearTope.Disable(0, 0);
            var after = PushBackBomBuilder.Build(system, catalog).Components
                .Where(c => c.Category == PushBackBomBuilder.RearTope).Sum(c => c.Quantity);
            Assert.Equal(before - 1, after);
        }

        [Fact]
        public void Bom_KeepsSharedStructure_SeparatorsAndCabeceraPosts()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var bom = PushBackBomBuilder.Build(system, catalog);

            // The shared structure survives the black-box composition.
            Assert.Contains(bom.Lines, l => (l.ProfileId ?? string.Empty).Contains("POSTE"));
            Assert.Contains(bom.Components, c => c.Category == SystemBomBuilder.Separator);
        }
    }
}
