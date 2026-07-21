using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-16 (refactor/draw-services) equivalence baseline. The Plugin DrawServices are thin orchestrators
    /// that funnel every view into a <see cref="DynamicSystemPlan"/> and hand it to the shared writer. I-16
    /// collapses that boilerplate WITHOUT changing the Application plan builders. These tests pin the plan
    /// STRUCTURE each DrawService depends on — the only equivalence surface reachable without AutoCAD — so a
    /// refactor that quietly drops a per-view specialization (the plan factory, the grouped-vs-all-loose
    /// shape, the postIndex/DynamicRackEnd inputs) is caught. Block names, error messages, method signatures
    /// and the single-final-Regen contract live in the Plugin (which references AutoCAD) and are verified by
    /// the mechanical inventory in docs/initiatives/I-16-draw-services-baseline.md, NOT here.
    /// The plans below are reconstructed EXACTLY as the corresponding DrawService builds them.
    /// </summary>
    public class DrawServicePlanBaselineTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        /// <summary>Two-front dynamic system (three transverse post lines), mirrors the multi-view builder tests.</summary>
        private static DynamicRackSystem DynamicSystem()
        {
            var design = DynamicFrontGeometryTests.Design();
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 3 });
            return new DynamicRackSystemResolver(Catalog).Resolve(design).System;
        }

        /// <summary>Single-bay, four-level resolved selective system (levels already placed).</summary>
        private static SelectiveRackSystem SelectiveSystem()
        {
            var system = new SelectiveRackSystem
            {
                Height = 240.0,
                PostId = TestCatalogIds.Profiles.Posts.Standard,
                PostPeralte = 3.0
            };
            var bay = new SelectiveBay { BeamLength = 100.0, Height = 240.0 };
            foreach (var y in new[] { 48.0, 96.0, 144.0, 192.0 })
            {
                bay.Levels.Add(new SelectiveLevel
                {
                    Y = y,
                    BeamId = TestCatalogIds.Profiles.Beams.SelectiveThreeRivet,
                    BeamPeralte = 4.0
                });
            }

            system.Bays.Add(bay);
            return system;
        }

        private static FlowBedConfiguration FlowBed()
            => new FlowBedConfiguration
            {
                BedType = FlowBedType.Pushback,
                LaneDepth = 100.0,
                PalletDepth = 40.0,
                RollerId = TestCatalogIds.FlowBed.Roller1Point9
            };

        private static RackFrameConfiguration PlantaHeaderConfig(double depth)
        {
            var template = RackFrameTemplateCatalog.FindById(TestCatalogIds.Templates.Standard)
                ?? RackFrameTemplateCatalog.Default;
            return new RackFrameConfigurationFactory(Catalog).Build(
                template,
                TestCatalogIds.Profiles.Posts.Standard,
                132.0,
                depth);
        }

        /// <summary>Order-independent structural fingerprint of a plan (role, piece, world position of every
        /// flattened instance). Two plans with the same fingerprint draw the same pieces in the same places.</summary>
        private static string Signature(DynamicSystemPlan plan)
            => string.Join("|", plan.Flatten().Instances
                .Select(i => FormattableString.Invariant(
                    $"{i.View}:{i.Role}:{i.PieceId}:{i.Insertion.X:0.###}:{i.Insertion.Y:0.###}"))
                .OrderBy(s => s, StringComparer.Ordinal));

        // --- Grouped (ARRAY) vs all-loose plan shapes --------------------------------------------------

        [Fact]
        public void DynamicLateral_IsGrouped_AndFlattenExpandsEveryPlacement()
        {
            var plan = new DynamicSystemLateralBuilder().Build(DynamicSystem(), Catalog);

            Assert.NotEmpty(plan.Headers); // identical headers share ONE nested definition (ARRAY pattern)
            var expanded = plan.Headers.Sum(g => g.Instances.Count * g.Placements.Count) + plan.LooseInstances.Count;
            Assert.Equal(expanded, plan.Flatten().Instances.Count);
        }

        [Fact]
        public void FlowBed_DrawServicePlanShape_IsAllLoose()
        {
            // FlowBedDrawService builds: new DynamicSystemPlan(new List<HeaderGroup>(), builder.Build(config, catalog)).
            var loose = new FlowBedLateralBuilder().Build(FlowBed(), Catalog);
            var plan = new DynamicSystemPlan(new List<HeaderGroup>(), loose);

            Assert.Empty(plan.Headers);
            Assert.NotEmpty(plan.LooseInstances);
            Assert.Equal(loose.Count, plan.Flatten().Instances.Count);
        }

        [Fact]
        public void PlantaHeader_DrawServicePlanShape_IsAllLoose()
        {
            // PlantaHeaderDrawService builds: new DynamicSystemPlan(new List<HeaderGroup>(), builder.Build(config, catalog)).
            var loose = new PlantaHeaderLayoutBuilder().Build(PlantaHeaderConfig(48.0), Catalog);
            var plan = new DynamicSystemPlan(new List<HeaderGroup>(), loose);

            Assert.Empty(plan.Headers);
            Assert.NotEmpty(plan.LooseInstances);
            Assert.Equal(loose.Count, plan.Flatten().Instances.Count);
        }

        [Fact]
        public void AllLoose_And_Grouped_ShapesCoexist_SoTheGenericMustNotUnifyThePlanFactory()
        {
            var cama = new DynamicSystemPlan(new List<HeaderGroup>(), new FlowBedLateralBuilder().Build(FlowBed(), Catalog));
            var lateral = new DynamicSystemLateralBuilder().Build(DynamicSystem(), Catalog);

            Assert.Empty(cama.Headers);        // cama / cabecera planta: everything loose
            Assert.NotEmpty(lateral.Headers);  // dynamic lateral: grouped nested definitions
        }

        // --- postIndex specialization (dynamic lateral) -----------------------------------------------

        [Fact]
        public void DynamicLateral_PostIndexOverload_ProducesADistinctNonEmptyCut()
        {
            var system = DynamicSystem();
            var full = new DynamicSystemLateralBuilder().Build(system, Catalog);
            var singlePost = new DynamicSystemLateralBuilder().Build(system, Catalog, 0);

            Assert.NotEmpty(singlePost.Flatten().Instances);
            // A single-post cut can never draw more pieces than the whole-system layout; dropping the
            // postIndex would revert this call to the full system. (Plugin-side propagation of postIndex is
            // inventory-verified — it lives in code that references AutoCAD.)
            Assert.True(singlePost.Flatten().Instances.Count <= full.Flatten().Instances.Count);
        }

        // --- DynamicRackEnd specialization (dynamic frontal) ------------------------------------------

        [Fact]
        public void DynamicFrontal_EntranceAndExit_ProduceDistinctNonEmptyPlans()
        {
            var system = DynamicSystem();
            var exit = new DynamicSystemFrontalBuilder().BuildPlan(system, Catalog, DynamicRackEnd.Exit);
            var entrance = new DynamicSystemFrontalBuilder().BuildPlan(system, Catalog, DynamicRackEnd.Entrance);

            Assert.NotEmpty(exit.Flatten().Instances);
            Assert.NotEmpty(entrance.Flatten().Instances);
            // Dropping the DynamicRackEnd would draw the wrong frontal cut.
            Assert.NotEqual(Signature(exit), Signature(entrance));
        }

        // --- Per-view distinctness (linked views must remain different plans) --------------------------

        [Fact]
        public void DynamicViews_Lateral_Frontal_Planta_AreThreeDistinctPlans()
        {
            var system = DynamicSystem();
            var lateral = new DynamicSystemLateralBuilder().Build(system, Catalog);
            var frontal = new DynamicSystemFrontalBuilder().BuildPlan(system, Catalog, DynamicRackEnd.Exit);
            var planta = new DynamicSystemPlantaBuilder().BuildPlan(system, Catalog);

            Assert.NotEmpty(planta.Flatten().Instances);
            var signatures = new[] { Signature(lateral), Signature(frontal), Signature(planta) };
            Assert.Equal(3, signatures.Distinct().Count());
        }

        [Fact]
        public void Selective_FrontalAndPlanta_AreDistinctNonEmptyPlans()
        {
            var system = SelectiveSystem();
            var frontal = new SelectiveFrontalBuilder().BuildPlan(system, Catalog);
            var planta = new SelectivePlantaBuilder().BuildPlan(system, Catalog);

            Assert.NotEmpty(frontal.Flatten().Instances);
            Assert.NotEmpty(planta.Flatten().Instances);
            Assert.NotEqual(Signature(frontal), Signature(planta));
        }
    }
}
