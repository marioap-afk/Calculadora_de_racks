using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>Exercises the four pallet-driven derivation rules (larguero length, separation, height, rounding).</summary>
    public class SelectiveGeometryResolverTests
    {
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        /// <summary>The troquel grid base (first troquel Y), from the catalog — the resolver snaps the first level to it.</summary>
        private static double GridBase()
            => Catalog.ConnectionLayout.FindConnectionLayout(PostId, "TROQUEL_LARGUERO", "FRONTAL").LocalY;

        private static double Snap(double v, double baseY, double paso)
            => baseY + Math.Round((v - baseY) / paso, MidpointRounding.AwayFromZero) * paso;

        private static double RoundUpToFoot(double x) => Math.Ceiling(x / 12.0 - 1e-9) * 12.0;

        private static SelectiveCell Cell(double frente, double alto, int count, double beamPeralte)
            => new SelectiveCell
            {
                Pallet = new Tarima { Frente = frente, Alto = alto },
                PalletCount = count,
                BeamId = BeamId,
                BeamPeralte = beamPeralte
            };

        private static SelectivePalletDesign Design(double firstLevel, params SelectiveCell[] levels)
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId,
                PostPeralte = 3.0,
                PalletTolerance = 4.0,
                VerticalClearance = 6.0,
                FirstLevel = firstLevel
            };

            var bay = new SelectiveBayDesign();
            foreach (var level in levels)
            {
                bay.Levels.Add(level);
            }

            design.Bays.Add(bay);
            return design;
        }

        [Fact]
        public void BeamLength_IsFrenteTimesCountPlusTolerancePerGap()
        {
            var system = new SelectiveGeometryResolver().Resolve(Design(6.0, Cell(40, 60, 2, 4)), Catalog);

            // 2 tarimas de 40" + tolerancia 4" en 3 huecos (izq, entre, der) = 40*2 + 4*3 = 92.
            Assert.Equal(92.0, system.Bays[0].BeamLength, 4);
        }

        [Fact]
        public void BeamLength_WidestLevelGovernsTheBay()
        {
            var system = new SelectiveGeometryResolver().Resolve(
                Design(6.0, Cell(40, 60, 1, 4), Cell(48, 50, 2, 4)), Catalog);

            // nivel 0: 40 + 4*2 = 48 ; nivel 1: 48*2 + 4*3 = 108 -> gana el más ancho.
            Assert.Equal(108.0, system.Bays[0].BeamLength, 4);
        }

        [Fact]
        public void FirstLevel_SnapsToTheTroquelGrid()
        {
            var system = new SelectiveGeometryResolver().Resolve(Design(6.0, Cell(40, 60, 1, 4)), Catalog);

            Assert.Equal(Snap(6.0, GridBase(), 2.0), system.Bays[0].Levels[0].Y, 4);
        }

        [Fact]
        public void Separation_IsClearOpeningPlusPeralteRoundedUpToTroquel()
        {
            // claro libre = redondearPar(60 + 6) = 66 ; + peralte 4 = 70 (par) -> separación 70.
            var system = new SelectiveGeometryResolver().Resolve(
                Design(6.0, Cell(40, 60, 1, 4), Cell(40, 60, 1, 4)), Catalog);

            var sep = system.Bays[0].Levels[1].Y - system.Bays[0].Levels[0].Y;
            Assert.Equal(70.0, sep, 4);
        }

        [Fact]
        public void Separation_RoundsBothClearOpeningAndPitchUpwardToEven()
        {
            // claro libre = redondearPar(59 + 6 = 65) = 66 ; + peralte 3 = 69 -> redondearTroquel arriba = 70.
            var system = new SelectiveGeometryResolver().Resolve(
                Design(6.0, Cell(40, 59, 1, 3), Cell(40, 59, 1, 3)), Catalog);

            var sep = system.Bays[0].Levels[1].Y - system.Bays[0].Levels[0].Y;
            Assert.Equal(70.0, sep, 4);
        }

        [Fact]
        public void PostHeight_IsTopLevelPlusPalletThirdRoundedUpToFoot()
        {
            var system = new SelectiveGeometryResolver().Resolve(
                Design(6.0, Cell(40, 60, 1, 4), Cell(40, 60, 1, 4)), Catalog);

            var topY = system.Bays[0].Levels.Last().Y;
            Assert.Equal(RoundUpToFoot(topY + 60.0 / 3.0), system.Height, 4);
        }

        [Fact]
        public void Height_TakesTheTallestBayForUniformPosts()
        {
            var design = new SelectivePalletDesign { PostId = PostId, PostPeralte = 3.0, FirstLevel = 6.0 };

            var shortBay = new SelectiveBayDesign();
            shortBay.Levels.Add(Cell(40, 40, 1, 4));

            var tallBay = new SelectiveBayDesign();
            tallBay.Levels.Add(Cell(40, 40, 1, 4));
            tallBay.Levels.Add(Cell(40, 40, 1, 4));

            design.Bays.Add(shortBay);
            design.Bays.Add(tallBay);

            var system = new SelectiveGeometryResolver().Resolve(design, Catalog);

            var tallTop = system.Bays[1].Levels.Last().Y + 40.0 / 3.0;
            Assert.Equal(RoundUpToFoot(tallTop), system.Height, 4);
        }
    }
}
