using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// PB-VAL-05 — the low IN/OUT beam is placed by an EXPLICIT canonical tangency point (the catalog's measured
    /// TROQUEL_CAMA, which the Owner confirmed on the DWG IS the beam's physical contact face with the bed), landed on
    /// the line through the bed's physical origin AT THAT POINT'S OWN X. Numeric assertions on the transformed point,
    /// plus proof that the bed itself — origin, slope, axis and length — is left bit-identical.
    /// </summary>
    public class PushBackBedTangencyTests
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

        // ---- the tangency point is a named, measured point of the real block ----

        [Fact]
        public void TangencyPoint_IsTheCatalogsMeasuredTroquelCama_NeverTheInsertionPoint()
        {
            var catalog = Catalog;
            var local = PushBackLoadBeamGeometry.BedTangencyPointLocal(catalog, DynamicRackDefaults.InOutBeamCatalogId);
            Assert.True(local.HasValue, "the IN/OUT beam must carry a measured bed-contact point");

            var entry = catalog.ConnectionLayout.FindConnectionLayout(
                DynamicRackDefaults.InOutBeamCatalogId,
                DynamicRackDefaults.InOutBeamBedMatePoint,
                DynamicRackDefaults.InOutBeamView);
            Assert.Equal(entry.LocalX, local.Value.X, 9);
            Assert.Equal(entry.LocalY, local.Value.Y, 9);

            // It is a real offset from the block origin: the insertion point is NOT the contact face.
            Assert.True(Math.Abs(local.Value.X) > 1e-9 || Math.Abs(local.Value.Y) > 1e-9);

            // A missing mate is a missing physical contract — never silently the block origin.
            Assert.Null(PushBackLoadBeamGeometry.BedTangencyPointLocal(catalog, "PIEZA_SIN_PUNTO_DE_CAMA"));
            Assert.Null(PushBackLoadBeamGeometry.BedTangencyPointLocal(null, DynamicRackDefaults.InOutBeamCatalogId));
        }

        [Fact]
        public void TangencyPoint_TransformsWithThePlacementMirror()
        {
            var local = new Point2D(3.0, 2.0);
            var straight = PushBackLoadBeamGeometry.BedTangencyPointWorld(local, 100.0, 50.0, mirroredX: false);
            Assert.Equal(103.0, straight.X, 9);
            Assert.Equal(52.0, straight.Y, 9);

            var mirrored = PushBackLoadBeamGeometry.BedTangencyPointWorld(local, 100.0, 50.0, mirroredX: true);
            Assert.Equal(97.0, mirrored.X, 9);
            Assert.Equal(52.0, mirrored.Y, 9);
        }

        // ---- the placed beam's tangency point lands ON the bed-origin line, at its own X ----

        [Fact]
        public void PlacedLowBeam_PutsItsTangencyPoint_OnTheBedOriginLine_AtThatPointsOwnX()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];

            var axes = PushBackFlowBedGeometry.Resolve(system, catalog, front);
            Assert.NotEmpty(axes);
            var beams = PushBackLoadBeamGeometry.LowBeams(system, catalog, front);
            Assert.NotEmpty(beams);

            var placements = DynamicLoadBeamGeometry.Placements(system.Structure, front)
                .Where(p => !p.IsEntrance).ToList();

            foreach (var axis in axes)
            {
                var placement = placements.FirstOrDefault(p => p.LevelNumber == axis.LevelNumber);
                Assert.NotNull(placement);

                var local = PushBackLoadBeamGeometry.BedTangencyPointLocal(catalog, placement.BeamCatalogId);
                Assert.True(local.HasValue);

                // The beam AS DRAWN (its instance carries the shift).
                var drawn = beams.Single(b => Math.Abs(b.Insertion.X - placement.X) < 1e-9
                    && Math.Abs(b.Insertion.Y - (placement.Y + PushBackLoadBeamGeometry.BedOriginOffset(axes, axis.LevelNumber))) < 1e-9);

                var tangency = PushBackLoadBeamGeometry.BedTangencyPointWorld(
                    local.Value, drawn.Insertion.X, drawn.Insertion.Y, drawn.MirroredX);

                // THE contract: the transformed tangency point sits exactly on the bed-ORIGIN line, evaluated at that
                // point's own X — not on the parallel TROQUEL line it used to sit on.
                Assert.Equal(axis.RailOriginYAt(tangency.X), tangency.Y, 9);
                Assert.NotEqual(axis.ExitMate.Y, tangency.Y);

                // The shift is purely vertical and equal to the constant separation between the two parallel lines.
                Assert.Equal(placement.X, drawn.Insertion.X, 9);
                Assert.Equal(axis.RailOriginYAt(axis.ExitMate.X) - axis.ExitMate.Y, drawn.Insertion.Y - placement.Y, 9);
            }
        }

        // ---- the bed is authority: it must come out bit-identical ----

        [Fact]
        public void TheBed_StaysBitIdentical_OriginSlopeAxisAndLength()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];

            // Resolved BEFORE and AFTER building the shifted beams: the resolver reads the RAW placements, so nothing the
            // low beam does can move the bed.
            var before = PushBackFlowBedGeometry.Resolve(system, catalog, front);
            var beams = PushBackLoadBeamGeometry.LowBeams(system, catalog, front);
            var after = PushBackFlowBedGeometry.Resolve(system, catalog, front);
            Assert.NotEmpty(beams);
            Assert.Equal(before.Count, after.Count);

            for (var i = 0; i < before.Count; i++)
            {
                Assert.Equal(before[i].LevelNumber, after[i].LevelNumber);
                Assert.Equal(before[i].ExitMate.X, after[i].ExitMate.X, 12);
                Assert.Equal(before[i].ExitMate.Y, after[i].ExitMate.Y, 12);
                Assert.Equal(before[i].HighMate.X, after[i].HighMate.X, 12);
                Assert.Equal(before[i].HighMate.Y, after[i].HighMate.Y, 12);
                Assert.Equal(before[i].RailOrigin.X, after[i].RailOrigin.X, 12);
                Assert.Equal(before[i].RailOrigin.Y, after[i].RailOrigin.Y, 12);
                Assert.Equal(before[i].AngleRadians, after[i].AngleRadians, 12);
                Assert.Equal(before[i].Length, after[i].Length, 12);
            }

            // And the commercial bed length is untouched.
            Assert.Equal(
                PushBackFlowBedLateralBuilder.ResolveBedLength(system, front),
                PushBackFlowBedGeometry.ResolveBedLength(system, front), 12);
        }

        [Fact]
        public void TheShift_IsVerticalOnly_AndNeverTouchesTheRearEnd()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];

            var placements = DynamicLoadBeamGeometry.Placements(system.Structure, front).ToList();
            var high = PushBackLoadBeamGeometry.HighBeams(system, catalog, 0, front);

            // The rear (high) beams keep their troquel-snapped elevation exactly: the troquel line stays authority there.
            foreach (var beam in high)
            {
                Assert.Contains(placements.Where(p => p.IsEntrance),
                    p => Math.Abs(p.X - beam.Insertion.X) < 1e-12 && Math.Abs(p.Y - beam.Insertion.Y) < 1e-12);
            }

            // And the low beams move ONLY in Y.
            var low = PushBackLoadBeamGeometry.LowBeams(system, catalog, front);
            foreach (var beam in low)
            {
                Assert.Contains(placements.Where(p => !p.IsEntrance), p => Math.Abs(p.X - beam.Insertion.X) < 1e-12);
            }
        }
    }
}
