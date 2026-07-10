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

            // El BOM cuenta la profundidad: cada cabecera es de frente Y atras (x2). 2 frentes -> 3 cabeceras ->
            // 6 postes + 6 placas; 2 x 2 niveles = 4 largueros frontales -> 8 fisicos; 2 ménsulas c/u = 16.
            Assert.Equal(6, Qty(bom, SelectiveBomBuilder.Post));
            Assert.Equal(6, Qty(bom, SelectiveBomBuilder.BasePlate));
            Assert.Equal(8, Qty(bom, SelectiveBomBuilder.Beam));
            Assert.Equal(16, Qty(bom, SelectiveBomBuilder.Mensula));
        }

        [Fact]
        public void Build_GroupsIdenticalLarguerosIntoOneLine()
        {
            var instances = new SelectiveFrontalBuilder().Build(TwoBaySystem(), Catalog);

            var bom = SelectiveBomBuilder.Build(instances, Catalog);

            var beamLines = bom.Lines.Where(l => l.Category == SelectiveBomBuilder.Beam).ToList();
            Assert.Single(beamLines); // all four share length 100 + peralte 4
            Assert.Equal(8, beamLines[0].Quantity); // 4 frontales x 2 (frente/atras)
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
            Assert.Equal(2, beamLines.Count); // peralte 4 (x3 -> x6) and peralte 5 (x1 -> x2)
            Assert.Equal(8, beamLines.Sum(l => l.Quantity));
        }

        [Fact]
        public void Build_DoubleDepth_CountsEveryFondo()
        {
            var instances = new SelectiveFrontalBuilder().Build(TwoBaySystem(), Catalog);

            var single = SelectiveBomBuilder.Build(instances, Catalog, depthCount: 1);
            var doble = SelectiveBomBuilder.Build(instances, Catalog, depthCount: 2);

            // Cada fondo extra repite postes/placas/largueros/ménsulas: doble profundidad = 2x el sencillo.
            Assert.Equal(2 * Qty(single, SelectiveBomBuilder.Post), Qty(doble, SelectiveBomBuilder.Post));
            Assert.Equal(2 * Qty(single, SelectiveBomBuilder.Beam), Qty(doble, SelectiveBomBuilder.Beam));
            Assert.Equal(12, Qty(doble, SelectiveBomBuilder.Post)); // 3 cabeceras x2 (frente/atras) x2 fondos
            Assert.Equal(16, Qty(doble, SelectiveBomBuilder.Beam)); // 4 frontales x2 x2
        }

        private static int Qty(Application.Bom.BillOfMaterials bom, string category)
            => bom.Lines.Where(l => l.Category == category).Sum(l => l.Quantity);
    }
}
