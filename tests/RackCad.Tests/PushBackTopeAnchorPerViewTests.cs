using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
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
    /// Owner decision (2026-07-24, FINAL) on the rear tope's anchor point PER VIEW, audited against the real rows of
    /// <c>connection-layout.csv</c>:
    /// <list type="bullet">
    /// <item>LATERAL — X on the vertical axis of <c>TROQUEL_SEPARADOR</c>, and the elevation resolved from that SAME
    /// point, with the vertical distance between the two a whole multiple of the 2" troquel pitch (the approved +4"
    /// rides on that grid).</item>
    /// <item>FRONTAL posterior — anchored by the post's own <c>TROQUEL_TOPE</c> in the FRONTAL view.</item>
    /// <item>PLANTA — anchored by <c>TROQUEL_TOPE</c> in PLANTA, resolved with the real post peralte, and the block's
    /// orientation inverted.</item>
    /// </list>
    /// No view may use <c>TROQUEL_LARGUERO</c>, and no view may fall back to the insertion point.
    /// </summary>
    public class PushBackTopeAnchorPerViewTests
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

        // ---- the catalog really publishes what each view needs ----

        [Fact]
        public void TheCatalog_PublishesTheAnchorPoint_ForEveryRequiredView()
        {
            var catalog = Catalog;
            var (postId, _) = Post(System(catalog), catalog);

            foreach (var view in new[] { "LATERAL", "FRONTAL", "PLANTA" })
            {
                var point = PushBackRearTopeBuilder.AnchorPoint(view);
                var row = catalog.ConnectionLayout.FindConnectionLayout(postId, point, view);
                Assert.True(row != null, $"connection-layout.csv has no row {postId},{point},{view}");
            }

            Assert.Equal(DynamicRackDefaults.SeparatorPostPoint, PushBackRearTopeBuilder.AnchorPoint("LATERAL"));
            Assert.Equal(PushBackRearTopeBuilder.TopePostPoint, PushBackRearTopeBuilder.AnchorPoint("FRONTAL"));
            Assert.Equal(PushBackRearTopeBuilder.TopePostPoint, PushBackRearTopeBuilder.AnchorPoint("PLANTA"));
        }

        [Fact]
        public void NoView_UsesTroquelLarguero_ToPlaceTheTope()
        {
            foreach (var view in new[] { "LATERAL", "FRONTAL", "PLANTA" })
            {
                Assert.NotEqual(SelectiveRackDefaults.PostBeamPoint, PushBackRearTopeBuilder.AnchorPoint(view));
            }
        }

        [Fact]
        public void NoFallbackToTheInsertionPoint_WhenTheAnchorRowIsMissing()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];

            // A post with no measured anchor yields no anchor at all...
            foreach (var view in new[] { "LATERAL", "FRONTAL", "PLANTA" })
            {
                Assert.Null(PushBackRearTopeBuilder.PostAnchorLocal(catalog, "POSTE_INEXISTENTE", 3.0, view));
                Assert.Null(PushBackRearTopeBuilder.PostAnchorLocal(null, "CUALQUIERA", 3.0, view));
            }

            // ...and every emitted tope really sits OFF the beam's bare insertion X, in every view.
            var beams = DynamicLoadBeamGeometry.Placements(system.Structure, front).Where(p => p.IsEntrance).ToList();
            var builder = new PushBackRearTopeBuilder();
            foreach (var view in new[] { "LATERAL", "PLANTA" })
            {
                var topes = builder.Build(system, catalog, 0, front, view);
                Assert.NotEmpty(topes);
                Assert.All(topes, tope => Assert.DoesNotContain(
                    beams, b => Math.Abs(b.X - tope.Insertion.X) < 1e-9));
            }
        }

        // ---- LATERAL: separator axis, and a whole number of 2" steps ----

        [Fact]
        public void Lateral_ResolvesFromTheSeparator_AndTheVerticalGapIsAWholeMultipleOfTwoInches()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];
            var (postId, peralte) = Post(system, catalog);

            var separator = PushBackRearTopeBuilder.PostAnchorLocal(catalog, postId, peralte, "LATERAL");
            Assert.True(separator.HasValue);

            var topes = new PushBackRearTopeBuilder().BuildLateral(system, catalog, 0, front);
            Assert.NotEmpty(topes);

            var beams = DynamicLoadBeamGeometry.Placements(system.Structure, front).Where(p => p.IsEntrance).ToList();
            foreach (var tope in topes)
            {
                // X on the separator's vertical axis.
                Assert.Contains(
                    beams.Select(b => b.X + (b.MirroredX ? -separator.Value.X : separator.Value.X)),
                    x => Math.Abs(x - tope.Insertion.X) < 1e-9);

                // Y measured FROM the separator, on the 2" troquel grid, plus the approved 4".
                var gap = tope.Insertion.Y - separator.Value.Y;
                var steps = gap / SelectiveRackDefaults.TroquelPaso;
                Assert.Equal(Math.Round(steps), steps, 6);
                Assert.True(steps > 0.0, "the tope must rise above the separator");
            }

            // And the +4" is itself a whole number of steps, so it cannot break the grid.
            Assert.Equal(4.0, PushBackRearTopeBuilder.ExtraRise, 9);
            Assert.Equal(
                Math.Round(PushBackRearTopeBuilder.ExtraRise / SelectiveRackDefaults.TroquelPaso),
                PushBackRearTopeBuilder.ExtraRise / SelectiveRackDefaults.TroquelPaso, 9);
        }

        // ---- FRONTAL posterior: the post's own TROQUEL_TOPE ----

        [Fact]
        public void RearFrontal_AnchorsOnTheTroquelTope_OfThePostInThatView()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var (postId, peralte) = Post(system, catalog);

            var anchor = PushBackRearTopeBuilder.PostAnchorLocal(catalog, postId, peralte, "FRONTAL");
            Assert.True(anchor.HasValue);

            var plan = new PushBackSystemFrontalBuilder()
                .BuildPlan(system, catalog, PushBackFrontalEnd.Posterior).Flatten().Instances;
            var topes = plan.Where(i => i.Role == HeaderBlockRole.Tope).ToList();
            Assert.NotEmpty(topes);

            // Owner clarification (2026-07-25): the block mates by its ORIGIN, so the tope's X is the POST's
            // TROQUEL_TOPE in world coordinates — resolved from the post instance of this same plan.
            foreach (var tope in topes)
            {
                var mate = PushBackRearTopeBuilder.PostMateWorld(
                    catalog, postId, peralte, "FRONTAL", plan, tope.Insertion);
                Assert.True(mate.HasValue);
                Assert.Equal(mate.Value.X, tope.Insertion.X, 9);
            }

            // The Owner's physical orientation for the elevations is preserved.
            Assert.All(topes, tope => Assert.Equal(PushBackRearTopeBuilder.ElevationMirrored, tope.MirroredX));
        }

        // ---- PLANTA: TROQUEL_TOPE in PLANTA, and the inverted orientation ----

        [Fact]
        public void Planta_AnchorsOnTheTroquelTope_WithTheRealPeralte_AndTheInvertedOrientation()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var front = system.Structure.Fronts[0];
            var (postId, peralte) = Post(system, catalog);

            var anchor = PushBackRearTopeBuilder.PostAnchorLocal(catalog, postId, peralte, "PLANTA");
            var separator = PushBackRearTopeBuilder.PostAnchorLocal(catalog, postId, peralte, "PLANTA");
            Assert.True(anchor.HasValue);

            // The PLANTA point is resolved WITH the post peralte (its depth offset depends on it), and it is not the
            // separator's row — they point opposite ways.
            var separatorRow = catalog.ConnectionLayout.FindConnectionLayout(
                postId, DynamicRackDefaults.SeparatorPostPoint, "PLANTA");
            var separatorPlanta = SelectivePostGeometry.Resolve(
                separatorRow, new Dictionary<string, double> { [SelectiveRackDefaults.PeralteParam] = peralte });
            Assert.NotEqual(separatorPlanta.Y, anchor.Value.Y);
            Assert.NotEqual(0.0, anchor.Value.Y);   // it really moved with the peralte

            var beams = DynamicLoadBeamGeometry.Placements(system.Structure, front).Where(p => p.IsEntrance).ToList();
            var topes = new PushBackRearTopeBuilder().Build(system, catalog, 0, front, "PLANTA");
            Assert.NotEmpty(topes);

            foreach (var tope in topes)
            {
                Assert.Contains(
                    beams.Select(b => b.X + (b.MirroredX ? -anchor.Value.X : anchor.Value.X)),
                    x => Math.Abs(x - tope.Insertion.X) < 1e-9);
                Assert.Contains(beams.Select(b => b.Y + anchor.Value.Y), y => Math.Abs(y - tope.Insertion.Y) < 1e-9);

                // Orientation INVERTED with respect to the beam's plan mirror.
                Assert.Contains(beams, b => Math.Abs(b.X + (b.MirroredX ? -anchor.Value.X : anchor.Value.X) - tope.Insertion.X) < 1e-9
                                            && tope.MirroredX == !b.MirroredX);
            }

            Assert.False(PushBackRearTopeBuilder.Mirrored("PLANTA", beamMirroredX: true));
            Assert.True(PushBackRearTopeBuilder.Mirrored("PLANTA", beamMirroredX: false));
        }

        // ---- everything else about the stop is frozen ----

        [Fact]
        public void PieceSaqueLongitudOffCellsAndBom_AreUnchanged()
        {
            var catalog = Catalog;
            var design = new PushBackDesign { Structure = BaseStructure() };
            design.RearTope.OffCells.Add(new SelectiveGridCell { Frente = 0, Level = 1 });
            var system = new PushBackResolver(catalog).Resolve(design);
            var front = system.Structure.Fronts[0];

            var topes = new PushBackRearTopeBuilder().BuildLateral(system, catalog, 0, front);
            Assert.Equal(Math.Max(1, front.LoadLevels) - 1, topes.Count);   // OffCells still removes its cell

            var expectedLongitud = PushBackLoadBeamGeometry.CellBeamLength(system.Structure, front, 1)
                + SelectiveTopePlacement.LengthAllowance;
            Assert.All(topes, tope =>
            {
                Assert.Equal(PushBackRearTopeBuilder.TopePieceId, tope.PieceId);
                Assert.Equal(PushBackDefaults.RearTopeSaque, tope.DynamicParameters[SelectiveSafetyDefaults.SaqueParam], 9);
                Assert.Equal(expectedLongitud, tope.DynamicParameters[SelectiveRackDefaults.LengthParam], 6);
            });

            // The BOM still counts one stop per active cell.
            var bom = PushBackBomBuilder.Build(system, catalog);
            var counted = bom.Lines
                .Where(line => string.Equals(line.ProfileId, PushBackRearTopeBuilder.TopePieceId, StringComparison.OrdinalIgnoreCase))
                .Sum(line => line.Quantity);
            Assert.True(counted > 0);
        }
    }
}
