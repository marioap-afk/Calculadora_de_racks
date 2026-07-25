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
    /// Owner decisions (2026-07-24) for the Push Back bed, which SUPERSEDE the earlier low-beam displacement:
    /// <list type="number">
    /// <item>The LOW IN/OUT beam is placed so its <c>TROQUEL_CAMA</c> coincides EXACTLY with the rail's <c>TROQUEL_IN</c>.
    /// It carries no shift of its own — the bed is resolved from that very mate.</item>
    /// <item>The REAR beam is the one that moves: its correct transformed contact edge
    /// (<c>INICIO_IZQUIERDO</c>/<c>INICIO_DERECHO</c>) is TANGENT to the line through the bed's physical origin, while
    /// the bed keeps its slope, full length, axis and origin.</item>
    /// </list>
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

        private static string HighBeamId(PushBackSystem system)
            => string.IsNullOrWhiteSpace(system.HighEndBeamCatalogId)
                ? PushBackDefaults.HighEndBeamCatalogId
                : system.HighEndBeamCatalogId;

        // ---- (1) the low beam is bolted: TROQUEL_CAMA == TROQUEL_IN, in WORLD coordinates ----

        [Fact]
        public void LowBeam_TroquelCama_CoincidesExactlyWithTheRailsTroquelIn()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];

            var axes = PushBackFlowBedGeometry.Resolve(system, catalog, front);
            var beams = PushBackLoadBeamGeometry.LowBeams(system, catalog, front);
            Assert.NotEmpty(axes);
            Assert.NotEmpty(beams);

            var railMate = catalog.ConnectionLayout.FindConnectionLayout(
                FlowBedDefaults.RailId, FlowBedDefaults.RailInOutMatePoint, FlowBedDefaults.View);
            Assert.NotNull(railMate);

            foreach (var axis in axes)
            {
                // Where the rail's TROQUEL_IN actually lands once the rail block is placed at its origin and rotated.
                var cos = Math.Cos(axis.AngleRadians);
                var sin = Math.Sin(axis.AngleRadians);
                var railTroquel = new Point2D(
                    axis.RailOrigin.X + railMate.LocalX * cos - railMate.LocalY * sin,
                    axis.RailOrigin.Y + railMate.LocalX * sin + railMate.LocalY * cos);

                // Where the DRAWN low beam's TROQUEL_CAMA lands.
                var placement = DynamicLoadBeamGeometry.Placements(system.Structure, front)
                    .Single(p => !p.IsEntrance && p.LevelNumber == axis.LevelNumber);
                var local = PushBackLoadBeamGeometry.BedTangencyPointLocal(catalog, placement.BeamCatalogId);
                Assert.True(local.HasValue);
                var drawn = beams.Single(b => Math.Abs(b.Insertion.X - placement.X) < 1e-9
                    && Math.Abs(b.Insertion.Y - placement.Y) < 1e-9);
                var beamTroquel = PushBackLoadBeamGeometry.BedTangencyPointWorld(
                    local.Value, drawn.Insertion.X, drawn.Insertion.Y, drawn.MirroredX);

                Assert.Equal(railTroquel.X, beamTroquel.X, 9);
                Assert.Equal(railTroquel.Y, beamTroquel.Y, 9);
            }
        }

        [Fact]
        public void LowBeam_CarriesNoDisplacement_ItSitsOnTheResolvedExitElevation()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];

            var placements = DynamicLoadBeamGeometry.Placements(system.Structure, front)
                .Where(p => !p.IsEntrance).ToList();
            foreach (var beam in PushBackLoadBeamGeometry.LowBeams(system, catalog, front))
            {
                Assert.Contains(placements, p =>
                    Math.Abs(p.X - beam.Insertion.X) < 1e-12 && Math.Abs(p.Y - beam.Insertion.Y) < 1e-12);
            }
        }

        // ---- (2) the rear beam is tangent to the bed-origin line, at the correct transformed edge ----

        [Fact]
        public void RearBeam_ContactEdge_IsTangentToTheBedOriginLine()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];
            var beamId = HighBeamId(system);

            var axes = PushBackFlowBedGeometry.Resolve(system, catalog, front);
            var high = PushBackLoadBeamGeometry.HighBeams(system, catalog, 0, front);
            Assert.NotEmpty(axes);
            Assert.NotEmpty(high);

            var placements = DynamicLoadBeamGeometry.Placements(system.Structure, front)
                .Where(p => p.IsEntrance).ToList();

            foreach (var axis in axes)
            {
                var placement = placements.Single(p => p.LevelNumber == axis.LevelNumber);
                var offset = PushBackLoadBeamGeometry.RearBeamTangencyOffset(
                    axes, axis.LevelNumber, catalog, beamId, placement.X, placement.Y, placement.MirroredX);
                var drawn = high.Single(b => Math.Abs(b.Insertion.Y - (placement.Y + offset)) < 1e-9
                    && Math.Abs(b.Insertion.X - placement.X) < 1e-9);

                var contact = PushBackLoadBeamGeometry.RearBeamTangencyPointWorld(
                    catalog, beamId, drawn.Insertion.X, drawn.Insertion.Y, drawn.MirroredX);
                Assert.True(contact.HasValue);

                // THE contract: the transformed contact edge sits exactly on the bed-ORIGIN line at its own X.
                Assert.Equal(axis.RailOriginYAt(contact.Value.X), contact.Value.Y, 9);

                // And it really moved off the raw troquel-snapped elevation.
                Assert.NotEqual(placement.Y, drawn.Insertion.Y);
                Assert.Equal(placement.X, drawn.Insertion.X, 9);
            }
        }

        [Fact]
        public void RearBeam_ContactEdge_IsThePointTheBedLandsOn_ChosenByGeometryNotByAFixedSide()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var beamId = HighBeamId(system);

            var left = catalog.ConnectionLayout.FindConnectionLayout(
                beamId, PushBackDefaults.HighEndBeamLeftBedMatePoint, PushBackDefaults.HighEndBeamView);
            var right = catalog.ConnectionLayout.FindConnectionLayout(
                beamId, PushBackDefaults.HighEndBeamRightBedMatePoint, PushBackDefaults.HighEndBeamView);
            Assert.NotNull(left);
            Assert.NotNull(right);

            // The bed line rises toward +X, so contact happens at the edge with the GREATER world X.
            var straight = PushBackLoadBeamGeometry.RearBeamTangencyPointWorld(catalog, beamId, 100.0, 50.0, mirroredX: false);
            Assert.True(straight.HasValue);
            Assert.Equal(100.0 + Math.Max(left.LocalX, right.LocalX), straight.Value.X, 9);

            // Mirrored, the local X flips, so the correct edge becomes the OTHER one — chosen by geometry.
            var mirrored = PushBackLoadBeamGeometry.RearBeamTangencyPointWorld(catalog, beamId, 100.0, 50.0, mirroredX: true);
            Assert.True(mirrored.HasValue);
            Assert.Equal(100.0 - Math.Min(left.LocalX, right.LocalX), mirrored.Value.X, 9);
        }

        [Fact]
        public void NoFallbackToTheRawOrigin_WhenTheBlockHasNoMeasuredContactFace()
        {
            var catalog = Catalog;
            // No measured edges => no anchor at all, never the insertion point.
            Assert.Null(PushBackLoadBeamGeometry.RearBeamTangencyPointWorld(catalog, "PIEZA_SIN_PUNTOS", 10.0, 20.0, false));
            Assert.Null(PushBackLoadBeamGeometry.RearBeamTangencyPointWorld(null, "CUALQUIERA", 10.0, 20.0, false));
            Assert.Null(PushBackLoadBeamGeometry.BedTangencyPointLocal(catalog, "PIEZA_SIN_PUNTO_DE_CAMA"));
            Assert.Null(PushBackLoadBeamGeometry.BedTangencyPointLocal(null, DynamicRackDefaults.InOutBeamCatalogId));

            // And with no resolved axis there is no line to be tangent to: no shift, rather than an unrelated one.
            Assert.Equal(0.0, PushBackLoadBeamGeometry.RearBeamTangencyOffset(
                null, 1, catalog, PushBackDefaults.HighEndBeamCatalogId, 0.0, 0.0, false), 12);
        }

        // ---- the bed itself is authority and must come out bit-identical ----

        [Fact]
        public void TheBed_StaysBitIdentical_OriginSlopeAxisAndLength()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];

            var before = PushBackFlowBedGeometry.Resolve(system, catalog, front);
            var low = PushBackLoadBeamGeometry.LowBeams(system, catalog, front);
            var high = PushBackLoadBeamGeometry.HighBeams(system, catalog, 0, front);
            var after = PushBackFlowBedGeometry.Resolve(system, catalog, front);
            Assert.NotEmpty(low);
            Assert.NotEmpty(high);
            Assert.Equal(before.Count, after.Count);

            for (var i = 0; i < before.Count; i++)
            {
                Assert.Equal(before[i].ExitMate.X, after[i].ExitMate.X, 12);
                Assert.Equal(before[i].ExitMate.Y, after[i].ExitMate.Y, 12);
                Assert.Equal(before[i].HighMate.X, after[i].HighMate.X, 12);
                Assert.Equal(before[i].HighMate.Y, after[i].HighMate.Y, 12);
                Assert.Equal(before[i].RailOrigin.X, after[i].RailOrigin.X, 12);
                Assert.Equal(before[i].RailOrigin.Y, after[i].RailOrigin.Y, 12);
                Assert.Equal(before[i].AngleRadians, after[i].AngleRadians, 12);
                Assert.Equal(before[i].Length, after[i].Length, 12);
            }

            // The commercial bed length is the FULL span, with no 4" deduction.
            var front0 = system.Structure.Fronts[0];
            Assert.Equal(front0.EndX - front0.StartX, PushBackFlowBedGeometry.ResolveBedLength(system, front0), 9);
        }

        [Fact]
        public void TheBedSlope_IsUnchanged_AndTheShiftIsVerticalOnly()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];

            var axes = PushBackFlowBedGeometry.Resolve(system, catalog, front);
            var slopes = axes.Select(a => Math.Round(a.Rise / a.Run, 9)).Distinct().ToList();
            Assert.Single(slopes);   // one lane slope for every level

            // Every drawn beam keeps its placement X: the correction never moves anything horizontally.
            var placements = DynamicLoadBeamGeometry.Placements(system.Structure, front).ToList();
            foreach (var beam in PushBackLoadBeamGeometry.LowBeams(system, catalog, front)
                .Concat(PushBackLoadBeamGeometry.HighBeams(system, catalog, 0, front)))
            {
                Assert.Contains(placements, p => Math.Abs(p.X - beam.Insertion.X) < 1e-12);
            }
        }
    }
}
