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
    /// I-18a corrective run: closes the six review findings (per-cell level assignment in the rear frontal, the planta
    /// enveloping peralte, the per-cell IN/OUT BOM, the single low-end safety authority, the canonical rear-tope
    /// rise-and-snap, and — in <see cref="PushBackGoldenTests"/> — fixed golden signatures).
    /// </summary>
    public class PushBackCorrectionTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private const string Redondo = "LARGUERO_ESCALON_TROQUEL_REDONDO";
        private const string Tope = "LARGUERO_ESCALON_TOPE_DE_3";

        private static DynamicRackDesign BaseStructure(int levels = 3)
            => new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = levels,
                FirstLevelHeight = 6.0,
                BeamDepth = 4.0
            };

        // ---- Finding 1: rear frontal uses the FRONT'S OWN levels for peralte and tope state -----------------

        [Fact]
        public void RearFrontal_AssignsEachBlockItsOwnCellPeralteAndTopeState_AcrossFrontsWithDifferentHeights()
        {
            var catalog = Catalog;
            var design = new PushBackDesign { Structure = BaseStructure() };
            // Two fronts with DIFFERENT first-level height, level count and rear peraltes.
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 3, PalletsDeep = 4, DepthStartPosition = 1, FirstLevelHeight = 6.0 });
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 4, DepthStartPosition = 1, FirstLevelHeight = 24.0 });
            var f0 = new PushBackFrontConfig(); f0.HighEndBeamPeraltes.Add(5.0); f0.HighEndBeamPeraltes.Add(4.0); f0.HighEndBeamPeraltes.Add(3.5);
            var f1 = new PushBackFrontConfig(); f1.HighEndBeamPeraltes.Add(6.0); f1.HighEndBeamPeraltes.Add(4.5);
            design.Fronts.Add(f0);
            design.Fronts.Add(f1);
            design.RearTope.Disable(0, 1);   // front 0, level 1 off

            var system = new PushBackResolver(catalog).Resolve(design);
            var plan = new PushBackSystemFrontalBuilder().BuildPlan(system, catalog, PushBackFrontalEnd.Posterior).Flatten().Instances;

            // Each rear beam carries ITS OWN cell peralte (not a globally-projected neighbour's).
            var expected = new List<double>();
            for (var f = 0; f < system.Structure.Fronts.Count; f++)
            {
                for (var l = 0; l < system.Structure.Fronts[f].LoadLevels; l++)
                {
                    expected.Add(system.HighEndBeamPeralteAt(f, l));
                }
            }
            var actual = plan.Where(i => i.PieceId == Redondo)
                .Select(i => i.DynamicParameters[SelectiveRackDefaults.PeralteParam]).ToList();
            Assert.Equal(expected.OrderBy(v => v).ToList(), actual.OrderBy(v => v).ToList());

            // Exactly the active cells get a tope (one deactivated of five).
            var activeCells = 0;
            for (var f = 0; f < system.Structure.Fronts.Count; f++)
            {
                for (var l = 0; l < system.Structure.Fronts[f].LoadLevels; l++)
                {
                    if (system.RearTope.At(f, l)) activeCells++;
                }
            }
            Assert.Equal(activeCells, plan.Count(i => i.Role == HeaderBlockRole.Tope));
            Assert.Equal(4, plan.Count(i => i.Role == HeaderBlockRole.Tope));
        }

        // ---- Finding 2: planta carries the ENVELOPING (max) rear peralte ------------------------------------

        [Fact]
        public void Planta_RearBeam_CarriesTheEnvelopingMaxPeralte_NotLevelZero()
        {
            var catalog = Catalog;
            var design = new PushBackDesign { Structure = BaseStructure(levels: 2) };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 4 });
            var f0 = new PushBackFrontConfig(); f0.HighEndBeamPeraltes.Add(3.5); f0.HighEndBeamPeraltes.Add(6.0);
            design.Fronts.Add(f0);

            var system = new PushBackResolver(catalog).Resolve(design);
            Assert.Equal(6.0, PushBackHighEndBeamGeometry.PlantaPeralte(system, 0), 4);

            var redondo = new PushBackSystemPlantaBuilder().Build(system, catalog).Where(i => i.PieceId == Redondo).ToList();
            Assert.NotEmpty(redondo);
            Assert.All(redondo, i => Assert.Equal(6.0, i.DynamicParameters[SelectiveRackDefaults.PeralteParam], 4)); // 6.0, not 3.5
        }

        // ---- Finding 5: rear tope follows the canonical Selective rise-and-snap -----------------------------

        [Fact]
        public void RearTope_Frontal_UsesSelectiveSnapY_NotTheRawBeamElevation()
        {
            var catalog = Catalog;
            var system = new PushBackResolver(catalog).Resolve(new PushBackDesign { Structure = BaseStructure() });
            var plan = new PushBackSystemFrontalBuilder().BuildPlan(system, catalog, PushBackFrontalEnd.Posterior).Flatten().Instances;

            var postId = DynamicFrontGeometry.PostId(system.Structure, catalog);
            var postPeralte = DynamicFrontGeometry.PostPeralte(system.Structure, catalog, postId);
            var troquelEntry = catalog.ConnectionLayout.FindConnectionLayout(postId, SelectiveRackDefaults.PostBeamPoint, "FRONTAL");
            var troquelMateY = SelectivePostGeometry.Resolve(troquelEntry, new Dictionary<string, double> { [SelectiveRackDefaults.PeralteParam] = postPeralte }).Y;

            var topes = plan.Where(i => i.Role == HeaderBlockRole.Tope && i.PieceId == Tope).ToList();
            var beams = plan.Where(i => i.PieceId == Redondo).ToList();
            Assert.NotEmpty(topes);

            foreach (var tope in topes)
            {
                // Several levels share the same X; the tope's source beam is the one whose canonical rear-tope Y matches
                // (PB-VAL-03: the Selective rise-and-snap PLUS the Owner-validated 4" extra rise).
                var sameX = beams.Where(b => Math.Abs(b.Insertion.X - tope.Insertion.X) < 1e-6).ToList();
                var source = sameX.FirstOrDefault(b =>
                    Math.Abs(PushBackRearTopeBuilder.ElevationY(troquelMateY, b.Insertion.Y) - tope.Insertion.Y) < 1e-4);
                Assert.NotNull(source);                                          // Y is exactly the canonical SnapY of a real rear beam
                Assert.True(tope.Insertion.Y > source.Insertion.Y, "the tope must RISE above its larguero, not sit on it");
                Assert.True(tope.DynamicParameters.ContainsKey("SAQUE"));
                Assert.True(tope.DynamicParameters.ContainsKey(SelectiveRackDefaults.LengthParam));
            }
            // The rule genuinely shifts (YOffset = 8" rise then snap): a larguero at 10 does not stay at 10.
            Assert.NotEqual(10.0, SelectiveTopePlacement.SnapY(0.0, 10.0, SelectiveRackDefaults.TroquelPaso));
        }

        [Fact]
        public void RearTope_DeactivatedCell_ProducesNoTopeBlock()
        {
            var catalog = Catalog;
            var design = new PushBackDesign { Structure = BaseStructure() };
            design.RearTope.Disable(0, 0);
            var system = new PushBackResolver(catalog).Resolve(design);

            var withOff = new PushBackSystemFrontalBuilder().BuildPlan(system, catalog, PushBackFrontalEnd.Posterior)
                .Flatten().Instances.Count(i => i.Role == HeaderBlockRole.Tope);
            var full = new PushBackResolver(catalog).Resolve(new PushBackDesign { Structure = BaseStructure() });
            var withAll = new PushBackSystemFrontalBuilder().BuildPlan(full, catalog, PushBackFrontalEnd.Posterior)
                .Flatten().Instances.Count(i => i.Role == HeaderBlockRole.Tope);

            Assert.Equal(withAll - 1, withOff);
        }
    }
}
