using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-18b — characterization of the defects the Owner found in the AutoCAD 2025 manual gate (round 1), so each one is
    /// frozen against regression:
    /// <list type="bullet">
    /// <item>PB-VAL-02 — the rear tope inherited the rear BEAM's mirror and was drawn inverted.</item>
    /// <item>PB-VAL-03 — the rear tope sat exactly 4" below its required elevation.</item>
    /// <item>PB-VAL-04 — a brand-new Push Back system carried NO safety defaults at all.</item>
    /// </list>
    /// PB-VAL-01 (window layout) is a UI concern covered by the UI suite; PB-VAL-05 (low-beam tangency) is NOT fixed in
    /// this round and is deliberately not pinned here.
    /// </summary>
    public class PushBackOwnerRejectionRound1Tests
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

        private static double TroquelMateY(PushBackSystem system, RackCatalog catalog, string view)
        {
            var postId = DynamicFrontGeometry.PostId(system.Structure, catalog);
            var postPeralte = DynamicFrontGeometry.PostPeralte(system.Structure, catalog, postId);
            var entry = catalog.ConnectionLayout.FindConnectionLayout(postId, SelectiveRackDefaults.PostBeamPoint, view);
            return SelectivePostGeometry.Resolve(
                entry, new Dictionary<string, double> { [SelectiveRackDefaults.PeralteParam] = postPeralte }).Y;
        }

        // ---- PB-VAL-03: the rear tope rises exactly 4" more than the canonical Selective snap ----

        [Fact]
        public void PbVal03_ExtraRise_IsExactlyFourInches_AndAWholeNumberOfTroquelPasos()
        {
            Assert.Equal(4.0, PushBackRearTopeBuilder.ExtraRise, 9);

            // 4" is exactly 2 pasos, so the tope stays on the SAME troquel grid the Selective snap lands on.
            var steps = PushBackRearTopeBuilder.ExtraRise / SelectiveRackDefaults.TroquelPaso;
            Assert.Equal(Math.Round(steps), steps, 9);
        }

        [Fact]
        public void PbVal03_ElevationY_IsTheSelectiveSnapPlusFour_AndStaysOnTheGrid()
        {
            const double mate = 3.0;
            const double larguero = 41.0;

            var snap = SelectiveTopePlacement.SnapY(mate, larguero, SelectiveRackDefaults.TroquelPaso);
            var raised = PushBackRearTopeBuilder.ElevationY(mate, larguero);

            Assert.Equal(snap + 4.0, raised, 9);                                     // exactly 4" higher than before
            var stepsFromMate = (raised - mate) / SelectiveRackDefaults.TroquelPaso; // still a whole number of pasos
            Assert.Equal(Math.Round(stepsFromMate), stepsFromMate, 9);
        }

        [Fact]
        public void PbVal03_LateralTopes_SitFourInchesAboveTheOldRule()
        {
            var catalog = Catalog;
            var system = System(catalog);
            var mateY = TroquelMateY(system, catalog, SelectiveRackDefaults.View);
            var front = system.Structure.Fronts[0];

            var topes = new PushBackRearTopeBuilder().BuildLateral(system, catalog, 0, front);
            Assert.NotEmpty(topes);

            var beams = DynamicLoadBeamGeometry.Placements(system.Structure, front).Where(p => p.IsEntrance).ToList();
            foreach (var tope in topes)
            {
                var source = beams.FirstOrDefault(b =>
                    Math.Abs(PushBackRearTopeBuilder.ElevationY(mateY, b.Y) - tope.Insertion.Y) < 1e-6);
                Assert.NotNull(source);
                var old = SelectiveTopePlacement.SnapY(mateY, source.Y, SelectiveRackDefaults.TroquelPaso);
                Assert.Equal(old + 4.0, tope.Insertion.Y, 6);   // the Owner-measured correction, exactly
            }
        }

        // ---- PB-VAL-02: orientation is an explicit rear-tope rule, never the beam's mirror ----

        [Fact]
        public void PbVal02_ElevationViews_DrawTheRearTopeUnmirrored_EvenThoughTheRearBeamIsMirrored()
        {
            // The rear placement is mirrored (it is the dynamic ENTRANCE beam); the tope must NOT inherit that.
            Assert.False(PushBackRearTopeBuilder.Mirrored("LATERAL", beamMirroredX: true));
            Assert.False(PushBackRearTopeBuilder.Mirrored("FRONTAL", beamMirroredX: true));

            // PLANTA is a top view: the tope lies along the beam and keeps its plan orientation.
            Assert.True(PushBackRearTopeBuilder.Mirrored("PLANTA", beamMirroredX: true));
            Assert.False(PushBackRearTopeBuilder.Mirrored("PLANTA", beamMirroredX: false));
        }

        [Fact]
        public void PbVal02_LateralAndRearFrontalTopes_AreUnmirrored()
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

        [Fact]
        public void PbVal02And03_DoNotChangeSaqueLengthOrPerCellActivation()
        {
            var catalog = Catalog;
            var design = new PushBackDesign { Structure = BaseStructure() };
            design.RearTope.OffCells.Add(new SelectiveGridCell { Frente = 0, Level = 1 });   // level 2 of front 1 off
            var system = new PushBackResolver(catalog).Resolve(design);
            var front = system.Structure.Fronts[0];

            var topes = new PushBackRearTopeBuilder().BuildLateral(system, catalog, 0, front);

            Assert.Equal(Math.Max(1, front.LoadLevels) - 1, topes.Count);   // the deactivated cell still has no tope
            Assert.All(topes, tope =>
            {
                Assert.True(tope.DynamicParameters.ContainsKey(SelectiveSafetyPlacement.SaqueParam));
                Assert.True(tope.DynamicParameters[SelectiveSafetyPlacement.SaqueParam] > 0.0);
                Assert.Equal(PushBackRearTopeBuilder.TopePieceId, tope.PieceId);   // same piece, never swapped
            });
        }

        [Fact]
        public void PbVal02And03_DoNotChangeTheBom()
        {
            // Orientation and elevation are drawing concerns: the bill of materials is untouched by them.
            var catalog = Catalog;
            var system = System(catalog);
            var bom = PushBackBomBuilder.Build(system, catalog);

            var topes = bom.Components.Where(c => c.Category == PushBackBomBuilder.RearTope).Sum(c => c.Quantity);
            var cells = system.Structure.Fronts.Sum(f => Math.Max(1, f.LoadLevels));
            Assert.Equal(cells, topes);
        }

        // ---- PB-VAL-04: a new Push Back system opens WITH the catalog-driven safety defaults ----

        [Fact]
        public void PbVal04_Defaults_AreNotEmpty_AndComeFromTheSharedCatalogAuthority()
        {
            var catalog = Catalog;
            var defaults = new PushBackSafetyAuthority(catalog).Defaults();

            Assert.NotEmpty(defaults);                       // the whole point of PB-VAL-04
            var dynamicFamilies = DynamicSafetyDefaults.Build(catalog);
            Assert.NotEmpty(dynamicFamilies);
            Assert.All(defaults, selection => Assert.Contains(
                dynamicFamilies, d => string.Equals(d.ElementId, selection.ElementId, StringComparison.OrdinalIgnoreCase)));
        }

        [Fact]
        public void PbVal04_Defaults_AreLowEndOnly_AndNeverIncludeGuia()
        {
            var catalog = Catalog;
            var authority = new PushBackSafetyAuthority(catalog);
            var defaults = authority.Defaults();

            // The low-end guarantee: NOTHING may sit at the rear. A two-ended/rear side collapses to Left; a family the
            // shared defaults leave unsided (Side.None) stays unsided — what matters is that Right/Both never survive.
            Assert.All(defaults, selection => Assert.NotEqual(SafetySide.Right, selection.Side));
            Assert.All(defaults, selection => Assert.NotEqual(SafetySide.Both, selection.Side));
            Assert.Contains(defaults, selection => selection.Side == SafetySide.Left);   // at least one really is placed low
            Assert.All(defaults, selection => Assert.False(authority.IsEntranceGuide(selection)));
            Assert.All(defaults, selection => Assert.DoesNotContain(selection.PostSides, side => side != null));
        }

        [Fact]
        public void PbVal04_Defaults_DropExactlyTheGuiaTheDynamicDefaultsCarry()
        {
            var catalog = Catalog;
            var authority = new PushBackSafetyAuthority(catalog);

            var dynamicDefaults = DynamicSafetyDefaults.Build(catalog);
            var guias = dynamicDefaults.Count(authority.IsEntranceGuide);
            Assert.Equal(dynamicDefaults.Count - guias, authority.Defaults().Count);
        }

        [Fact]
        public void PbVal04_Defaults_DoNotMutateTheSharedDynamicDefaults()
        {
            var catalog = Catalog;
            var before = DynamicSafetyDefaults.Build(catalog).Select(s => (s.ElementId, s.Side)).ToList();

            var defaults = new PushBackSafetyAuthority(catalog).Defaults();
            Assert.NotEmpty(defaults);

            var after = DynamicSafetyDefaults.Build(catalog).Select(s => (s.ElementId, s.Side)).ToList();
            Assert.Equal(before, after);   // the shared authority is untouched (deep copies all the way down)
        }

        [Fact]
        public void PbVal04_DefaultsSurviveIntoTheResolvedSystem_LowEndOnly()
        {
            var catalog = Catalog;
            var design = new PushBackDesign { Structure = BaseStructure() };
            foreach (var selection in new PushBackSafetyAuthority(catalog).Defaults())
            {
                design.Structure.SafetySelections.Add(selection);
            }

            var system = new PushBackResolver(catalog).Resolve(design);

            Assert.NotEmpty(system.SafetySelections);
            Assert.All(system.SafetySelections, selection => Assert.NotEqual(SafetySide.Right, selection.Side));
            Assert.All(system.SafetySelections, selection => Assert.NotEqual(SafetySide.Both, selection.Side));
            Assert.All(system.SafetySelections,
                selection => Assert.False(new PushBackSafetyAuthority(catalog).IsEntranceGuide(selection)));
        }
    }
}
