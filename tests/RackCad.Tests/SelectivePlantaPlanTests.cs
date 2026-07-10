using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// The selective planta plan groups identical cabecera-planta frames into ONE shared nested definition
    /// placed once per frente (the dynamic system's ARRAY pattern), so a long run appends each distinct frame's
    /// pieces once instead of once per post. Build() must still flatten to the exact same instances the plan
    /// expands to — the drawing cannot change, only how it is structured.
    /// </summary>
    public class SelectivePlantaPlanTests
    {
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static SelectivePalletDesign UniformDesign(int bays)
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId,
                PostPeralte = 3.0,
                PalletTolerance = 4.0,
                VerticalClearance = 6.0,
                PalletDepth = 48.0
            };

            for (var b = 0; b < bays; b++)
            {
                var bay = new SelectiveBayDesign();
                bay.Levels.Add(new SelectiveCell { Pallet = new Tarima { Frente = 40, Alto = 60 }, PalletCount = 2, BeamId = BeamId, BeamPeralte = 4.0 });
                bay.Levels.Add(new SelectiveCell { Pallet = new Tarima { Frente = 40, Alto = 60 }, PalletCount = 2, BeamId = BeamId, BeamPeralte = 4.0 });
                design.Bays.Add(bay);
            }

            return design;
        }

        [Fact]
        public void BuildPlan_UniformRun_OneSharedGroup_PlacedAtEveryFrenteY()
        {
            var system = new SelectiveGeometryResolver().Resolve(UniformDesign(5), Catalog);

            var plan = new SelectivePlantaBuilder().BuildPlan(system, Catalog);

            // 5 identical frentes -> the 6 posts share ONE frame definition, referenced 6 times.
            var group = Assert.Single(plan.Headers);
            Assert.Equal(system.Bays.Count + 1, group.Placements.Count);
            Assert.NotEmpty(group.Instances);

            var frenteYs = SelectivePostGeometry.Compute(system, Catalog).PostXs;
            for (var i = 0; i < frenteYs.Count; i++)
            {
                Assert.Equal(frenteYs[i], group.Placements[i].InsertionY, 6);
                Assert.Equal(0.0, group.Placements[i].InsertionX, 6);
                Assert.False(group.Placements[i].Mirrored);
            }

            // The frame pieces live at the LOCAL origin (Y=0); the placement carries the frente.
            Assert.All(group.Instances.Where(i => i.Role == HeaderBlockRole.Post), p => Assert.Equal(0.0, p.Insertion.Y, 6));
        }

        [Fact]
        public void BuildPlan_CustomPostCabecera_GetsItsOwnGroup()
        {
            var system = new SelectiveGeometryResolver().Resolve(UniformDesign(5), Catalog);

            var template = RackFrameTemplateCatalog.FindStandardOrDefault();
            var custom = new RackFrameConfigurationFactory(Catalog).Build(template, PostId, 300.0, 48.0);
            custom.LeftBasePlate.PeralteOverride = 9.0;
            system.PostCabeceras[0] = custom;

            var plan = new SelectivePlantaBuilder().BuildPlan(system, Catalog);

            // The customized post never shares a definition (editing it must not move its twins): its group has
            // exactly ONE placement at its frente, and the remaining 5 standard posts still share one group.
            Assert.Equal(2, plan.Headers.Count);

            var frenteYs = SelectivePostGeometry.Compute(system, Catalog).PostXs;
            var customGroup = plan.Headers.Single(g => g.Placements.Count == 1);
            Assert.Equal(frenteYs[0], customGroup.Placements[0].InsertionY, 6);
            Assert.Contains(customGroup.Instances, i => i.Role == HeaderBlockRole.BasePlate
                && i.DynamicParameters.TryGetValue("PERALTE", out var v) && Math.Abs(v - 9.0) < 1e-6);

            var sharedGroup = plan.Headers.Single(g => g.Placements.Count != 1);
            Assert.Equal(frenteYs.Count - 1, sharedGroup.Placements.Count);
        }

        [Fact]
        public void BuildPlan_PerPostPeralteOverride_SplitsTheGroup()
        {
            var design = UniformDesign(4);
            for (var p = 0; p <= 4; p++)
            {
                design.PostPeraltes.Add(p == 2 ? 5.0 : 0.0); // one post grows its peralte
            }

            var system = new SelectiveGeometryResolver().Resolve(design, Catalog);
            var plan = new SelectivePlantaBuilder().BuildPlan(system, Catalog);

            // The grown post draws a DIFFERENT frame (peralte is part of the geometry), so it cannot share.
            Assert.Equal(2, plan.Headers.Count);
            var grown = plan.Headers.Single(g => g.Placements.Count == 1);
            Assert.Contains(grown.Instances, i => i.Role == HeaderBlockRole.Post
                && i.DynamicParameters.TryGetValue("PERALTE", out var v) && Math.Abs(v - 5.0) < 1e-6);
        }

        [Fact]
        public void Build_Flattens_ToTheSamePositionsAndCounts_AsTheExpandedPlan()
        {
            var system = new SelectiveGeometryResolver().Resolve(UniformDesign(5), Catalog);
            system.NumberFronts = true;
            system.DrawRackName = true;
            system.Name = "R-01";

            var builder = new SelectivePlantaBuilder();
            var flat = builder.Build(system, Catalog);
            var plan = builder.BuildPlan(system, Catalog);

            // Expand the plan by hand: every group instance translated to each placement's frente Y + the loose.
            var expanded = new List<(HeaderBlockRole Role, string Block, double X, double Y, bool Mirrored)>();
            foreach (var group in plan.Headers)
            {
                foreach (var placement in group.Placements)
                {
                    foreach (var instance in group.Instances)
                    {
                        expanded.Add((instance.Role, instance.BlockName, instance.Insertion.X + placement.InsertionX,
                            instance.Insertion.Y + placement.InsertionY, instance.MirroredX));
                    }
                }
            }

            foreach (var instance in plan.LooseInstances)
            {
                expanded.Add((instance.Role, instance.BlockName, instance.Insertion.X, instance.Insertion.Y, instance.MirroredX));
            }

            Assert.Equal(expanded.Count, flat.Count);

            var actual = flat.Select(i => (i.Role, i.BlockName, i.Insertion.X, i.Insertion.Y, i.MirroredX))
                .OrderBy(t => t.Role).ThenBy(t => t.BlockName).ThenBy(t => t.X).ThenBy(t => t.Y).ToList();
            var expected = expanded
                .OrderBy(t => t.Role).ThenBy(t => t.Block).ThenBy(t => t.X).ThenBy(t => t.Y).ToList();

            for (var i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i].Role, actual[i].Role);
                Assert.Equal(expected[i].Block, actual[i].BlockName);
                Assert.Equal(expected[i].X, actual[i].X, 9);
                Assert.Equal(expected[i].Y, actual[i].Y, 9);
                Assert.Equal(expected[i].Mirrored, actual[i].MirroredX);
            }
        }

        [Fact]
        public void Build_MatchesTheLegacyPerPostConstruction()
        {
            // The regression guard for the grouping: rebuilding every frame directly at its frente (what Build did
            // BEFORE the plan existed) must give the same frame pieces the flattened plan gives.
            var system = new SelectiveGeometryResolver().Resolve(UniformDesign(5), Catalog);

            var frames = new SelectivePlantaBuilder().Build(system, Catalog)
                .Where(i => i.Role == HeaderBlockRole.Post || i.Role == HeaderBlockRole.BasePlate || i.Role == HeaderBlockRole.Horizontal)
                .ToList();

            var frenteYs = SelectivePostGeometry.Compute(system, Catalog).PostXs;
            var factory = new RackFrameConfigurationFactory(Catalog);
            var template = RackFrameTemplateCatalog.FindStandardOrDefault();
            var frameBuilder = new PlantaHeaderLayoutBuilder();
            var depth = SelectiveDepthLayout.CabeceraDepthOfFondo(system, 0); // the planta draws the frame at tarima − 6

            var legacy = new List<HeaderBlockInstance>();
            for (var i = 0; i < frenteYs.Count; i++)
            {
                var height = SelectivePostGeometry.PostHeight(system, i);
                var cabecera = factory.Build(template, system.PostId, height > 0.0 ? height : system.Height, depth);
                legacy.AddRange(frameBuilder.Build(cabecera, Catalog, new Point2D(0.0, frenteYs[i]), SelectivePostGeometry.PostPeralteAt(system, i)));
            }

            Assert.Equal(legacy.Count, frames.Count);
            for (var i = 0; i < legacy.Count; i++)
            {
                Assert.Equal(legacy[i].Role, frames[i].Role);
                Assert.Equal(legacy[i].BlockName, frames[i].BlockName);
                Assert.Equal(legacy[i].MirroredX, frames[i].MirroredX);
                Assert.Equal(legacy[i].Insertion.X, frames[i].Insertion.X, 9);
                Assert.Equal(legacy[i].Insertion.Y, frames[i].Insertion.Y, 9);
                Assert.Equal(legacy[i].ConnectionAnchor.Y, frames[i].ConnectionAnchor.Y, 9);
                Assert.Equal(legacy[i].DynamicParameters.OrderBy(p => p.Key), frames[i].DynamicParameters.OrderBy(p => p.Key));
            }
        }

        [Fact]
        public void BuildPlan_DrawBasePlateOff_OmitsThePlatesInsideTheGroups()
        {
            var system = new SelectiveGeometryResolver().Resolve(UniformDesign(3), Catalog);
            system.DrawBasePlate = false;

            var plan = new SelectivePlantaBuilder().BuildPlan(system, Catalog);

            Assert.All(plan.Headers.SelectMany(g => g.Instances), i => Assert.NotEqual(HeaderBlockRole.BasePlate, i.Role));
        }
    }
}
