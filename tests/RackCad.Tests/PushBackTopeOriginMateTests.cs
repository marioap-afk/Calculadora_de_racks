using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Geometry;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Owner clarification (2026-07-25) — the <c>LARGUERO_ESCALON_TOPE_DE_3</c> block mates by its ORIGIN: its insertion
    /// point IS its mate point, which is why the block publishes no connection point of its own. Placing the stop means
    /// putting that origin exactly on the POST's <c>TROQUEL_TOPE</c>, in WORLD coordinates, in the corresponding view.
    ///
    /// These tests transform both sides and assert the coincidence:
    /// <list type="bullet">
    /// <item>PLANTA — full coincidence (no elevation): X and Y.</item>
    /// <item>FRONTAL posterior — coincidence in X (the post's stop column) with the Y on that same column, a whole
    /// number of 2" troquel steps up, because the approved rise-and-snap (+4") chooses the hole of the level.</item>
    /// </list>
    /// The mate is resolved against the POST instance of the plan, never against the rear beam's insertion, which is
    /// what left the stop on the larguero troquel in 90d3b3a.
    /// </summary>
    public class PushBackTopeOriginMateTests
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

        /// <summary>The world TROQUEL_TOPE of the post nearest to <paramref name="near"/>, computed independently here.</summary>
        private static Point2D PostTroquelTopeWorld(
            RackCatalog catalog, string postId, double peralte, string view,
            IReadOnlyList<HeaderBlockInstance> plan, Point2D near)
        {
            var row = catalog.ConnectionLayout.FindConnectionLayout(postId, "TROQUEL_TOPE", view);
            Assert.True(row != null, $"connection-layout.csv has no row {postId},TROQUEL_TOPE,{view}");
            var local = SelectivePostGeometry.Resolve(
                row, new Dictionary<string, double> { [SelectiveRackDefaults.PeralteParam] = peralte });

            var post = plan
                .Where(i => i.Role == HeaderBlockRole.Post)
                .OrderBy(i => (i.Insertion.X - near.X) * (i.Insertion.X - near.X)
                              + (i.Insertion.Y - near.Y) * (i.Insertion.Y - near.Y))
                .FirstOrDefault();
            Assert.True(post != null, "the plan carries no post to mate against");

            return new Point2D(
                post.Insertion.X + (post.MirroredX ? -local.X : local.X),
                post.Insertion.Y + local.Y);
        }

        // ---- PLANTA: the origins coincide, both coordinates ----

        [Fact]
        public void Planta_TopeOrigin_CoincidesWithThePostTroquelTope()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var (postId, peralte) = Post(system, catalog);

            var plan = new PushBackSystemPlantaBuilder().BuildPlan(system, catalog).Flatten().Instances;
            var topes = plan.Where(i => i.Role == HeaderBlockRole.Tope).ToList();
            Assert.NotEmpty(topes);

            foreach (var tope in topes)
            {
                var mate = PostTroquelTopeWorld(catalog, postId, peralte, "PLANTA", plan, tope.Insertion);
                Assert.Equal(mate.X, tope.Insertion.X, 9);
                Assert.Equal(mate.Y, tope.Insertion.Y, 9);
            }
        }

        // ---- FRONTAL: the column coincides, and the height is on that column ----

        [Fact]
        public void RearFrontal_TopeOrigin_SitsOnThePostTroquelTopeColumn()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var (postId, peralte) = Post(system, catalog);

            var plan = new PushBackSystemFrontalBuilder()
                .BuildPlan(system, catalog, PushBackFrontalEnd.Posterior).Flatten().Instances;
            var topes = plan.Where(i => i.Role == HeaderBlockRole.Tope).ToList();
            Assert.NotEmpty(topes);

            foreach (var tope in topes)
            {
                var mate = PostTroquelTopeWorld(catalog, postId, peralte, "FRONTAL", plan, tope.Insertion);

                // X: exact coincidence with the post's stop column.
                Assert.Equal(mate.X, tope.Insertion.X, 9);

                // Y: the same column, a whole number of 2" holes up (the approved rise-and-snap picks the level's hole).
                var steps = (tope.Insertion.Y - mate.Y) / SelectiveRackDefaults.TroquelPaso;
                Assert.Equal(Math.Round(steps), steps, 6);
                Assert.True(steps > 0.0);
            }
        }

        // ---- the previous implementation does NOT satisfy the mate ----

        [Fact]
        public void TheBeamAnchoredPlacement_Of90d3b3a_DoesNotSatisfyTheMate()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var (postId, peralte) = Post(system, catalog);

            // PLANTA is where 90d3b3a is provably wrong: it placed the stop at the BEAM's insertion VERBATIM
            // (PushBackSystemPlantaBuilder used instance.Insertion.X / .Y), which is the larguero troquel — exactly what
            // the Owner still measured. Reproduce that formula and show it misses the mate.
            var plan = new PushBackSystemPlantaBuilder().BuildPlan(system, catalog).Flatten().Instances;
            var beams = plan.Where(i => i.Role == HeaderBlockRole.Beam && i.MirroredX).ToList();
            var topes = plan.Where(i => i.Role == HeaderBlockRole.Tope).ToList();
            Assert.NotEmpty(topes);
            Assert.NotEmpty(beams);

            foreach (var tope in topes)
            {
                var mate = PostTroquelTopeWorld(catalog, postId, peralte, "PLANTA", plan, tope.Insertion);

                // The old placement (the rear beam's own insertion) is NOT the mate...
                Assert.DoesNotContain(beams, b =>
                    Math.Abs(b.Insertion.X - mate.X) < 1e-6 && Math.Abs(b.Insertion.Y - mate.Y) < 1e-6);

                // ...and the stop is no longer sitting on it.
                Assert.DoesNotContain(beams, b =>
                    Math.Abs(b.Insertion.X - tope.Insertion.X) < 1e-6 && Math.Abs(b.Insertion.Y - tope.Insertion.Y) < 1e-6);
            }
        }

        [Fact]
        public void MirroringAlone_WithoutTranslating_DoesNotSatisfyTheMate()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var (postId, peralte) = Post(system, catalog);

            var plan = new PushBackSystemPlantaBuilder().BuildPlan(system, catalog).Flatten().Instances;
            var beams = plan.Where(i => i.Role == HeaderBlockRole.Beam).ToList();
            var topes = plan.Where(i => i.Role == HeaderBlockRole.Tope).ToList();
            Assert.NotEmpty(topes);

            foreach (var tope in topes)
            {
                var mate = PostTroquelTopeWorld(catalog, postId, peralte, "PLANTA", plan, tope.Insertion);

                // Take the beam this stop belongs to and flip ONLY its mirror, leaving it where it is: the origin does
                // not reach the post's TROQUEL_TOPE. The block has to be TRANSLATED, not just mirrored.
                foreach (var beam in beams)
                {
                    var mirroredOnly = new Point2D(beam.Insertion.X, beam.Insertion.Y);
                    Assert.True(
                        Math.Abs(mirroredOnly.X - mate.X) > 1e-6 || Math.Abs(mirroredOnly.Y - mate.Y) > 1e-6,
                        "mirroring without translating must not land on the mate");
                }
            }
        }

        // ---- what must not change ----

        [Fact]
        public void SaqueLongitudExtraRiseOffCellsAndBom_AreUnchanged()
        {
            var catalog = Catalog;
            var design = new PushBackDesign { Structure = BaseStructure() };
            design.RearTope.OffCells.Add(new SelectiveGridCell { Frente = 0, Level = 1 });
            var system = new PushBackResolver(catalog).Resolve(design);

            Assert.Equal(4.0, PushBackRearTopeBuilder.ExtraRise, 9);

            var plan = new PushBackSystemFrontalBuilder()
                .BuildPlan(system, catalog, PushBackFrontalEnd.Posterior).Flatten().Instances;
            var topes = plan.Where(i => i.Role == HeaderBlockRole.Tope).ToList();
            Assert.NotEmpty(topes);
            Assert.All(topes, tope =>
            {
                Assert.Equal(PushBackRearTopeBuilder.TopePieceId, tope.PieceId);
                Assert.Equal(PushBackDefaults.RearTopeSaque, tope.DynamicParameters[SelectiveSafetyDefaults.SaqueParam], 9);
                Assert.True(tope.DynamicParameters.ContainsKey(SelectiveRackDefaults.LengthParam));
            });

            // OffCells still removes its cell, and the BOM still counts the surviving stops.
            var front = system.Structure.Fronts[0];
            var lateral = new PushBackRearTopeBuilder().BuildLateral(system, catalog, 0, front);
            Assert.Equal(Math.Max(1, front.LoadLevels) - 1, lateral.Count);
            Assert.True(PushBackBomBuilder.Build(system, catalog).Lines
                .Where(l => string.Equals(l.ProfileId, PushBackRearTopeBuilder.TopePieceId, StringComparison.OrdinalIgnoreCase))
                .Sum(l => l.Quantity) > 0);
        }

        [Fact]
        public void NoPathUsesTroquelLarguero_ToPlaceTheTope()
        {
            foreach (var view in new[] { "LATERAL", "FRONTAL", "PLANTA" })
            {
                Assert.NotEqual(SelectiveRackDefaults.PostBeamPoint, PushBackRearTopeBuilder.AnchorPoint(view));
            }
        }

        [Fact]
        public void NoFallback_WhenThePlanHasNoPost_OrTheRowIsMissing()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var (postId, peralte) = Post(system, catalog);

            // No post in the plan -> no mate (rather than the beam's origin).
            Assert.Null(PushBackRearTopeBuilder.PostMateWorld(
                catalog, postId, peralte, "PLANTA", Array.Empty<HeaderBlockInstance>(), new Point2D(0, 0)));

            // No measured row -> no mate.
            var plan = new PushBackSystemPlantaBuilder().BuildPlan(system, catalog).Flatten().Instances;
            Assert.Null(PushBackRearTopeBuilder.PostMateWorld(
                catalog, "POSTE_INEXISTENTE", peralte, "PLANTA", plan, new Point2D(0, 0)));
            Assert.Null(PushBackRearTopeBuilder.PostMateWorld(
                null, postId, peralte, "PLANTA", plan, new Point2D(0, 0)));
        }
    }
}
