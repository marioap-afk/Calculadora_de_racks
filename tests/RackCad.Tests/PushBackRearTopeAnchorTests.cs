using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// PB-VAL-02 (Owner retest) — the rear tope is PLACED and ORIENTED from the rear beam's REAL transformed connection
    /// points and from the system's load side, never from the raw <c>placement.X</c> nor from the beam's own mirror.
    /// Numeric assertions on the WORLD anchor, the orientation and the height across lateral, rear frontal and planta;
    /// the elevation (+4"), SAQUE, LONGITUD, snap and OffCells contracts are pinned unchanged.
    /// </summary>
    public class PushBackRearTopeAnchorTests
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

        // ---- the anchor comes from the beam's measured contact face, on the load side ----

        [Fact]
        public void LateralAnchor_IsTheLoadSideContactPoint_NotTheRawPlacementX()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var beamId = HighBeamId(system);

            var left = catalog.ConnectionLayout.FindConnectionLayout(
                beamId, PushBackDefaults.HighEndBeamLeftBedMatePoint, PushBackDefaults.HighEndBeamView);
            var right = catalog.ConnectionLayout.FindConnectionLayout(
                beamId, PushBackDefaults.HighEndBeamRightBedMatePoint, PushBackDefaults.HighEndBeamView);
            Assert.NotNull(left);   // the shipped catalog measures both edges of the contact face
            Assert.NotNull(right);

            const double placementX = 100.0;

            // Unmirrored: both points transform to +local, so the LOAD side (lower world X) is the left edge.
            var unmirrored = PushBackRearTopeBuilder.LateralAnchorX(catalog, beamId, placementX, beamMirroredX: false);
            Assert.Equal(placementX + Math.Min(left.LocalX, right.LocalX), unmirrored, 9);

            // Mirrored: the local X flips, so the load side becomes the OTHER edge — the rule follows the mirror.
            var mirrored = PushBackRearTopeBuilder.LateralAnchorX(catalog, beamId, placementX, beamMirroredX: true);
            Assert.Equal(placementX - Math.Max(left.LocalX, right.LocalX), mirrored, 9);

            // In both cases it is a REAL measured point of the block, never the bare insertion point.
            Assert.NotEqual(placementX, unmirrored);
            Assert.NotEqual(placementX, mirrored);
        }

        [Fact]
        public void LateralAnchor_FallsBackToThePlacement_OnlyWhenTheBlockHasNoMeasuredContactFace()
        {
            var catalog = Catalog;
            // A piece with no INICIO_IZQUIERDO/INICIO_DERECHO rows: no silent preference for one edge, no magic offset.
            Assert.Equal(50.0, PushBackRearTopeBuilder.LateralAnchorX(catalog, "PIEZA_SIN_PUNTOS", 50.0, beamMirroredX: false), 9);
            Assert.Equal(50.0, PushBackRearTopeBuilder.LateralAnchorX(null, "CUALQUIERA", 50.0, beamMirroredX: true), 9);
        }

        [Fact]
        public void LateralTopes_SitOnTheContactPoint_AndFaceTheLoadSide()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];
            var beamId = HighBeamId(system);

            var topes = new PushBackRearTopeBuilder().BuildLateral(system, catalog, 0, front);
            Assert.NotEmpty(topes);

            var rearBeams = DynamicLoadBeamGeometry.Placements(system.Structure, front).Where(p => p.IsEntrance).ToList();
            Assert.NotEmpty(rearBeams);

            foreach (var tope in topes)
            {
                // Every tope's world X is the load-side contact point of ITS rear beam, and is DISPLACED from the raw
                // placement X the previous build anchored on.
                var expected = rearBeams
                    .Select(b => PushBackRearTopeBuilder.LateralAnchorX(catalog, beamId, b.X, b.MirroredX))
                    .ToList();
                Assert.Contains(expected, x => Math.Abs(x - tope.Insertion.X) < 1e-9);
                Assert.DoesNotContain(rearBeams, b => Math.Abs(b.X - tope.Insertion.X) < 1e-9);

                Assert.True(tope.MirroredX, "the lateral tope must face the load side");
                Assert.Equal(tope.Insertion, tope.ConnectionAnchor);
            }
        }

        // ---- the three views: anchor, orientation and height ----

        [Fact]
        public void RearFrontalAndPlanta_KeepTheBeamTransverseDatum_AndTheirOwnOrientation()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];
            var builder = new PushBackRearTopeBuilder();
            var rearBeams = DynamicLoadBeamGeometry.Placements(system.Structure, front).Where(p => p.IsEntrance).ToList();

            // FRONTAL and PLANTA look ACROSS the beam: the tope runs ALONG it, so it shares the beam's transverse datum
            // (the same datum its LONGITUD is measured from) — only the LATERAL is seen along the depth.
            foreach (var view in new[] { "FRONTAL", "PLANTA" })
            {
                var topes = builder.Build(system, catalog, 0, front, view);
                Assert.NotEmpty(topes);
                Assert.All(topes, tope => Assert.Contains(rearBeams, b => Math.Abs(b.X - tope.Insertion.X) < 1e-9));
            }

            // Rear frontal is an elevation: it faces the load side like the lateral.
            Assert.All(builder.Build(system, catalog, 0, front, "FRONTAL"), tope => Assert.True(tope.MirroredX));

            // Planta is a top view: the tope lies along the beam and keeps the beam's plan orientation.
            var planta = builder.Build(system, catalog, 0, front, "PLANTA");
            Assert.All(planta, tope => Assert.Contains(
                rearBeams, b => Math.Abs(b.X - tope.Insertion.X) < 1e-9 && b.MirroredX == tope.MirroredX));
        }

        [Fact]
        public void Heights_AreUnchanged_ElevationsRiseAndSnapPlusFour_PlantaKeepsTheFrenteY()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];
            var builder = new PushBackRearTopeBuilder();
            var rearBeams = DynamicLoadBeamGeometry.Placements(system.Structure, front).Where(p => p.IsEntrance).ToList();

            // PB-VAL-03 stays APPROVED: the elevation Y is still the canonical rise-and-snap plus exactly 4".
            Assert.Equal(4.0, PushBackRearTopeBuilder.ExtraRise, 9);
            foreach (var tope in builder.Build(system, catalog, 0, front, "LATERAL"))
            {
                Assert.Contains(rearBeams, b =>
                    Math.Abs(PushBackRearTopeBuilder.ElevationY(
                        PostGridBase(system, catalog), b.Y) - tope.Insertion.Y) < 1e-9);
            }

            // Planta keeps the frente Y (no rise-and-snap).
            Assert.All(builder.Build(system, catalog, 0, front, "PLANTA"),
                tope => Assert.Contains(rearBeams, b => Math.Abs(b.Y - tope.Insertion.Y) < 1e-9));
        }

        [Fact]
        public void SaqueLongitudAndOffCells_AreUntouchedByTheAnchorFix()
        {
            var catalog = Catalog;
            var design = new PushBackDesign { Structure = BaseStructure() };
            design.RearTope.OffCells.Add(new SelectiveGridCell { Frente = 0, Level = 1 });
            var system = new PushBackResolver(catalog).Resolve(design);
            var front = system.Structure.Fronts[0];

            var topes = new PushBackRearTopeBuilder().BuildLateral(system, catalog, 0, front);

            Assert.Equal(Math.Max(1, front.LoadLevels) - 1, topes.Count);   // the deactivated cell still has no tope
            Assert.All(topes, tope =>
            {
                Assert.Equal(PushBackDefaults.RearTopeSaque,
                    tope.DynamicParameters[SelectiveSafetyPlacement.SaqueParam], 9);
                Assert.Equal(HeaderBlockRole.Tope, tope.Role);
                Assert.Equal(PushBackRearTopeBuilder.TopePieceId, tope.PieceId);
            });

            // LONGITUD is the commercial rule and is view-independent here: the cell's transverse beam length plus the
            // canonical allowance, in the lateral and in the rear frontal alike. The anchor fix must not touch it.
            var expectedLongitud = PushBackLoadBeamGeometry.CellBeamLength(system.Structure, front, 1)
                + SelectiveTopePlacement.LengthAllowance;
            Assert.All(topes, tope => Assert.Equal(
                expectedLongitud, tope.DynamicParameters[SelectiveRackDefaults.LengthParam], 6));
            var frontal = new PushBackRearTopeBuilder().Build(system, catalog, 0, front, "FRONTAL");
            Assert.All(frontal, tope => Assert.Equal(
                expectedLongitud, tope.DynamicParameters[SelectiveRackDefaults.LengthParam], 6));
        }

        private static double PostGridBase(PushBackSystem system, RackCatalog catalog)
        {
            // Mirrors the builder's own grid base so the height assertion is independent of its private helper.
            var postId = DynamicFrontGeometry.PostId(system.Structure, catalog);
            var postPeralte = DynamicFrontGeometry.PostPeralte(system.Structure, catalog, postId);
            var entry = catalog.ConnectionLayout.FindConnectionLayout(
                postId, SelectiveRackDefaults.PostBeamPoint, SelectiveRackDefaults.View);
            return SelectivePostGeometry.Resolve(
                entry,
                new System.Collections.Generic.Dictionary<string, double> { [SelectiveRackDefaults.PeralteParam] = postPeralte }).Y;
        }
    }
}
