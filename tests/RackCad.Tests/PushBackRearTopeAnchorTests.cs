using System;
using System.Collections.Generic;
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
    /// Owner decisions (2026-07-24) for the rear tope, which SUPERSEDE the earlier rules:
    /// <list type="number">
    /// <item>Its ORIGIN sits on the vertical axis of the POST's <c>TROQUEL_SEPARADOR</c> — never the rear beam's contact
    /// points, never a raw <c>placement.X</c> fallback. PLANTA uses the separator point measured in the PLANTA view.</item>
    /// <item>Its ORIENTATION is INVERTED with respect to 10d8eeb, where the Owner measured it upside down.</item>
    /// </list>
    /// The approved elevation (+4"), snap, SAQUE, LONGITUD, OffCells and BOM are pinned unchanged.
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

        private static (string PostId, double Peralte) Post(PushBackSystem system, RackCatalog catalog)
        {
            var postId = DynamicFrontGeometry.PostId(system.Structure, catalog);
            return (postId, DynamicFrontGeometry.PostPeralte(system.Structure, catalog, postId));
        }

        // ---- (1) the anchor is the POST's TROQUEL_SEPARADOR axis ----

        [Fact]
        public void SeparatorAnchor_IsTheCatalogsMeasuredPostPoint_PerView()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var (postId, peralte) = Post(system, catalog);

            foreach (var view in new[] { "LATERAL", "FRONTAL", "PLANTA" })
            {
                var entry = catalog.ConnectionLayout.FindConnectionLayout(
                    postId, PushBackRearTopeBuilder.AnchorPoint(view), view);
                Assert.NotNull(entry);   // the shipped catalog measures the required point in each view

                var resolved = PushBackRearTopeBuilder.PostAnchorLocal(catalog, postId, peralte, view);
                Assert.True(resolved.HasValue);
                var expected = SelectivePostGeometry.Resolve(
                    entry, new Dictionary<string, double> { [SelectiveRackDefaults.PeralteParam] = peralte });
                Assert.Equal(expected.X, resolved.Value.X, 9);
                Assert.Equal(expected.Y, resolved.Value.Y, 9);
            }

            // Owner decision (2026-07-24, final): each view has its OWN point — LATERAL the separator, FRONTAL and
            // PLANTA the post's own TROQUEL_TOPE — and none of them is TROQUEL_LARGUERO.
            Assert.Equal(DynamicRackDefaults.SeparatorPostPoint, PushBackRearTopeBuilder.AnchorPoint("LATERAL"));
            Assert.Equal(PushBackRearTopeBuilder.TopePostPoint, PushBackRearTopeBuilder.AnchorPoint("FRONTAL"));
            Assert.Equal(PushBackRearTopeBuilder.TopePostPoint, PushBackRearTopeBuilder.AnchorPoint("PLANTA"));
            foreach (var view in new[] { "LATERAL", "FRONTAL", "PLANTA" })
            {
                Assert.NotEqual(SelectiveRackDefaults.PostBeamPoint, PushBackRearTopeBuilder.AnchorPoint(view));
            }

            // PLANTA carries its own depth offset, driven by the post peralte — it is NOT the lateral point.
            var lateral = PushBackRearTopeBuilder.PostAnchorLocal(catalog, postId, peralte, "LATERAL").Value;
            var planta = PushBackRearTopeBuilder.PostAnchorLocal(catalog, postId, peralte, "PLANTA").Value;
            Assert.NotEqual(lateral.Y, planta.Y);
        }

        [Fact]
        public void NoAnchor_AndNoRawPlacementFallback_WhenThePostHasNoSeparatorPoint()
        {
            var catalog = Catalog;
            Assert.Null(PushBackRearTopeBuilder.PostAnchorLocal(catalog, "POSTE_SIN_SEPARADOR", 3.0, "LATERAL"));
            Assert.Null(PushBackRearTopeBuilder.PostAnchorLocal(null, "CUALQUIERA", 3.0, "LATERAL"));
        }

        [Fact]
        public void TopeOrigin_SitsOnTheSeparatorAxis_InLateralAndInPlanta()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];
            var (postId, peralte) = Post(system, catalog);
            var builder = new PushBackRearTopeBuilder();
            var rearBeams = DynamicLoadBeamGeometry.Placements(system.Structure, front)
                .Where(p => p.IsEntrance).ToList();
            Assert.NotEmpty(rearBeams);

            foreach (var view in new[] { "LATERAL", "PLANTA" })
            {
                var separator = PushBackRearTopeBuilder.PostAnchorLocal(catalog, postId, peralte, view).Value;
                var topes = builder.Build(system, catalog, 0, front, view);
                Assert.NotEmpty(topes);

                foreach (var tope in topes)
                {
                    var expectedXs = rearBeams
                        .Select(b => b.X + (b.MirroredX ? -separator.X : separator.X))
                        .ToList();
                    Assert.Contains(expectedXs, x => Math.Abs(x - tope.Insertion.X) < 1e-9);

                    // It is NOT the bare placement X: the separator axis is offset from the post's insertion.
                    Assert.DoesNotContain(rearBeams, b => Math.Abs(b.X - tope.Insertion.X) < 1e-9);
                }

                // PLANTA also takes the separator's own depth offset.
                if (string.Equals(view, "PLANTA", StringComparison.Ordinal))
                {
                    Assert.All(topes, tope => Assert.Contains(
                        rearBeams, b => Math.Abs(b.Y + separator.Y - tope.Insertion.Y) < 1e-9));
                }
            }
        }

        [Fact]
        public void TopeAnchor_DoesNotUseTheRearBeamsContactPoints()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];
            var beamId = string.IsNullOrWhiteSpace(system.HighEndBeamCatalogId)
                ? PushBackDefaults.HighEndBeamCatalogId
                : system.HighEndBeamCatalogId;

            var topes = new PushBackRearTopeBuilder().BuildLateral(system, catalog, 0, front);
            Assert.NotEmpty(topes);

            var beamEdges = DynamicLoadBeamGeometry.Placements(system.Structure, front)
                .Where(p => p.IsEntrance)
                .SelectMany(p => new[]
                {
                    PushBackDefaults.HighEndBeamLeftBedMatePoint,
                    PushBackDefaults.HighEndBeamRightBedMatePoint
                }.Select(pointId =>
                {
                    var e = catalog.ConnectionLayout.FindConnectionLayout(
                        beamId, pointId, PushBackDefaults.HighEndBeamView);
                    return e == null ? double.NaN : p.X + (p.MirroredX ? -e.LocalX : e.LocalX);
                }))
                .Where(x => !double.IsNaN(x))
                .ToList();

            Assert.All(topes, tope => Assert.DoesNotContain(beamEdges, x => Math.Abs(x - tope.Insertion.X) < 1e-9));
        }

        // ---- (2) the orientation is the inverse of 10d8eeb ----

        [Fact]
        public void ElevationOrientation_IsInvertedWithRespectTo10d8eeb()
        {
            // 10d8eeb drew the elevations MIRRORED and the Owner measured the stop upside down; it is now unmirrored,
            // and still never reads the rear beam's own mirror.
            Assert.False(PushBackRearTopeBuilder.ElevationMirrored);
            Assert.False(PushBackRearTopeBuilder.Mirrored("LATERAL", beamMirroredX: true));
            Assert.False(PushBackRearTopeBuilder.Mirrored("LATERAL", beamMirroredX: false));
            Assert.False(PushBackRearTopeBuilder.Mirrored("FRONTAL", beamMirroredX: true));
            Assert.False(PushBackRearTopeBuilder.Mirrored("FRONTAL", beamMirroredX: false));

            // PLANTA: the Owner measured the block inverted there too, so its orientation is now the INVERSE of the
            // beam's plan mirror.
            Assert.False(PushBackRearTopeBuilder.Mirrored("PLANTA", beamMirroredX: true));
            Assert.True(PushBackRearTopeBuilder.Mirrored("PLANTA", beamMirroredX: false));
        }

        [Fact]
        public void LateralAndRearFrontalTopes_AreDrawnUnmirrored()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];

            var lateral = new PushBackRearTopeBuilder().BuildLateral(system, catalog, 0, front);
            Assert.NotEmpty(lateral);
            Assert.All(lateral, tope => Assert.False(tope.MirroredX));

            var frontal = new PushBackSystemFrontalBuilder()
                .BuildPlan(system, catalog, PushBackFrontalEnd.Posterior).Flatten().Instances
                .Where(i => i.Role == HeaderBlockRole.Tope).ToList();
            Assert.NotEmpty(frontal);
            Assert.All(frontal, tope => Assert.False(tope.MirroredX));
        }

        // ---- the approved contracts stay frozen ----

        [Fact]
        public void ElevationSaqueLongitudSnapAndOffCells_AreUntouched()
        {
            var catalog = Catalog;
            var design = new PushBackDesign { Structure = BaseStructure() };
            design.RearTope.OffCells.Add(new SelectiveGridCell { Frente = 0, Level = 1 });
            var system = new PushBackResolver(catalog).Resolve(design);
            var front = system.Structure.Fronts[0];

            var topes = new PushBackRearTopeBuilder().BuildLateral(system, catalog, 0, front);
            Assert.Equal(Math.Max(1, front.LoadLevels) - 1, topes.Count);   // OffCells still removes its cell

            // PB-VAL-03 stays approved: the elevation is the canonical rise-and-snap plus exactly 4".
            Assert.Equal(4.0, PushBackRearTopeBuilder.ExtraRise, 9);
            var rearBeams = DynamicLoadBeamGeometry.Placements(system.Structure, front).Where(p => p.IsEntrance).ToList();
            var gridBase = PostGridBase(system, catalog);
            Assert.All(topes, tope => Assert.Contains(
                rearBeams, b => Math.Abs(PushBackRearTopeBuilder.ElevationY(gridBase, b.Y) - tope.Insertion.Y) < 1e-9));

            var expectedLongitud = PushBackLoadBeamGeometry.CellBeamLength(system.Structure, front, 1)
                + SelectiveTopePlacement.LengthAllowance;
            Assert.All(topes, tope =>
            {
                Assert.Equal(PushBackDefaults.RearTopeSaque, tope.DynamicParameters[SelectiveSafetyPlacement.SaqueParam], 9);
                Assert.Equal(expectedLongitud, tope.DynamicParameters[SelectiveRackDefaults.LengthParam], 6);
                Assert.Equal(HeaderBlockRole.Tope, tope.Role);
                Assert.Equal(PushBackRearTopeBuilder.TopePieceId, tope.PieceId);
            });
        }

        private static double PostGridBase(PushBackSystem system, RackCatalog catalog)
        {
            var postId = DynamicFrontGeometry.PostId(system.Structure, catalog);
            var postPeralte = DynamicFrontGeometry.PostPeralte(system.Structure, catalog, postId);
            return PushBackRearTopeBuilder.PostAnchorLocal(catalog, postId, postPeralte, "LATERAL")?.Y ?? 0.0;
        }
    }
}
