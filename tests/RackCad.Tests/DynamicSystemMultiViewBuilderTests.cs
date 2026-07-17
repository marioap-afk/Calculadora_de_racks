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
    public class DynamicSystemMultiViewBuilderTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static DynamicRackSystem System()
        {
            var design = DynamicFrontGeometryTests.Design();
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 3 });
            return new DynamicRackSystemResolver(Catalog).Resolve(design).System;
        }

        [Fact]
        public void FrontalCuts_DrawOnlyInOutBeamsOnTheSharedFrontGrid()
        {
            var system = System();
            var builder = new DynamicSystemFrontalBuilder();

            var exit = builder.Build(system, Catalog, DynamicRackEnd.Exit);
            var entrance = builder.Build(system, Catalog, DynamicRackEnd.Entrance);
            var exitBeams = exit.Where(instance => instance.Role == HeaderBlockRole.Beam).ToList();
            var entranceBeams = entrance.Where(instance => instance.Role == HeaderBlockRole.Beam).ToList();

            Assert.Equal(system.Fronts.Count + 1, exit.Count(instance => instance.Role == HeaderBlockRole.Post));
            Assert.Equal(system.Fronts.Count * system.LoadBeamLevels.Count, exitBeams.Count);
            Assert.Equal(exitBeams.Count, entranceBeams.Count);
            Assert.All(exitBeams.Concat(entranceBeams), beam =>
                Assert.Equal(DynamicRackDefaults.InOutBeamCatalogId, beam.PieceId));
            Assert.Equal(system.LoadBeamLevels[0].ExitElevation, exitBeams.Min(beam => beam.Insertion.Y), 4);
            Assert.Equal(system.LoadBeamLevels[0].EntranceElevation, entranceBeams.Min(beam => beam.Insertion.Y), 4);
            Assert.DoesNotContain(exit, instance => instance.Role == HeaderBlockRole.Rail
                                                     || instance.Role == HeaderBlockRole.Roller
                                                     || instance.Role == HeaderBlockRole.Brake);
        }

        [Fact]
        public void Frontal_RespectsTheLevelCountOfEachFront()
        {
            var design = DynamicFrontGeometryTests.Design();
            design.LoadLevels = 4;
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 2, LoadLevels = 4 });
            var system = new DynamicRackSystemResolver(Catalog).Resolve(design).System;

            var beams = new DynamicSystemFrontalBuilder()
                .Build(system, Catalog, DynamicRackEnd.Exit)
                .Where(instance => instance.Role == HeaderBlockRole.Beam)
                .ToList();

            Assert.Equal(6, beams.Count);
        }

        [Fact]
        public void Frontal_PostHeight_IsTheTallestAdjacentFront_NotTheSystemMaximum()
        {
            var design = DynamicFrontGeometryTests.Design();
            design.LoadLevels = 3;
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 3 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 5 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 3 });
            var system = new DynamicRackSystemResolver(Catalog).Resolve(design).System;
            var totalDepth = design.PalletsDeep * design.Pallet.Depth
                             + 2.0 * DynamicRackDefaults.HeaderEndAllowance;
            var shortHeight = DynamicHeaderHeightCalculator.Calculate(
                design.Pallet.Height,
                3,
                design.FirstLevelHeight,
                system.InOutBeamDepth,
                totalDepth).HeaderHeight;
            var tallHeight = DynamicHeaderHeightCalculator.Calculate(
                design.Pallet.Height,
                5,
                design.FirstLevelHeight,
                system.InOutBeamDepth,
                totalDepth).HeaderHeight;

            var postHeights = new DynamicSystemFrontalBuilder()
                .Build(system, Catalog, DynamicRackEnd.Exit)
                .Where(instance => instance.Role == HeaderBlockRole.Post)
                .OrderBy(instance => instance.Insertion.X)
                .Select(instance => instance.DynamicParameters[SelectiveRackDefaults.LengthParam])
                .ToList();

            Assert.Equal(new[] { shortHeight, tallHeight, tallHeight, shortHeight }, postHeights);
        }

        [Fact]
        public void Planta_RepeatsStructureAcrossFrontsAndNeverDrawsBeds()
        {
            var system = System();
            var instances = new DynamicSystemPlantaBuilder().Build(system, Catalog);
            var supports = DynamicIntermediateBeamGeometry.Supports(
                system,
                CatalogLookup.Local(Catalog, DynamicFrontGeometry.PostId(system, Catalog), "FIN_POSTE", "LATERAL"));

            Assert.All(instances, instance => Assert.Equal("PLANTA", instance.View));
            Assert.DoesNotContain(instances, instance => instance.Role == HeaderBlockRole.Rail
                                                         || instance.Role == HeaderBlockRole.Roller
                                                         || instance.Role == HeaderBlockRole.Brake
                                                         || instance.Role == HeaderBlockRole.Stop);
            Assert.Equal(system.Fronts.Count * 2, instances.Count(instance =>
                instance.PieceId == DynamicRackDefaults.InOutBeamCatalogId));
            Assert.Equal(system.Fronts.Count * supports.Count, instances.Count(instance =>
                instance.PieceId == DynamicRackDefaults.IntermediateBeamCatalogId));
            Assert.True(instances.Count(instance => instance.Role == HeaderBlockRole.Post)
                        >= system.Fronts.Count + 1);
        }

        [Fact]
        public void Planta_AppliesTheLargestConfiguredIntermediateBeamPeralteOfEachFront()
        {
            var design = DynamicFrontGeometryTests.Design();
            var firstFront = new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 3 };
            firstFront.IntermediateBeamDepths.Add(3.0);
            firstFront.IntermediateBeamDepths.Add(4.0);
            firstFront.IntermediateBeamDepths.Add(5.0);
            design.Fronts.Add(firstFront);
            var secondFront = new DynamicRackFrontDesign { PalletCount = 2, LoadLevels = 3 };
            secondFront.IntermediateBeamDepths.Add(3.5);
            secondFront.IntermediateBeamDepths.Add(4.5);
            secondFront.IntermediateBeamDepths.Add(6.0);
            design.Fronts.Add(secondFront);
            var system = new DynamicRackSystemResolver(Catalog).Resolve(design).System;
            var layout = DynamicFrontGeometry.Compute(system, Catalog);
            var postId = DynamicFrontGeometry.PostId(system, Catalog);
            var postPeralte = DynamicFrontGeometry.PostPeralte(system, Catalog, postId);
            var troquelEntry = Catalog.ConnectionLayout.FindConnectionLayout(
                postId,
                SelectiveRackDefaults.PostBeamPoint,
                "PLANTA");
            var troquel = SelectivePostGeometry.Resolve(troquelEntry, new Dictionary<string, double>
            {
                [SelectiveRackDefaults.PeralteParam] = postPeralte
            });

            var beams = new DynamicSystemPlantaBuilder().Build(system, Catalog)
                .Where(instance => instance.PieceId == DynamicRackDefaults.IntermediateBeamCatalogId)
                .ToList();

            var firstY = layout.PostPositions[0] + troquel.Y;
            var secondY = layout.PostPositions[1] + troquel.Y;
            var firstBeams = beams.Where(beam => global::System.Math.Abs(beam.Insertion.Y - firstY) < 1e-4).ToList();
            var secondBeams = beams.Where(beam => global::System.Math.Abs(beam.Insertion.Y - secondY) < 1e-4).ToList();
            Assert.NotEmpty(firstBeams);
            Assert.NotEmpty(secondBeams);
            Assert.All(firstBeams,
                beam => Assert.Equal(5.0, beam.DynamicParameters[SelectiveRackDefaults.PeralteParam]));
            Assert.All(secondBeams,
                beam => Assert.Equal(6.0, beam.DynamicParameters[SelectiveRackDefaults.PeralteParam]));
        }

        [Fact]
        public void Planta_DerivedReinforcementContinuesAfterThePrimaryOnTheSameFrontLine()
        {
            var system = System();
            var instances = new DynamicSystemPlantaBuilder().Build(system, Catalog);
            var layout = DynamicFrontGeometry.Compute(system, Catalog);
            var postId = DynamicFrontGeometry.PostId(system, Catalog);
            var peralte = DynamicFrontGeometry.PostPeralte(system, Catalog, postId);
            var boundary = Assert.Single(system.GetDerivedPostOffsets());
            var finPoste = CatalogLookup.Local(Catalog, postId, "FIN_POSTE", "LATERAL");
            var primaryX = boundary - finPoste.X;
            var reinforcementX = boundary;
            var posts = instances.Where(instance => instance.Role == HeaderBlockRole.Post
                                                     && instance.PieceId == postId
                                                     && (global::System.Math.Abs(instance.ConnectionAnchor.X - primaryX) < 1e-4
                                                         || global::System.Math.Abs(instance.ConnectionAnchor.X - reinforcementX) < 1e-4))
                .ToList();
            var plates = instances.Where(instance => instance.Role == HeaderBlockRole.BasePlate
                                                      && (global::System.Math.Abs(instance.ConnectionAnchor.X - primaryX) < 1e-4
                                                          || global::System.Math.Abs(instance.ConnectionAnchor.X - reinforcementX) < 1e-4))
                .ToList();

            Assert.Equal(layout.PostPositions.Count * 2, posts.Count);
            foreach (var y in layout.PostPositions)
            {
                Assert.Contains(posts, post => global::System.Math.Abs(post.ConnectionAnchor.Y - y) < 1e-4
                                               && !post.MirroredX
                                               && !post.MirroredY);
                Assert.Contains(posts, post => global::System.Math.Abs(post.ConnectionAnchor.X - reinforcementX) < 1e-4
                                               && global::System.Math.Abs(post.ConnectionAnchor.Y - y) < 1e-4
                                               && !post.MirroredX
                                               && !post.MirroredY);
                Assert.Contains(plates, plate => global::System.Math.Abs(plate.ConnectionAnchor.X - reinforcementX) < 1e-4
                                                 && global::System.Math.Abs(plate.ConnectionAnchor.Y - y) < 1e-4
                                                 && !plate.MirroredX
                                                 && !plate.MirroredY);
            }
        }

        [Fact]
        public void FrontalAndPlanta_ProjectBootsAndDesviadoresFromTheSharedSafetySelection()
        {
            var system = System();
            system.SafetySelections.Add(new SelectiveSafetySelection
            {
                ElementId = "PROTECTOR_BOTA_C_6",
                Side = SafetySide.Both,
                Quantity = 1
            });
            system.SafetySelections.Add(new SelectiveSafetySelection
            {
                ElementId = "DESVIADOR_A_3",
                Side = SafetySide.Both,
                Quantity = 1,
                DesviadorLongitud = 18.0,
                DesviadorPrimerNivelAltura = 18.0
            });

            var frontal = new DynamicSystemFrontalBuilder().Build(system, Catalog, DynamicRackEnd.Exit);
            var planta = new DynamicSystemPlantaBuilder().Build(system, Catalog);

            Assert.Contains(frontal, instance => instance.Role == HeaderBlockRole.Safety
                                                 && instance.PieceId == "PROTECTOR_BOTA_C_6");
            Assert.Contains(frontal, instance => instance.Role == HeaderBlockRole.Safety
                                                 && instance.PieceId == "DESVIADOR_A_3");
            Assert.Contains(planta, instance => instance.Role == HeaderBlockRole.Safety
                                                && instance.PieceId == "PROTECTOR_BOTA_C_6");
            Assert.Contains(planta, instance => instance.Role == HeaderBlockRole.Safety
                                                && instance.PieceId == "DESVIADOR_A_3");
        }

        [Fact]
        public void FrontalEntrance_KeepsTheAuthoredDesviadorOrientationWithoutMirroring()
        {
            var system = System();
            system.SafetySelections.Add(new SelectiveSafetySelection
            {
                ElementId = "DESVIADOR_A_3",
                Side = SafetySide.Both,
                Quantity = 1,
                DesviadorLongitud = 18.0,
                DesviadorPrimerNivelAltura = 18.0
            });

            var exit = new DynamicSystemFrontalBuilder().Build(system, Catalog, DynamicRackEnd.Exit)
                .Where(instance => instance.PieceId == "DESVIADOR_A_3")
                .ToList();
            var entrance = new DynamicSystemFrontalBuilder().Build(system, Catalog, DynamicRackEnd.Entrance)
                .Where(instance => instance.PieceId == "DESVIADOR_A_3")
                .ToList();

            Assert.NotEmpty(exit);
            Assert.Equal(exit.Count, entrance.Count);
            Assert.All(exit, instance => Assert.False(instance.MirroredX));
            Assert.All(entrance, instance => Assert.False(instance.MirroredX));
        }

        [Fact]
        public void FrontalAndPlanta_ProjectOneForkliftDefenseAtEachEndOfEveryEnabledPost()
        {
            var system = System(); // two fronts -> three transverse post lines
            system.SafetySelections.Add(new SelectiveSafetySelection
            {
                ElementId = "DEFENSA_MONTACARGAS",
                Quantity = 1,
                Side = SafetySide.None
            });

            var exit = new DynamicSystemFrontalBuilder().Build(system, Catalog, DynamicRackEnd.Exit)
                .Where(instance => instance.PieceId == "DEFENSA_MONTACARGAS").ToList();
            var entrance = new DynamicSystemFrontalBuilder().Build(system, Catalog, DynamicRackEnd.Entrance)
                .Where(instance => instance.PieceId == "DEFENSA_MONTACARGAS").ToList();
            var planta = new DynamicSystemPlantaBuilder().Build(system, Catalog)
                .Where(instance => instance.PieceId == "DEFENSA_MONTACARGAS").ToList();

            Assert.Equal(3, exit.Count);
            Assert.Equal(3, entrance.Count);
            Assert.Equal(6, planta.Count);
            Assert.Equal(new[] { 12.0, 12.0, 12.0, 12.0, 36.0, 36.0 },
                planta.Select(instance => instance.DynamicParameters[SelectiveRackDefaults.LengthParam]).OrderBy(value => value));
            Assert.All(exit, instance => Assert.False(instance.MirroredX));
            Assert.All(entrance, instance => Assert.True(instance.MirroredX));
            var exitPlates = new DynamicSystemFrontalBuilder().Build(system, Catalog, DynamicRackEnd.Exit)
                .Where(instance => instance.Role == HeaderBlockRole.BasePlate)
                .OrderBy(instance => instance.ConnectionAnchor.X)
                .ToList();
            Assert.Equal(exitPlates[0].Insertion.Y, exit.OrderBy(instance => instance.Insertion.X).First().Insertion.Y, 4);
        }

        [Fact]
        public void EntranceGuideDrawsOnlyAtEntrance_TwoPerFrontAndLevel_WhilePlantaCollapsesLevels()
        {
            var system = System();
            system.SafetySelections.Add(new SelectiveSafetySelection
            {
                ElementId = "GUIA_ENTRADA",
                Quantity = 1,
                Side = SafetySide.None
            });

            var exit = new DynamicSystemFrontalBuilder().Build(system, Catalog, DynamicRackEnd.Exit)
                .Where(instance => instance.PieceId == "GUIA_ENTRADA").ToList();
            var entrance = new DynamicSystemFrontalBuilder().Build(system, Catalog, DynamicRackEnd.Entrance)
                .Where(instance => instance.PieceId == "GUIA_ENTRADA").ToList();
            var planta = new DynamicSystemPlantaBuilder().Build(system, Catalog)
                .Where(instance => instance.PieceId == "GUIA_ENTRADA").ToList();
            var physical = DynamicEntranceGuidePlan.Build(
                system,
                system.SafetySelections.Single(selection => selection.ElementId == "GUIA_ENTRADA"));

            Assert.Empty(exit);
            Assert.Equal(physical.Count, entrance.Count);
            Assert.Equal(system.Fronts.Count * 2, planta.Count);
            var layout = DynamicFrontGeometry.Compute(system, Catalog);
            foreach (var placement in physical)
            {
                var expectedX = layout.PostPositions[placement.PostIndex]
                                + (placement.MirroredAcrossFront
                                    ? -layout.TroquelPositions[placement.PostIndex]
                                    : layout.TroquelPositions[placement.PostIndex]);
                Assert.Contains(entrance, instance =>
                    Math.Abs(instance.Insertion.X - expectedX) < 1e-4
                    && Math.Abs(instance.Insertion.Y - placement.Elevation) < 1e-4
                    && instance.MirroredX == placement.MirroredAcrossFront
                    && Math.Abs(instance.DynamicParameters[SelectiveRackDefaults.LengthParam] - placement.Length) < 1e-4);
            }

            Assert.All(planta, instance => Assert.True(instance.MirroredX));
            Assert.All(planta, instance => Assert.True(
                instance.DynamicParameters[SelectiveRackDefaults.LengthParam] > 0.0));
        }

        [Fact]
        public void LinkedViews_DrawRequestedNumbersDimensionsAndRackName()
        {
            var system = System();
            system.Name = "Rack dinamico A";
            system.NumberFronts = true;
            system.NumberLevels = true;
            system.DrawRackName = true;
            system.Dimensions = DimensionDetail.Standard;
            system.DimensionStyle = "RackCad";

            var frontal = new DynamicSystemFrontalBuilder().Build(system, Catalog, DynamicRackEnd.Exit);
            var planta = new DynamicSystemPlantaBuilder().Build(system, Catalog);
            var lateral = new DynamicSystemLateralBuilder().Build(system, Catalog).Flatten().Instances;

            Assert.Contains(frontal, instance => instance.Role == HeaderBlockRole.Annotation
                                                 && instance.Text == system.Name);
            Assert.Contains(planta, instance => instance.Role == HeaderBlockRole.Annotation
                                                && instance.Text == system.Name);
            Assert.Contains(lateral, instance => instance.Role == HeaderBlockRole.Annotation
                                                 && instance.Text == system.Name);
            Assert.True(frontal.Count(instance => instance.Role == HeaderBlockRole.Annotation) >=
                        system.Fronts.Count + system.LoadBeamLevels.Count + 1);
            Assert.True(planta.Count(instance => instance.Role == HeaderBlockRole.Annotation) >=
                        system.Fronts.Count + 1);
            Assert.Contains(frontal, instance => instance.Role == HeaderBlockRole.Dimension
                                                 && instance.DimensionStyleName == "RackCad");
            Assert.Contains(planta, instance => instance.Role == HeaderBlockRole.Dimension);
            Assert.Contains(lateral, instance => instance.Role == HeaderBlockRole.Dimension);
        }

        [Fact]
        public void Frontal_BeamCutDimensionStartsAtTheProfileSection_NotAtTheHookTroquel()
        {
            var system = System();
            system.Dimensions = DimensionDetail.Standard;
            var catalog = Catalog;
            var layout = DynamicFrontGeometry.Compute(system, catalog);
            var profileStart = SelectivePostGeometry.Resolve(
                catalog.ConnectionLayout.FindConnectionLayout(
                    system.InOutBeamCatalogId,
                    SelectiveRackDefaults.BeamProfileStartPoint,
                    "FRONTAL"),
                new Dictionary<string, double>
                {
                    [SelectiveRackDefaults.PeralteParam] = system.InOutBeamDepth
                }).X;
            var expectedStart = layout.PostPositions[0] + layout.TroquelPositions[0] + profileStart;
            var expectedLength = system.Fronts[0].BeamLength;

            var dimension = Assert.Single(
                new DynamicSystemFrontalBuilder().Build(system, catalog, DynamicRackEnd.Exit),
                instance => instance.Role == HeaderBlockRole.Dimension
                            && global::System.Math.Abs(
                                instance.ConnectionAnchor.X - instance.Insertion.X - expectedLength) < 1e-4);

            Assert.Equal(expectedStart, dimension.Insertion.X, 4);
        }
    }
}
