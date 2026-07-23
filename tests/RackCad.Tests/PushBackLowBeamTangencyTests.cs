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
    /// PB-VAL-05 — the LOW (entrance/exit) IN/OUT beam must be tangent to the line that starts at the bed's PHYSICAL
    /// origin (<see cref="PushBackFlowBedAxis.RailOrigin"/>), not to the TROQUEL line through its <c>TROQUEL_CAMA</c> mate.
    ///
    /// Measured on the shipped catalog (rail local mate <c>TROQUEL_IN</c> = (1.5, 1.25), so the two lines are genuinely
    /// distinct and PARALLEL): the bed-origin line runs <b>1.251889" below</b> the troquel line at the exit X, identically
    /// on every level. The correction lowers the LOW BEAM by exactly that constant.
    ///
    /// The bed is the geometric authority and never moves: <see cref="PushBackFlowBedGeometry.Resolve"/> is computed from
    /// the RAW dynamic placements and never sees the shift, so origin, axis, slope and full length are bit-identical
    /// before and after. Because the offset is a constant on parallel lines it cannot alter the slope, and because only
    /// the low beam moves the rear snap and the intermediates are untouched.
    /// </summary>
    public class PushBackLowBeamTangencyTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

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

        /// <summary>The beam's bed mate (TROQUEL_CAMA) in world coordinates, for a drawn low-beam instance.</summary>
        private static Point2D BedMateOf(HeaderBlockInstance beam, RackCatalog catalog)
        {
            var local = CatalogLookup.Local(
                catalog, beam.PieceId, DynamicRackDefaults.InOutBeamBedMatePoint, DynamicRackDefaults.InOutBeamView);
            var localX = beam.MirroredX ? -local.X : local.X;
            return new Point2D(beam.Insertion.X + localX, beam.Insertion.Y + local.Y);
        }

        // ---- The two lines really are distinct and parallel ----

        [Fact]
        public void TheBedOriginLine_AndTheTroquelLine_AreDistinctAndParallel()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];

            var axes = PushBackFlowBedGeometry.Resolve(system, catalog, front);
            Assert.NotEmpty(axes);

            foreach (var axis in axes)
            {
                var offset = axis.RailOriginYAt(axis.ExitMate.X) - axis.ExitMate.Y;
                Assert.True(Math.Abs(offset) > 1e-6, "the bed-origin line must differ from the troquel line");

                // Parallel: the gap between them is the SAME at both ends, so no slope is introduced.
                var offsetAtHigh = axis.RailOriginYAt(axis.HighMate.X) - (axis.ExitMate.Y + (axis.HighMate.X - axis.ExitMate.X) * axis.Rise / axis.Run);
                Assert.Equal(offset, offsetAtHigh, 9);
            }
        }

        [Fact]
        public void BedOriginOffset_IsTheSameConstantOnEveryLevel()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];
            var axes = PushBackFlowBedGeometry.Resolve(system, catalog, front);

            var offsets = axes.Select(a => PushBackLoadBeamGeometry.BedOriginOffset(axes, a.LevelNumber)).ToList();
            Assert.NotEmpty(offsets);
            Assert.All(offsets, o => Assert.Equal(offsets[0], o, 9));
            Assert.True(offsets[0] < 0.0, "the bed-origin line sits BELOW the troquel line, so the beam moves down");
        }

        // ---- The contract: the beam lands on the bed-origin line ----

        [Fact]
        public void LowBeamBedMate_LiesOnTheBedOriginLine()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];

            var axes = PushBackFlowBedGeometry.Resolve(system, catalog, front);
            var beams = PushBackLoadBeamGeometry.LowBeams(system, catalog, front);
            Assert.NotEmpty(beams);

            foreach (var axis in axes)
            {
                var beam = beams.Single(b => Math.Abs(b.Insertion.Y - (
                    DynamicLoadBeamGeometry.Placements(system.Structure, front)
                        .Single(p => !p.IsEntrance && p.LevelNumber == axis.LevelNumber).Y
                    + PushBackLoadBeamGeometry.BedOriginOffset(axes, axis.LevelNumber))) < 1e-9);

                var mate = BedMateOf(beam, catalog);
                Assert.Equal(axis.RailOriginYAt(mate.X), mate.Y, 6);   // ON the bed-origin line
            }
        }

        [Fact]
        public void LowBeamBedMate_NoLongerLiesOnTheTroquelLine()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];

            var axes = PushBackFlowBedGeometry.Resolve(system, catalog, front);
            var beams = PushBackLoadBeamGeometry.LowBeams(system, catalog, front);
            Assert.NotEmpty(beams);

            // Every level shares the same exit X, so compare per LEVEL: the troquel line through that level's ExitMate,
            // evaluated at the mate X, is exactly ExitMate.Y — the drawn mate must NOT land there any more.
            foreach (var axis in axes)
            {
                var mateY = axis.RailOriginYAt(axis.ExitMate.X);
                Assert.True(Math.Abs(mateY - axis.ExitMate.Y) > 1e-6,
                    "the beam must no longer be tangent to the troquel line while the two lines differ");
                Assert.Contains(beams, b => Math.Abs(BedMateOf(b, catalog).Y - mateY) < 1e-6);
            }
        }

        // ---- The bed is untouched: origin, slope, length, rear end, intermediates ----

        [Fact]
        public void BedOriginAxisSlopeAndLength_AreIdenticalBeforeAndAfterTheCorrection()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];

            // Resolve is computed from the RAW placements; drawing the (shifted) low beams must not perturb it.
            var before = PushBackFlowBedGeometry.Resolve(system, catalog, front);
            var _ = PushBackLoadBeamGeometry.LowBeams(system, catalog, front);
            var after = PushBackFlowBedGeometry.Resolve(system, catalog, front);

            Assert.Equal(before.Count, after.Count);
            for (var i = 0; i < before.Count; i++)
            {
                Assert.Equal(before[i].RailOrigin.X, after[i].RailOrigin.X, 9);
                Assert.Equal(before[i].RailOrigin.Y, after[i].RailOrigin.Y, 9);
                Assert.Equal(before[i].ExitMate.Y, after[i].ExitMate.Y, 9);
                Assert.Equal(before[i].HighMate.Y, after[i].HighMate.Y, 9);   // rear end untouched
                Assert.Equal(before[i].Length, after[i].Length, 9);
                Assert.Equal(before[i].AngleRadians, after[i].AngleRadians, 9);
            }
        }

        [Fact]
        public void BedLength_StaysTheFullPhysicalSpan_WithNoFourInchDeduction()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];

            var length = PushBackFlowBedGeometry.ResolveBedLength(system, front);
            Assert.Equal(front.EndX - front.StartX, length, 6);   // full span, no −4"
        }

        [Fact]
        public void RearBeams_KeepTheirTroquelSnappedElevation()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];

            var raw = DynamicLoadBeamGeometry.Placements(system.Structure, front).Where(p => p.IsEntrance).ToList();
            var high = PushBackLoadBeamGeometry.HighBeams(system, catalog, 0, front);

            Assert.Equal(raw.Count, high.Count);
            foreach (var beam in high)
            {
                Assert.Contains(raw, p => Math.Abs(p.Y - beam.Insertion.Y) < 1e-9);   // no shift at the rear
            }
        }

        [Fact]
        public void OnlyTheLowBeamMoved_AndByExactlyTheBedOriginOffset()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];

            var axes = PushBackFlowBedGeometry.Resolve(system, catalog, front);
            var raw = DynamicLoadBeamGeometry.Placements(system.Structure, front).Where(p => !p.IsEntrance).ToList();
            var low = PushBackLoadBeamGeometry.LowBeams(system, catalog, front);

            Assert.Equal(raw.Count, low.Count);
            foreach (var placement in raw)
            {
                var expected = placement.Y + PushBackLoadBeamGeometry.BedOriginOffset(axes, placement.LevelNumber);
                Assert.Contains(low, b => Math.Abs(b.Insertion.X - placement.X) < 1e-9
                                          && Math.Abs(b.Insertion.Y - expected) < 1e-9);
            }
        }

        [Fact]
        public void LateralPlan_StillDrawsTheBedAndBothEndBeams()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var plan = new PushBackSystemLateralBuilder().Build(system, catalog).Flatten().Instances;

            Assert.Contains(plan, i => i.PieceId == PushBackDefaults.HighEndBeamCatalogId);   // rear beam present
            Assert.Contains(plan, i => i.PieceId == DynamicRackDefaults.InOutBeamCatalogId);  // low beam present
            Assert.Contains(plan, i => i.PieceId == FlowBedDefaults.RailId);                  // bed present
        }
    }
}
