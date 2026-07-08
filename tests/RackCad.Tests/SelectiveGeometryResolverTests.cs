using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>Exercises the pallet-driven derivation: larguero length, floor-referenced stacking, separation, height.</summary>
    public class SelectiveGeometryResolverTests
    {
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        /// <summary>The troquel grid base (lowest troquel Y), from the catalog — beams snap onto this grid.</summary>
        private static double GridBase()
            => Catalog.ConnectionLayout.FindConnectionLayout(PostId, "TROQUEL_LARGUERO", "FRONTAL").LocalY;

        private static double RoundUp(double x, double m) => Math.Ceiling(x / m - 1e-9) * m;
        private static double RoundUpFoot(double x) => RoundUp(x, 12.0);
        private static double SnapUp(double v) => GridBase() + RoundUp(v - GridBase(), 2.0);

        /// <summary>Beam-to-beam separation = roundUpTroquel(roundUpEven(alto + 6) + peralte), floored at one paso.</summary>
        private static double Separation(double alto, double peralte)
            => Math.Max(2.0, RoundUp(RoundUp(alto + 6.0, 2.0) + peralte, 2.0));

        /// <summary>Y of the first larguero when there is no floor beam: the pallet is on the floor, so the
        /// beam only clears (alto + holgura) — no peralte term — snapped up onto the grid.</summary>
        private static double FirstBeamNoFloor(double groundAlto)
            => SnapUp(RoundUp(groundAlto + 6.0, 2.0));

        /// <summary>The larguero's INICIO_PERFIL Y (escalón height where the pallet rests), from the catalog.</summary>
        private static double BeamStartY()
            => Catalog.ConnectionLayout.FindConnectionLayout(BeamId, "INICIO_PERFIL", "FRONTAL").LocalY;

        private static SelectiveCell Cell(double frente, double alto, int count, double beamPeralte)
            => new SelectiveCell
            {
                Pallet = new Tarima { Frente = frente, Alto = alto },
                PalletCount = count,
                BeamId = BeamId,
                BeamPeralte = beamPeralte
            };

        private static SelectivePalletDesign Design(bool floorBeam, params SelectiveCell[] levels)
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId,
                PostPeralte = 3.0,
                PalletTolerance = 4.0,
                VerticalClearance = 6.0
            };

            var bay = new SelectiveBayDesign { FloorBeam = floorBeam };
            foreach (var level in levels)
            {
                bay.Levels.Add(level);
            }

            design.Bays.Add(bay);
            return design;
        }

        private static SelectiveBay ResolveBay(SelectivePalletDesign design)
            => new SelectiveGeometryResolver().Resolve(design, Catalog).Bays[0];

        [Fact]
        public void BeamLength_IsFrenteTimesCountPlusTolerancePerGap()
        {
            var bay = ResolveBay(Design(false, Cell(40, 60, 2, 4)));

            // 2 tarimas de 40" + tolerancia 4" en 3 huecos (izq, entre, der) = 40*2 + 4*3 = 92.
            Assert.Equal(92.0, bay.BeamLength, 4);
        }

        [Fact]
        public void BeamLength_WidestLevelGovernsTheBay()
        {
            var bay = ResolveBay(Design(false, Cell(40, 60, 1, 4), Cell(48, 50, 2, 4)));

            // nivel 0 (piso): 40 + 4*2 = 48 ; nivel 1: 48*2 + 4*3 = 108 -> gana el más ancho.
            Assert.Equal(108.0, bay.BeamLength, 4);
        }

        [Fact]
        public void FloorBeamOff_GroundHasNoBeam_SoBeamsAreLevelsMinusOne()
        {
            var bay = ResolveBay(Design(false, Cell(40, 60, 1, 4), Cell(40, 50, 1, 4), Cell(40, 50, 1, 4)));

            // 3 niveles, sin larguero a piso -> 2 largueros (niveles 2 y 3).
            Assert.Equal(2, bay.Levels.Count);
        }

        [Fact]
        public void FloorBeamOff_SingleGroundLevel_HasNoBeamsAtAll()
        {
            var system = new SelectiveGeometryResolver().Resolve(Design(false, Cell(40, 60, 1, 4)), Catalog);

            Assert.Empty(system.Bays[0].Levels);
            // Post still covers a third of the ground pallet.
            Assert.Equal(RoundUpFoot(60.0 / 3.0), system.Height, 4);
        }

        [Fact]
        public void FloorBeamOff_FirstBeamSnapsOntoTheGridAboveTheGroundPallet()
        {
            var bay = ResolveBay(Design(false, Cell(40, 60, 1, 4), Cell(40, 50, 1, 4)));

            // No peralte term: the ground pallet is on the floor, so the beam just clears alto + holgura.
            Assert.Equal(FirstBeamNoFloor(60.0), bay.Levels[0].Y, 4);
        }

        [Fact]
        public void FloorBeamOn_GroundBeamRisesAboveTheLowestTroquel()
        {
            var design = Design(true, Cell(40, 60, 1, 4), Cell(40, 50, 1, 4));
            var bay = new SelectiveGeometryResolver().Resolve(design, Catalog).Bays[0];

            // 2 niveles con larguero a piso -> 2 largueros; el de piso sube FloorBeamRise para librar la placa.
            Assert.Equal(2, bay.Levels.Count);
            Assert.Equal(GridBase() + design.FloorBeamRise, bay.Levels[0].Y, 4);
            Assert.Equal(GridBase() + design.FloorBeamRise + Separation(60.0, 4.0), bay.Levels[1].Y, 4);
        }

        [Fact]
        public void FloorBeamRise_IsEditable()
        {
            var design = Design(true, Cell(40, 60, 1, 4), Cell(40, 50, 1, 4));
            design.FloorBeamRise = 8.0;

            var bay = new SelectiveGeometryResolver().Resolve(design, Catalog).Bays[0];

            Assert.Equal(GridBase() + 8.0, bay.Levels[0].Y, 4);
        }

        [Fact]
        public void Separation_IsClearOpeningPlusPeralteRoundedUpToTroquel()
        {
            var bay = ResolveBay(Design(true, Cell(40, 60, 1, 4), Cell(40, 60, 1, 4)));

            // claro libre = redondearPar(60 + 6) = 66 ; + peralte 4 = 70 -> separación 70.
            Assert.Equal(70.0, bay.Levels[1].Y - bay.Levels[0].Y, 4);
        }

        [Fact]
        public void Separation_RoundsBothClearOpeningAndPitchUpwardToEven()
        {
            var bay = ResolveBay(Design(true, Cell(40, 59, 1, 3), Cell(40, 59, 1, 3)));

            // claro libre = redondearPar(59 + 6 = 65) = 66 ; + peralte 3 = 69 -> redondearTroquel arriba = 70.
            Assert.Equal(70.0, bay.Levels[1].Y - bay.Levels[0].Y, 4);
        }

        [Fact]
        public void PostHeight_IsTopLoadSurfacePlusPalletThirdRoundedUpToFoot()
        {
            var system = new SelectiveGeometryResolver().Resolve(
                Design(false, Cell(40, 60, 1, 4), Cell(40, 60, 1, 4)), Catalog);

            // The pallet rests on the beam's escalón (BeamStartY above the troquel); the third is measured from there.
            var loadSurface = system.Bays[0].Levels.Last().Y + BeamStartY();
            Assert.Equal(RoundUpFoot(loadSurface + 60.0 / 3.0), system.Height, 4);
        }

        [Fact]
        public void PostHeight_CoversAtLeastAThirdOfTheTopPalletAboveItsLoadSurface()
        {
            var system = new SelectiveGeometryResolver().Resolve(
                Design(true, Cell(40, 60, 1, 4), Cell(40, 60, 1, 4), Cell(40, 60, 1, 4)), Catalog);

            var top = system.Bays[0].Levels.Last();
            var loadSurface = top.Y + BeamStartY();
            Assert.True(system.Height - loadSurface >= 60.0 / 3.0 - 1e-6);
        }

        [Fact]
        public void Height_TakesTheTallestBayForUniformPosts()
        {
            var design = new SelectivePalletDesign { PostId = PostId, PostPeralte = 3.0 };

            var shortBay = new SelectiveBayDesign { FloorBeam = true };
            shortBay.Levels.Add(Cell(40, 40, 1, 4));

            var tallBay = new SelectiveBayDesign { FloorBeam = true };
            tallBay.Levels.Add(Cell(40, 40, 1, 4));
            tallBay.Levels.Add(Cell(40, 40, 1, 4));
            tallBay.Levels.Add(Cell(40, 40, 1, 4));

            design.Bays.Add(shortBay);
            design.Bays.Add(tallBay);

            var system = new SelectiveGeometryResolver().Resolve(design, Catalog);

            var tallTop = system.Bays[1].Levels.Last().Y + BeamStartY() + 40.0 / 3.0;
            Assert.Equal(RoundUpFoot(tallTop), system.Height, 4);
        }

        [Fact]
        public void BayHeight_IsSetPerBaySoPostsCanDiffer()
        {
            var design = new SelectivePalletDesign { PostId = PostId, PostPeralte = 3.0 };

            var shortBay = new SelectiveBayDesign { FloorBeam = true };
            shortBay.Levels.Add(Cell(40, 40, 1, 4));

            var tallBay = new SelectiveBayDesign { FloorBeam = true };
            tallBay.Levels.Add(Cell(40, 40, 1, 4));
            tallBay.Levels.Add(Cell(40, 40, 1, 4));
            tallBay.Levels.Add(Cell(40, 40, 1, 4));

            design.Bays.Add(shortBay);
            design.Bays.Add(tallBay);

            var system = new SelectiveGeometryResolver().Resolve(design, Catalog);

            Assert.Equal(RoundUpFoot(system.Bays[0].Levels.Last().Y + BeamStartY() + 40.0 / 3.0), system.Bays[0].Height, 4);
            Assert.Equal(RoundUpFoot(system.Bays[1].Levels.Last().Y + BeamStartY() + 40.0 / 3.0), system.Bays[1].Height, 4);
            Assert.True(system.Bays[1].Height > system.Bays[0].Height);
        }
    }
}
