using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Direct unit coverage for <see cref="SelectiveTopePlan.BuildFrontal"/> (I-22, defecto 1): the FRONTAL result of
    /// the tope family, resolved in Application so the frontal builder keeps only the catalogued-point + snap
    /// projection. The golden equivalence tests freeze the placed output; these pin the pure intent the plan now owns —
    /// off-cells, levels, loaded/unloaded tramos, offsets, longitudes and the source larguero Y — and, crucially, that
    /// the frontal is per-frente and NEVER multiplied by the physical spots (no duplication when TopeShared=false).
    /// </summary>
    public class SelectiveTopePlanFrontalTests
    {
        private const string PostId = TestCatalogIds.Profiles.Posts.Standard;
        private const string BeamId = TestCatalogIds.Profiles.Beams.SelectiveThreeRivet;
        private const string TopeId = TestCatalogIds.Safety.Stops.Post;

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static SelectiveBayDesign Bay(int levels)
        {
            var bay = new SelectiveBayDesign { FloorBeam = true };
            for (var l = 0; l < levels; l++)
            {
                bay.Levels.Add(new SelectiveCell
                {
                    Pallet = new Tarima { Frente = 40.0, Alto = 45.0 + l * 5.0 },
                    PalletCount = 2, BeamId = BeamId, BeamPeralte = 4.0
                });
            }

            return bay;
        }

        private static SelectiveBayDesign MedioBay(int levels, params (double Length, bool Loaded)[] tramos)
        {
            var bay = Bay(levels);
            foreach (var (length, loaded) in tramos) bay.Segments.Add(new SelectiveSegment { Length = length, Loaded = loaded });
            return bay;
        }

        private static SelectiveRackSystem Resolve(SelectivePalletDesign design)
            => new SelectiveGeometryResolver().Resolve(design, Catalog);

        private static SelectiveSafetySelection Tope(bool shared, SafetySide side = SafetySide.Both)
            => new SelectiveSafetySelection { ElementId = TopeId, Side = side, TopeShared = shared, TopeFrontal = true };

        // ---- Off cell + several levels: each grid-on (frente,level) draws one full-bay tope; the off cell is gone ----
        [Fact]
        public void BuildFrontal_RemovesOffCell_AndCarriesEveryGridOnLevel()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0, PalletDepth = 48.0
            };
            design.Bays.Add(Bay(2));
            design.Bays.Add(Bay(2));
            var tope = Tope(shared: true);
            tope.TopeOffCells.Add(new SelectiveGridCell { Frente = 1, Level = 1 });
            design.SafetySelections.Add(tope);

            var system = Resolve(design);
            var cells = SelectiveTopePlan.BuildFrontal(SelectiveDepthLayout.FondoSystemView(system, 0), Catalog);

            var frente0 = cells.Single(c => c.Frente == 0);
            var frente1 = cells.Single(c => c.Frente == 1);

            // Frente 0: both levels on; frente 1: level 1 removed (the off cell).
            Assert.Equal(new[] { 0, 1 }, frente0.Topes.Select(t => t.Level).ToArray());
            Assert.Equal(new[] { 0 }, frente1.Topes.Select(t => t.Level).ToArray());

            // Each tope rises from its own larguero Y, and a full bay spans the whole beam (+ the allowance) at offset 0.
            var bay0 = system.Bays[0];
            Assert.Equal(bay0.Levels[0].Y, frente0.Topes[0].SourceY, 6);
            Assert.Equal(bay0.Levels[1].Y, frente0.Topes[1].SourceY, 6);
            Assert.NotEqual(frente0.Topes[0].SourceY, frente0.Topes[1].SourceY);
            Assert.All(frente0.Topes, t => Assert.Equal(0.0, t.StartOffset, 6));
            Assert.All(frente0.Topes, t => Assert.Equal(bay0.BeamLength + SelectiveTopePlacement.LengthAllowance, t.Longitud, 6));
        }

        // ---- Medio frente: a tope per LOADED tramo (the middle empty one is skipped), offsets increase left→right ----
        [Fact]
        public void BuildFrontal_MedioFrente_DrawsPerLoadedTramo_SkippingTheUnloaded()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0, PalletDepth = 48.0
            };
            design.Bays.Add(MedioBay(1, (36.0, true), (30.0, false), (0.0, true))); // 3 tramos, middle unloaded
            design.SafetySelections.Add(Tope(shared: true));

            var system = Resolve(design);
            var cells = SelectiveTopePlan.BuildFrontal(SelectiveDepthLayout.FondoSystemView(system, 0), Catalog);

            var cell = Assert.Single(cells);
            // Only the two loaded tramos draw (the 30" middle one does not); one level.
            Assert.Equal(2, cell.Topes.Count);
            Assert.All(cell.Topes, t => Assert.Equal(0, t.Level));

            // First loaded tramo: the specified 36" larguero (+ allowance) at the bay's left post (offset 0).
            Assert.Equal(0.0, cell.Topes[0].StartOffset, 6);
            Assert.Equal(36.0 + SelectiveTopePlacement.LengthAllowance, cell.Topes[0].Longitud, 6);
            // The last loaded tramo sits to the RIGHT of the first, past the 36" tramo and its intermediate posts.
            Assert.True(cell.Topes[1].StartOffset > 36.0);
            Assert.True(cell.Topes[1].Longitud > SelectiveTopePlacement.LengthAllowance);
        }

        // ---- A non-split bay draws exactly one full-bay tope per grid-on level, at offset 0 ----
        [Fact]
        public void BuildFrontal_FullBay_DrawsOneTopePerLevel_AtOffsetZero()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0, PalletDepth = 48.0
            };
            design.Bays.Add(Bay(3));
            design.SafetySelections.Add(Tope(shared: true));

            var system = Resolve(design);
            var cells = SelectiveTopePlan.BuildFrontal(SelectiveDepthLayout.FondoSystemView(system, 0), Catalog);

            var cell = Assert.Single(cells);
            Assert.Equal(new[] { 0, 1, 2 }, cell.Topes.Select(t => t.Level).ToArray());
            Assert.All(cell.Topes, t => Assert.Equal(0.0, t.StartOffset, 6));
            Assert.All(cell.Topes, t => Assert.Equal(system.Bays[0].BeamLength + SelectiveTopePlacement.LengthAllowance, t.Longitud, 6));
        }

        // ---- No duplication when TopeShared=false: the frontal is per-frente, NOT multiplied by the physical spots ----
        [Fact]
        public void BuildFrontal_TopeSharedFalse_DoesNotDuplicatePerSpot()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0,
                PalletDepth = 48.0, DepthCount = 2
            };
            design.Bays.Add(Bay(2));
            design.Bays.Add(Bay(2));
            design.ExtraFondoBays.Add(new List<SelectiveBayDesign> { Bay(2), Bay(2) });
            // TopeShared=false + Both: the PHYSICAL plan resolves TWO spots for this pair; the frontal must NOT double.
            design.SafetySelections.Add(Tope(shared: false, side: SafetySide.Both));

            var system = Resolve(design);
            var fondo0 = SelectiveDepthLayout.FondoSystemView(system, 0);
            var cells = SelectiveTopePlan.BuildFrontal(fondo0, Catalog);

            // Exactly one tope per grid-on (frente, level): 2 bays × 2 levels = 4 — not 8.
            var total = cells.Sum(c => c.Topes.Count);
            Assert.Equal(4, total);

            // Sanity: the physical spot plan DOES resolve more than one spot here, proving the frontal deliberately diverges.
            Assert.True(SelectiveTopePlan.Build(system, Catalog).Count > 1);
        }

        // ---- Empty when no tope family is selected ----
        [Fact]
        public void BuildFrontal_NoTopeSelected_IsEmpty()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0, PalletDepth = 48.0
            };
            design.Bays.Add(Bay(2));

            var system = Resolve(design);
            Assert.Empty(SelectiveTopePlan.BuildFrontal(SelectiveDepthLayout.FondoSystemView(system, 0), Catalog));
        }
    }
}
