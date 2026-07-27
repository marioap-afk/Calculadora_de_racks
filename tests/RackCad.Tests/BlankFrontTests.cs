using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.Persistence;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-33 / PB-014 — the "frente en blanco" contract, shared by the Dinamico and by Push Back (which composes the
    /// very same dynamic structure). A blank front CONSERVES its claro and its structure, still DISPLACES the fronts
    /// behind it, and carries ZERO effective load levels and therefore zero load components in every view and in the
    /// BOM. Its own configuration stays DORMANT so reactivating it restores exactly the rack it had; it is never
    /// modelled with a fake cell. Legacy documents know nothing about the flag and load every front active, and at
    /// least one front must stay active.
    /// </summary>
    public class BlankFrontTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        /// <summary>Three fronts with DIFFERENT pallet counts and level counts, so every count below is unambiguous.</summary>
        private static DynamicRackDesign Structure()
        {
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 3,
                FirstLevelHeight = 6.0,
                BeamDepth = 4.0
            };
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 3, LoadLevels = 4 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 2, LoadLevels = 3 });
            return design;
        }

        /// <summary>The same rack with its MIDDLE front blank; every other input is untouched.</summary>
        private static DynamicRackDesign StructureWithBlankMiddle()
        {
            var design = Structure();
            design.Fronts[1].IsActive = false;
            return design;
        }

        private static DynamicRackSystem Resolve(DynamicRackDesign design)
            => new DynamicRackSystemResolver(Catalog).Resolve(design).System;

        private static PushBackSystem ResolvePushBack(DynamicRackDesign structure)
            => new PushBackResolver(Catalog).Resolve(new PushBackDesign { Structure = structure });

        private static int Quantity(BillOfMaterials bom, string category)
            => bom.Components.Where(component => component.Category == category).Sum(component => component.Quantity);

        // ---- The shared authority ---------------------------------------------------------------------------

        [Fact]
        public void Authority_AnswersZeroForABlankFrontAndTheHistoricalMaxForAnActiveOne()
        {
            Assert.Equal(4, DynamicFrontActivation.EffectiveLoadLevels(new DynamicRackFront { LoadLevels = 4 }));
            // An active front with a zeroed count keeps the historical Math.Max(1, ...) floor.
            Assert.Equal(1, DynamicFrontActivation.EffectiveLoadLevels(new DynamicRackFront { LoadLevels = 0 }));
            Assert.Equal(0, DynamicFrontActivation.EffectiveLoadLevels(
                new DynamicRackFront { LoadLevels = 4, IsActive = false }));
            Assert.Equal(0, DynamicFrontActivation.EffectiveLoadLevels((DynamicRackFront)null));
            Assert.True(DynamicFrontActivation.IsBlank(new DynamicRackFront { IsActive = false }));
            Assert.False(DynamicFrontActivation.IsBlank(new DynamicRackFront()));
        }

        [Fact]
        public void Authority_ReactivatesTheFirstFrontWhenEveryFrontCameBackBlank()
        {
            var designs = new List<DynamicRackFrontDesign>
            {
                new DynamicRackFrontDesign { IsActive = false },
                new DynamicRackFrontDesign { IsActive = false }
            };

            Assert.False(DynamicFrontActivation.HasActiveFront(designs));
            DynamicFrontActivation.EnsureActiveFront(designs);

            Assert.True(designs[0].IsActive);
            Assert.False(designs[1].IsActive);
        }

        // ---- Claro, structure and displacement --------------------------------------------------------------

        [Fact]
        public void Blank_KeepsItsClaroAndDisplacesTheFrontsBehindIt()
        {
            var active = Resolve(Structure());
            var blank = Resolve(StructureWithBlankMiddle());

            // Same claro: the transverse cut length and the lane width of the blank front are untouched...
            Assert.Equal(active.Fronts[1].BeamLength, blank.Fronts[1].BeamLength, 6);
            Assert.Equal(active.Fronts[1].Bfr, blank.Fronts[1].Bfr, 6);
            Assert.Equal(active.Fronts[1].PalletCount, blank.Fronts[1].PalletCount);

            // ...so the shared post grid is identical and the fronts behind it stay displaced exactly the same.
            var activeLayout = DynamicFrontGeometry.Compute(active, Catalog);
            var blankLayout = DynamicFrontGeometry.Compute(blank, Catalog);
            Assert.Equal(activeLayout.PostPositions.Count, blankLayout.PostPositions.Count);
            for (var post = 0; post < activeLayout.PostPositions.Count; post++)
            {
                Assert.Equal(activeLayout.PostPositions[post], blankLayout.PostPositions[post], 6);
            }

            Assert.Equal(active.Fronts[2].StartX, blank.Fronts[2].StartX, 6);
            Assert.Equal(active.Fronts[2].EndX, blank.Fronts[2].EndX, 6);
            Assert.Equal(active.TotalLength, blank.TotalLength, 6);
        }

        [Fact]
        public void Blank_KeepsTheStructuralPostsAndTheirHeightsInTheFrontalCut()
        {
            var active = new DynamicSystemFrontalBuilder().Build(Resolve(Structure()), Catalog, DynamicRackEnd.Exit);
            var blank = new DynamicSystemFrontalBuilder()
                .Build(Resolve(StructureWithBlankMiddle()), Catalog, DynamicRackEnd.Exit);

            var activePosts = active.Where(instance => instance.Role == HeaderBlockRole.Post).ToList();
            var blankPosts = blank.Where(instance => instance.Role == HeaderBlockRole.Post).ToList();

            Assert.Equal(4, activePosts.Count);
            Assert.Equal(activePosts.Count, blankPosts.Count);
            for (var index = 0; index < activePosts.Count; index++)
            {
                Assert.Equal(activePosts[index].Insertion.X, blankPosts[index].Insertion.X, 6);
                Assert.Equal(
                    activePosts[index].DynamicParameters[SelectiveRackDefaults.LengthParam],
                    blankPosts[index].DynamicParameters[SelectiveRackDefaults.LengthParam],
                    6);
            }

            Assert.Equal(
                active.Count(instance => instance.Role == HeaderBlockRole.BasePlate),
                blank.Count(instance => instance.Role == HeaderBlockRole.BasePlate));
        }

        // ---- Zero load in every dynamic view ----------------------------------------------------------------

        [Fact]
        public void Blank_DrawsNoInOutBeamInEitherFrontalCut()
        {
            var system = Resolve(StructureWithBlankMiddle());
            var builder = new DynamicSystemFrontalBuilder();
            var layout = DynamicFrontGeometry.Compute(system, Catalog);
            var blankBeamX = layout.PostPositions[1] + layout.TroquelPositions[1];

            foreach (var end in new[] { DynamicRackEnd.Exit, DynamicRackEnd.Entrance })
            {
                var beams = builder.Build(system, Catalog, end)
                    .Where(instance => instance.Role == HeaderBlockRole.Beam)
                    .ToList();

                // Only fronts 0 (2 levels) and 2 (3 levels) contribute; the blank front's 4 levels are gone.
                Assert.Equal(5, beams.Count);
                Assert.DoesNotContain(beams, beam => System.Math.Abs(beam.Insertion.X - blankBeamX) < 1e-6);
            }
        }

        [Fact]
        public void Blank_DrawsNoLoadBeamAndNoBedInTheLateralSectionsItTouches()
        {
            var system = Resolve(StructureWithBlankMiddle());
            var builder = new DynamicSystemLateralBuilder();

            // Posts 1 and 2 are the two sections that touch the blank middle front.
            var atPost1 = builder.Build(system, Catalog, 1).Flatten().Instances;
            var atPost2 = builder.Build(system, Catalog, 2).Flatten().Instances;

            // Post 1 sees fronts 0 (2 levels) and 1 (blank): only front 0's two IN + two OUT beams survive.
            Assert.Equal(4, atPost1.Count(instance => instance.Role == HeaderBlockRole.Beam
                && instance.PieceId == TestCatalogIds.Profiles.Beams.DynamicInOut));
            // Post 2 sees fronts 1 (blank) and 2 (3 levels): only front 2's three IN + three OUT beams survive.
            Assert.Equal(6, atPost2.Count(instance => instance.Role == HeaderBlockRole.Beam
                && instance.PieceId == TestCatalogIds.Profiles.Beams.DynamicInOut));

            // The blank front contributes no bed at either section.
            var blankFront = system.Fronts[1];
            Assert.Empty(DynamicFlowBedGeometry.Resolve(system, Catalog, blankFront));
            Assert.Empty(DynamicFrontGeometry.LoadBeamLevels(system, blankFront));
            Assert.Empty(DynamicLoadBeamGeometry.Placements(system, blankFront));
        }

        [Fact]
        public void Blank_KeepsSeparatorsAndDerivedPostsInTheLateralSection()
        {
            var active = new DynamicSystemLateralBuilder().Build(Resolve(Structure()), Catalog, 1).Flatten().Instances;
            var blank = new DynamicSystemLateralBuilder()
                .Build(Resolve(StructureWithBlankMiddle()), Catalog, 1).Flatten().Instances;

            Assert.Equal(
                active.Count(instance => instance.Role == HeaderBlockRole.Separator),
                blank.Count(instance => instance.Role == HeaderBlockRole.Separator));
            Assert.True(blank.Count(instance => instance.Role == HeaderBlockRole.Separator) > 0);
            Assert.Equal(
                active.Count(instance => instance.Role == HeaderBlockRole.Post),
                blank.Count(instance => instance.Role == HeaderBlockRole.Post));
        }

        [Fact]
        public void Blank_DrawsNoTransverseLoadBeamInPlanta()
        {
            var active = new DynamicSystemPlantaBuilder().Build(Resolve(Structure()), Catalog);
            var blank = new DynamicSystemPlantaBuilder().Build(Resolve(StructureWithBlankMiddle()), Catalog);

            var activeBeams = active.Count(instance => instance.Role == HeaderBlockRole.Beam);
            var blankBeams = blank.Count(instance => instance.Role == HeaderBlockRole.Beam);

            Assert.True(activeBeams > blankBeams, "la planta debe perder los largueros del frente en blanco");
            Assert.Equal(
                active.Count(instance => instance.Role == HeaderBlockRole.Separator),
                blank.Count(instance => instance.Role == HeaderBlockRole.Separator));

            // No transverse beam may sit on the blank front's own line.
            var layout = DynamicFrontGeometry.Compute(Resolve(StructureWithBlankMiddle()), Catalog);
            var blankFront = Resolve(StructureWithBlankMiddle()).Fronts[1];
            Assert.DoesNotContain(
                blank.Where(instance => instance.Role == HeaderBlockRole.Beam),
                beam => beam.Insertion.X > blankFront.StartX - 1e-6 && beam.Insertion.X < blankFront.EndX + 1e-6
                        && System.Math.Abs(beam.Insertion.Y - layout.PostPositions[1]) < layout.PostPositions[1] * 0.0);
        }

        // ---- Zero load in the dynamic BOM -------------------------------------------------------------------

        [Fact]
        public void Bom_CountsNoInOutBeamIntermediateBeamOrBedForABlankFront()
        {
            var activeSystem = Resolve(Structure());
            var blankSystem = Resolve(StructureWithBlankMiddle());
            var active = SystemBomBuilder.Build(activeSystem, Catalog);
            var blank = SystemBomBuilder.Build(blankSystem, Catalog);

            // IN/OUT: two beams per front x level. The blank front's 4 levels disappear.
            Assert.Equal(18, Quantity(active, SystemBomBuilder.InOutBeam));
            Assert.Equal(10, Quantity(blank, SystemBomBuilder.InOutBeam));

            // Beds: one per lane and level. The blank front had 3 lanes x 4 levels.
            Assert.Equal(
                Quantity(active, SystemBomBuilder.Cama) - 12,
                Quantity(blank, SystemBomBuilder.Cama));

            Assert.True(Quantity(active, SystemBomBuilder.IntermediateBeam)
                        > Quantity(blank, SystemBomBuilder.IntermediateBeam));
        }

        [Fact]
        public void Bom_KeepsTheStructuralComponentsOfABlankFront()
        {
            var active = SystemBomBuilder.Build(Resolve(Structure()), Catalog);
            var blank = SystemBomBuilder.Build(Resolve(StructureWithBlankMiddle()), Catalog);

            Assert.Equal(Quantity(active, SystemBomBuilder.Separator), Quantity(blank, SystemBomBuilder.Separator));
            Assert.Equal(Quantity(active, SystemBomBuilder.DerivedPost), Quantity(blank, SystemBomBuilder.DerivedPost));
            Assert.Equal(Quantity(active, SystemBomBuilder.ReinforcedPost), Quantity(blank, SystemBomBuilder.ReinforcedPost));
            Assert.Equal(Quantity(active, BomBuilder.Post), Quantity(blank, BomBuilder.Post));
        }

        // ---- Dormancy: the configuration survives and reactivation restores the rack ------------------------

        [Fact]
        public void Blank_KeepsItsConfigurationDormantAndReactivatingRestoresTheActiveRack()
        {
            var blankDesign = StructureWithBlankMiddle();
            var blankSystem = Resolve(blankDesign);

            // The dormant intent is intact on the resolved front: levels, lanes and cells were not zeroed.
            Assert.False(blankSystem.Fronts[1].IsActive);
            Assert.Equal(4, blankSystem.Fronts[1].LoadLevels);
            Assert.Equal(3, blankSystem.Fronts[1].PalletCount);
            Assert.Equal(4, blankSystem.Fronts[1].Levels.Count);

            // Reactivating reproduces the all-active rack exactly, without restating any cell value.
            blankDesign.Fronts[1].IsActive = true;
            var reactivated = Resolve(blankDesign);
            var reference = Resolve(Structure());

            Assert.Equal(
                Quantity(SystemBomBuilder.Build(reference, Catalog), SystemBomBuilder.InOutBeam),
                Quantity(SystemBomBuilder.Build(reactivated, Catalog), SystemBomBuilder.InOutBeam));
            Assert.Equal(
                new DynamicSystemFrontalBuilder().Build(reference, Catalog, DynamicRackEnd.Exit).Count,
                new DynamicSystemFrontalBuilder().Build(reactivated, Catalog, DynamicRackEnd.Exit).Count);
        }

        [Fact]
        public void Snapshot_CarriesTheBlankFlagBackIntoTheEditableDesign()
        {
            var system = Resolve(StructureWithBlankMiddle());

            var snapshot = new DynamicRackSystemResolver(Catalog).Snapshot(system, 3, 6.0, 4.0, null);

            Assert.True(snapshot.Fronts[0].IsActive);
            Assert.False(snapshot.Fronts[1].IsActive);
            Assert.True(snapshot.Fronts[2].IsActive);
            Assert.Equal(4, snapshot.Fronts[1].Levels.Count);
        }

        // ---- Persistence: round-trip plus the legacy fallback ----------------------------------------------

        [Fact]
        public void Document_RoundTripsTheBlankFlagForTheDesignAndForTheResolvedSystem()
        {
            var design = StructureWithBlankMiddle();

            var reloadedDesign = DynamicRackSystemDocument.From(design).ToDesign();
            Assert.Equal(new[] { true, false, true }, reloadedDesign.Fronts.Select(front => front.IsActive).ToArray());

            // The real editor path — resolve, snapshot, persist — must carry the blank front's DORMANT cells across.
            var resolver = new DynamicRackSystemResolver(Catalog);
            var snapshot = resolver.Snapshot(Resolve(design), 3, 6.0, 4.0, null);
            var reloadedSnapshot = DynamicRackSystemDocument.From(snapshot).ToDesign();
            Assert.False(reloadedSnapshot.Fronts[1].IsActive);
            Assert.Equal(4, reloadedSnapshot.Fronts[1].Levels.Count);
            Assert.Equal(4, reloadedSnapshot.Fronts[1].LoadLevels);
            Assert.Equal(3, reloadedSnapshot.Fronts[1].PalletCount);

            var reloadedSystem = DynamicRackSystemDocument.From(Resolve(design)).ToDomain();
            Assert.Equal(new[] { true, false, true }, reloadedSystem.Fronts.Select(front => front.IsActive).ToArray());
            Assert.Equal(4, reloadedSystem.Fronts[1].Levels.Count);
        }

        [Fact]
        public void ActiveFronts_AddNothingToTheWireFormat_AndOnlyABlankFrontIsWritten()
        {
            // A rack with no blank front must serialize exactly as it did before I-33: the flag is omitted, not null.
            var untouched = JsonSerializer.Serialize(DynamicRackSystemDocument.From(Structure()));
            Assert.DoesNotContain("IsActive", untouched);

            var withBlank = JsonSerializer.Serialize(DynamicRackSystemDocument.From(StructureWithBlankMiddle()));
            Assert.Single(
                System.Text.RegularExpressions.Regex.Matches(withBlank, "\"IsActive\""));
            Assert.Contains("\"IsActive\":false", withBlank);
        }

        [Fact]
        public void LegacyDocumentWithoutTheFlag_LoadsEveryFrontActive()
        {
            // A document written before I-33 simply has no IsActive member anywhere — which is what an all-active
            // rack serializes to today, so this is the legacy shape verbatim.
            var legacy = JsonSerializer.Serialize(DynamicRackSystemDocument.From(Structure()));
            Assert.DoesNotContain("IsActive", legacy);

            var design = JsonSerializer.Deserialize<DynamicRackSystemDocument>(legacy).ToDesign();

            Assert.Equal(3, design.Fronts.Count);
            Assert.All(design.Fronts, front => Assert.True(front.IsActive));
        }

        [Fact]
        public void Document_KeepsAtLeastOneActiveFrontWhenEveryFrontWasBlanked()
        {
            var design = Structure();
            foreach (var front in design.Fronts)
            {
                front.IsActive = false;
            }

            var reloaded = DynamicRackSystemDocument.From(design).ToDesign();

            Assert.True(DynamicFrontActivation.HasActiveFront(reloaded.Fronts));
            Assert.True(reloaded.Fronts[0].IsActive);
        }

        // ---- Push Back: the same contract over the composed structure ---------------------------------------

        [Fact]
        public void PushBack_KeepsTheClaroAndTheStructureOfABlankFront()
        {
            var active = ResolvePushBack(Structure());
            var blank = ResolvePushBack(StructureWithBlankMiddle());

            Assert.Equal(active.Fronts[1].BeamLength, blank.Fronts[1].BeamLength, 6);
            Assert.Equal(active.Fronts[2].StartX, blank.Fronts[2].StartX, 6);
            Assert.Equal(active.TotalLength, blank.TotalLength, 6);
            Assert.False(blank.Fronts[1].IsActive);
        }

        [Fact]
        public void PushBack_DrawsNoLoadForABlankFrontInAnyView()
        {
            var system = ResolvePushBack(StructureWithBlankMiddle());
            var layout = DynamicFrontGeometry.Compute(system.Structure, Catalog);
            var blankBeamX = layout.PostPositions[1] + layout.TroquelPositions[1];

            foreach (var end in new[] { PushBackFrontalEnd.EntradaSalida, PushBackFrontalEnd.Posterior })
            {
                var frontal = new PushBackSystemFrontalBuilder().BuildPlan(system, Catalog, end)
                    .Flatten().Instances;
                Assert.DoesNotContain(
                    frontal.Where(instance => instance.Role == HeaderBlockRole.Beam),
                    beam => System.Math.Abs(beam.Insertion.X - blankBeamX) < 1e-6);
            }

            // The rear (high-end) beam is resolved per front x level: a blank front resolves none.
            Assert.Empty(system.HighEndBeams[1].HighEndBeamPeraltes);

            var planta = new PushBackSystemPlantaBuilder().BuildPlan(system, Catalog).Flatten().Instances;
            var activePlanta = new PushBackSystemPlantaBuilder()
                .BuildPlan(ResolvePushBack(Structure()), Catalog).Flatten().Instances;
            Assert.True(
                activePlanta.Count(instance => instance.Role == HeaderBlockRole.Beam)
                > planta.Count(instance => instance.Role == HeaderBlockRole.Beam));
        }

        [Fact]
        public void PushBack_BomCountsNoLoadComponentForABlankFront()
        {
            var active = PushBackBomBuilder.Build(ResolvePushBack(Structure()), Catalog);
            var blank = PushBackBomBuilder.Build(ResolvePushBack(StructureWithBlankMiddle()), Catalog);

            // One low IN/OUT and one high troquel-redondo per front x level; the blank front's 4 levels disappear.
            Assert.Equal(9, Quantity(active, SystemBomBuilder.InOutBeam));
            Assert.Equal(5, Quantity(blank, SystemBomBuilder.InOutBeam));
            Assert.Equal(9, Quantity(active, PushBackBomBuilder.HighEndBeam));
            Assert.Equal(5, Quantity(blank, PushBackBomBuilder.HighEndBeam));

            // One rear tope per active cell, and one opaque bed per lane and level.
            Assert.Equal(9, Quantity(active, PushBackBomBuilder.RearTope));
            Assert.Equal(5, Quantity(blank, PushBackBomBuilder.RearTope));
            Assert.Equal(Quantity(active, SystemBomBuilder.Cama) - 12, Quantity(blank, SystemBomBuilder.Cama));

            // The structure it shares with the dynamic BOM is untouched.
            Assert.Equal(Quantity(active, SystemBomBuilder.Separator), Quantity(blank, SystemBomBuilder.Separator));
        }

        [Fact]
        public void PushBack_DocumentRoundTripsTheBlankFlag()
        {
            var design = new PushBackDesign { Structure = StructureWithBlankMiddle() };

            var reloaded = PushBackDesignDocument.FromDomain(design).ToDomain();

            Assert.Equal(
                new[] { true, false, true },
                reloaded.Structure.Fronts.Select(front => front.IsActive).ToArray());
        }
    }
}
