using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>A standalone larguero component: one profile + two ménsulas, as a component BOM.</summary>
    public class LargueroBomBuilderTests
    {
        private const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        [Fact]
        public void Build_OneLargueroComponent_ProfilePlusTwoMensulas()
        {
            var design = new LargueroDesign { BeamProfileId = BeamId, Peralte = 4.0, Length = 96.0 };

            var bom = LargueroBomBuilder.Build(design, Catalog);

            Assert.True(bom.IsComponentBased);
            var larguero = Assert.Single(bom.Components);
            Assert.Equal(LargueroBomBuilder.Larguero, larguero.Category);
            Assert.Equal(1, larguero.Quantity);
            Assert.Equal(96.0, larguero.Length, 4);
            Assert.Equal(1, larguero.Pieces.Where(p => p.Category == LargueroBomBuilder.Perfil).Sum(p => p.Quantity));
            Assert.Equal(2, larguero.Pieces.Where(p => p.Category == LargueroBomBuilder.Mensula).Sum(p => p.Quantity));
        }

        [Fact]
        public void Build_MensulaOverride_WinsOverCatalogDefault()
        {
            var design = new LargueroDesign { BeamProfileId = BeamId, Peralte = 4.0, Length = 96.0, MensulaOverride = "MENSULA_CUSTOM" };

            var bom = LargueroBomBuilder.Build(design, Catalog);

            var mensula = bom.Components.Single().Pieces.Single(p => p.Category == LargueroBomBuilder.Mensula);
            Assert.Equal("MENSULA_CUSTOM", mensula.ProfileId);
        }

        [Fact]
        public void Build_NoBeamProfile_IsEmpty()
        {
            var bom = LargueroBomBuilder.Build(new LargueroDesign(), Catalog);

            Assert.False(bom.IsComponentBased);
            Assert.Empty(bom.Components);
        }
    }
}
