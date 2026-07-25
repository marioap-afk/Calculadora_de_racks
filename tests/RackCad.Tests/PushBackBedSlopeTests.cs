using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// PB-004 (I-32) — the Push Back bed rises 7/16" per commercial FOOT of rack, so a 204" rack rises exactly
    /// 7.4375" (= 7 7/16"). The Owner measured 11.2" on the drawing, produced by three compounding errors that no
    /// single function owned: the two end elevations were snapped to the 2" troquel INDEPENDENTLY, the axis then
    /// jumped between two different catalog datums (the low beam's TROQUEL_CAMA at 1.8783 and the rear beam's
    /// INICIO_DERECHO at 6.8125, +4.9342), and the drawn bed re-scaled that rise from the mate-to-mate run onto its
    /// full commercial length.
    ///
    /// The rule now lives in ONE pure function (<see cref="PushBackBedSlope"/>) that the axis, the drawing, the BOM
    /// and the UI all consume. The LOW end stays anchored on the exit larguero's troquel-snapped TROQUEL_CAMA (the
    /// Owner's "ajustada al troquel más cercano del larguero de salida"); the HIGH end is DERIVED from it, so the
    /// rear beam follows the bed instead of a second independent snap.
    ///
    /// These tests measure what the Owner measures: the vertical rise of the DRAWN bed over the rack's own length,
    /// not an identity of the axis with itself.
    /// </summary>
    public class PushBackBedSlopeTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        /// <summary>The Owner's reported scenario: 4 pallets of 48" deep + the header allowances = a 204" rack.</summary>
        private static DynamicRackDesign BaseStructure() => new DynamicRackDesign
        {
            Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
            PalletsDeep = 4,
            LoadLevels = 3,
            FirstLevelHeight = 6.0,
            BeamDepth = 4.0
        };

        private static PushBackSystem System(RackCatalog catalog)
            => new PushBackResolver(catalog).Resolve(new PushBackDesign { Structure = BaseStructure() });

        /// <summary>The rise the Owner's rule demands over <paramref name="run"/> inches of rack: 7/16" per foot.</summary>
        private static double ExpectedRise(double run) => run / 12.0 * (7.0 / 16.0);

        // ---- The reported defect, measured on the rack's own span ----

        [Fact]
        public void Bed_RisesSevenAndSevenSixteenths_OverTheOwners204InchRack()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];
            var span = front.EndX - front.StartX;
            Assert.Equal(204.0, span, 6);

            var axes = PushBackFlowBedGeometry.Resolve(system, catalog, front);
            Assert.NotEmpty(axes);
            foreach (var axis in axes)
            {
                // Rise over the RACK's span, not over the mate-to-mate run: this is the AutoCAD dimension.
                Assert.Equal(7.4375, axis.Rise / axis.Run * span, 6);
            }
        }

        /// <summary>
        /// The same measurement taken on the DRAWN bed of the lateral plan (the rail instance's own rotation and
        /// LONGITUD), so the pin cannot be satisfied by an axis that agrees with itself while the builder anchors or
        /// scales the assembly differently.
        /// </summary>
        [Fact]
        public void DrawnBed_RisesSevenAndSevenSixteenths_OverItsOwnCommercialLength()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];
            var bedLength = PushBackFlowBedGeometry.ResolveBedLength(system, front);
            Assert.Equal(204.0, bedLength, 6);

            var rails = new PushBackSystemLateralBuilder()
                .Build(system, catalog)
                .Flatten()
                .Instances
                .Where(instance => string.Equals(instance.PieceId, FlowBedDefaults.RailId, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Assert.NotEmpty(rails);

            // The rack's 7.4375" is measured over HORIZONTAL run (what an AutoCAD vertical dimension reads). The rail
            // is the same line laid along its own commercial LONGITUD, so it rises L*sin(a) instead of L*tan(a):
            // 7.4326" instead of 7.4375" over 204". The 0.0049" gap is recorded here on purpose, not smoothed over.
            var alongLength = bedLength * Math.Sin(Math.Atan(PushBackBedSlope.Ratio));
            Assert.Equal(7.4375, ExpectedRise(bedLength), 9);
            Assert.True(Math.Abs(ExpectedRise(bedLength) - alongLength) < 0.005);

            foreach (var rail in rails)
            {
                var longitud = rail.DynamicParameters[SelectiveRackDefaults.LengthParam];
                Assert.Equal(204.0, longitud, 6);

                var drawnRise = longitud * Math.Sin(rail.RotationRadians);
                Assert.Equal(alongLength, drawnRise, 6);
            }
        }

        // ---- The two rules the Owner fixed, as separate invariants ----

        [Fact]
        public void LowMate_StaysAnchoredOnTheExitLargueroSnappedTroquel()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];
            var camaLocal = catalog.ConnectionLayout.FindConnectionLayout(
                DynamicRackDefaults.InOutBeamCatalogId, DynamicRackDefaults.InOutBeamBedMatePoint, DynamicRackDefaults.InOutBeamView);
            Assert.NotNull(camaLocal);

            var levels = DynamicFrontGeometry.LoadBeamLevels(system.Structure, front);
            foreach (var axis in PushBackFlowBedGeometry.Resolve(system, catalog, front))
            {
                var level = levels.First(entry => entry.LevelNumber == axis.LevelNumber);
                Assert.Equal(level.ExitElevation + camaLocal.LocalY, axis.ExitMate.Y, 9);
            }
        }

        /// <summary>
        /// The negative pin of the rule that was REMOVED: the high end is no longer the rear beam's snapped elevation
        /// plus its own INICIO datum. Asserting the exact defective value (rather than a bare NotEqual) makes this
        /// fail if the old rule ever comes back.
        /// </summary>
        [Fact]
        public void HighMate_IsNoLongerTheRearBeamsOwnSnappedDatum()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];
            var inicio = catalog.ConnectionLayout.FindConnectionLayout(
                PushBackDefaults.HighEndBeamCatalogId, PushBackDefaults.HighEndBeamRightBedMatePoint, PushBackDefaults.HighEndBeamView);
            Assert.NotNull(inicio);

            var levels = DynamicFrontGeometry.LoadBeamLevels(system.Structure, front);
            foreach (var axis in PushBackFlowBedGeometry.Resolve(system, catalog, front))
            {
                var level = levels.First(entry => entry.LevelNumber == axis.LevelNumber);
                var oldRule = level.EntranceElevation + inicio.LocalY;
                Assert.Equal(axis.ExitMate.Y + PushBackBedSlope.Rise(axis.Run), axis.HighMate.Y, 9);
                Assert.True(
                    Math.Abs(oldRule - axis.HighMate.Y) > 3.0,
                    FormattableString.Invariant($"level {axis.LevelNumber}: the old datum rule is back ({oldRule:0.####} vs {axis.HighMate.Y:0.####})"));
            }
        }

        /// <summary>
        /// The same physical rear beam must be drawn at the SAME elevation in the lateral and in the rear frontal cut
        /// (D14 of the Owner's AutoCAD matrix). They diverged by 1.18" before this fix and would diverge by ~4.9"
        /// after it if the rear frontal kept the raw snapped elevation.
        /// </summary>
        [Fact]
        public void RearBeam_HasTheSameElevation_InTheLateralAndInTheRearFrontal()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];

            var lateralY = new PushBackSystemLateralBuilder()
                .Build(system, catalog)
                .Flatten()
                .Instances
                .Where(instance => string.Equals(instance.PieceId, PushBackDefaults.HighEndBeamCatalogId, StringComparison.OrdinalIgnoreCase))
                .Select(instance => Math.Round(instance.Insertion.Y, 4))
                .OrderBy(y => y)
                .ToList();
            var frontalY = new PushBackSystemFrontalBuilder()
                .BuildPlan(system, catalog, PushBackFrontalEnd.Posterior)
                .Flatten()
                .Instances
                .Where(instance => string.Equals(instance.PieceId, PushBackDefaults.HighEndBeamCatalogId, StringComparison.OrdinalIgnoreCase))
                .Select(instance => Math.Round(instance.Insertion.Y, 4))
                .OrderBy(y => y)
                .ToList();

            Assert.NotEmpty(lateralY);
            Assert.Equal(lateralY.Count, frontalY.Count);
            Assert.Equal(lateralY, frontalY);
        }

        // ---- Isolation: the DYNAMIC bed keeps its own rule ----

        /// <summary>
        /// The dynamic bed is a DIFFERENT type with the same mate on both ends, so its rise stays exactly the snapped
        /// elevation delta. This pins that Push Back's rule did not leak into it.
        /// </summary>
        [Fact]
        public void DynamicBed_KeepsItsOwnSnappedRise_Untouched()
        {
            var catalog = Catalog;
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 3,
                FirstLevelHeight = 6.0,
                BeamDepth = 4.0
            };
            var system = new DynamicRackSystemResolver(catalog).Resolve(design).System;
            var front = system.Fronts[0];
            var levels = DynamicFrontGeometry.LoadBeamLevels(system, front);

            var axes = DynamicFlowBedGeometry.Resolve(system, catalog, front);
            Assert.NotEmpty(axes);
            foreach (var axis in axes)
            {
                var level = levels.First(entry => entry.LevelNumber == axis.LevelNumber);
                Assert.Equal(level.EntranceElevation - level.ExitElevation, axis.Rise, 9);
            }
        }

        [Fact]
        public void Slope_IsSevenSixteenthsPerCommercialFoot()
        {
            Assert.Equal(7.0 / 16.0, PushBackBedSlope.RisePerFoot, 12);
            Assert.Equal(7.4375, PushBackBedSlope.Rise(204.0), 9);
            Assert.Equal(0.0, PushBackBedSlope.Rise(-10.0), 12);
        }
    }
}
