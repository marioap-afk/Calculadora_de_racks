using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>Aggregating a resolved selective rack's placed instances into a bill of materials.</summary>
    public class SelectiveBomBuilderTests
    {
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static SelectiveRackSystem TwoBaySystem()
        {
            var system = new SelectiveRackSystem { Height = 240.0, PostId = PostId, PostPeralte = 3.0 };
            for (var b = 0; b < 2; b++)
            {
                var bay = new SelectiveBay { BeamLength = 100.0, Height = 240.0 };
                bay.Levels.Add(new SelectiveLevel { Y = 48.0, BeamId = BeamId, BeamPeralte = 4.0 });
                bay.Levels.Add(new SelectiveLevel { Y = 96.0, BeamId = BeamId, BeamPeralte = 4.0 });
                system.Bays.Add(bay);
            }

            return system;
        }

        [Fact]
        public void Build_CountsPostsPlatesBeamsAndMensulas()
        {
            var instances = new SelectiveFrontalBuilder().Build(TwoBaySystem(), Catalog);

            var bom = SelectiveBomBuilder.Build(instances, Catalog);

            // 2 frentes -> 3 postes + 3 placas; 2 x 2 niveles = 4 largueros; 2 ménsulas c/u = 8.
            Assert.Equal(3, Qty(bom, SelectiveBomBuilder.Post));
            Assert.Equal(3, Qty(bom, SelectiveBomBuilder.BasePlate));
            Assert.Equal(4, Qty(bom, SelectiveBomBuilder.Beam));
            Assert.Equal(8, Qty(bom, SelectiveBomBuilder.Mensula));
        }

        [Fact]
        public void Build_GroupsIdenticalLarguerosIntoOneLine()
        {
            var instances = new SelectiveFrontalBuilder().Build(TwoBaySystem(), Catalog);

            var bom = SelectiveBomBuilder.Build(instances, Catalog);

            var beamLines = bom.Lines.Where(l => l.Category == SelectiveBomBuilder.Beam).ToList();
            Assert.Single(beamLines); // all four share length 100 + peralte 4
            Assert.Equal(4, beamLines[0].Quantity);
            Assert.Equal(100.0, beamLines[0].Length, 4);
        }

        [Fact]
        public void Build_SeparatesLarguerosByPeralte()
        {
            var system = TwoBaySystem();
            system.Bays[1].Levels[0].BeamPeralte = 5.0; // one different-peralte larguero

            var instances = new SelectiveFrontalBuilder().Build(system, Catalog);
            var bom = SelectiveBomBuilder.Build(instances, Catalog);

            var beamLines = bom.Lines.Where(l => l.Category == SelectiveBomBuilder.Beam).ToList();
            Assert.Equal(2, beamLines.Count); // peralte 4 (x3) and peralte 5 (x1)
            Assert.Equal(4, beamLines.Sum(l => l.Quantity));
        }

        private static int Qty(Application.Bom.BillOfMaterials bom, string category)
            => bom.Lines.Where(l => l.Category == category).Sum(l => l.Quantity);
    }
}
