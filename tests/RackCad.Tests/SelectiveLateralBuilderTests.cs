using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>The selective lateral draws one cabecera per post at the frontal post Xs (so the views line up).</summary>
    public class SelectiveLateralBuilderTests
    {
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static SelectivePalletDesign TwoBayDesign()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId,
                PostPeralte = 3.0,
                PalletTolerance = 4.0,
                VerticalClearance = 6.0,
                PalletDepth = 48.0
            };

            for (var b = 0; b < 2; b++)
            {
                var bay = new SelectiveBayDesign();
                bay.Levels.Add(new SelectiveCell { Pallet = new Tarima { Frente = 40, Alto = 60 }, PalletCount = 2, BeamId = BeamId, BeamPeralte = 4.0 });
                bay.Levels.Add(new SelectiveCell { Pallet = new Tarima { Frente = 40, Alto = 60 }, PalletCount = 2, BeamId = BeamId, BeamPeralte = 4.0 });
                design.Bays.Add(bay);
            }

            return design;
        }

        [Fact]
        public void Build_PlacesOneCabeceraPerPost_AtTheFrontalPostXs()
        {
            var system = new SelectiveGeometryResolver().Resolve(TwoBayDesign(), Catalog);

            var plan = new SelectiveLateralBuilder().Build(system, Catalog);

            var placements = plan.Headers.SelectMany(h => h.Placements).ToList();
            Assert.Equal(3, placements.Count); // 2 frentes -> 3 postes

            var expected = SelectivePostGeometry.Compute(system, Catalog).PostXs.OrderBy(x => x).ToList();
            var placed = placements.Select(p => p.InsertionX).OrderBy(x => x).ToList();
            for (var i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i], placed[i], 4);
            }

            Assert.All(plan.Headers, h => Assert.NotEmpty(h.Instances)); // each cabecera actually has geometry
        }
    }
}
