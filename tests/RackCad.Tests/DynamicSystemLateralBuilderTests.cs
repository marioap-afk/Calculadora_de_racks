using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    public class DynamicSystemLateralBuilderTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static DynamicRackSystem StandardSystem()
        {
            return new DynamicRackSystemBuilder(Catalog).BuildDefault(
                new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                palletsDeep: 4,
                headerTemplate: RackFrameTemplateCatalog.Default,
                headerPostCatalogId: CatalogIds.StandardPost,
                headerHeight: 132.0);
        }

        private static DynamicRackSystem ResolvedSystem()
        {
            return new DynamicRackSystemResolver(Catalog).Resolve(new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 3,
                FirstLevelHeight = 6.0,
                BeamDepth = 4.0,
                InOutBeamCatalogId = DynamicRackDefaults.InOutBeamCatalogId,
                HeaderPostCatalogId = CatalogIds.StandardPost
            }).System;
        }

        [Fact]
        public void Build_PlacesHeadersAlongTheRun()
        {
            var layout = new DynamicSystemLateralBuilder().Build(StandardSystem(), Catalog).Flatten();

            var postXs = layout.OfRole(HeaderBlockRole.Post)
                .Select(p => Math.Round(p.Insertion.X, 2))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            // More than one header → posts appear at more than one run position (offset by StartX).
            Assert.True(postXs.Count >= 2);
            Assert.Contains(0.0, postXs);            // first header at the origin
            Assert.Contains(postXs, x => x > 0.0);   // later headers shifted along the run
        }

        [Fact]
        public void Build_AddsSeparatorsAtEachLevel_WithResolvedBlockAndLength()
        {
            var layout = new DynamicSystemLateralBuilder().Build(StandardSystem(), Catalog).Flatten();
            var separators = layout.OfRole(HeaderBlockRole.Separator).ToList();

            Assert.NotEmpty(separators);

            // 132" header → 3 vertical levels.
            var levels = separators.Select(s => Math.Round(s.ConnectionAnchor.Y, 2)).Distinct().Count();
            Assert.Equal(SeparatorLevelCalculator.Count(132.0), levels);

            Assert.All(separators, s =>
            {
                Assert.False(string.IsNullOrWhiteSpace(s.BlockName));      // FRONTAL separator block resolved
                Assert.Equal(48.0, s.DynamicParameters["LONGITUD"], 4);    // LONGITUD = the module length (48"), as shown in the preview
                Assert.Equal("FRONTAL", s.View);
            });
        }

        [Fact]
        public void Build_SeparatorsAnchorOnThePostTroquel_NotTheModuleEdge()
        {
            var catalog = Catalog;
            var system = StandardSystem();
            var troquelSeparadorX = catalog.ConnectionLayout
                .FindConnectionLayout(CatalogIds.StandardPost, "TROQUEL_SEPARADOR", "LATERAL").LocalX;
            var firstSeparator = system.Modules.First(m => m.Kind == DynamicRackModuleKind.Separator && m.Length > 0.0);

            var layout = new DynamicSystemLateralBuilder().Build(system, catalog).Flatten();

            // Separators of the first gap anchor at moduleStartX - troquelSeparadorX (the previous post's troquel),
            // one per vertical level.
            var anchored = layout.OfRole(HeaderBlockRole.Separator)
                .Where(s => Math.Abs(s.ConnectionAnchor.X - (firstSeparator.StartX - troquelSeparadorX)) < 1e-3)
                .ToList();

            Assert.Equal(SeparatorLevelCalculator.Count(132.0), anchored.Count);
        }

        [Fact]
        public void Build_DerivedPost_AddsReinforcedPostWithPlate()
        {
            var catalog = Catalog;
            var system = StandardSystem();
            var offsets = system.GetDerivedPostOffsets();
            Assert.NotEmpty(offsets); // pallets-deep 4 → one derived post

            var layout = new DynamicSystemLateralBuilder().Build(system, catalog).Flatten();
            var offset = offsets[0];
            var finPosteX = catalog.ConnectionLayout
                .FindConnectionLayout(CatalogIds.StandardPost, "FIN_POSTE", "LATERAL").LocalX;
            var primaryX = offset - finPosteX;
            var reinforcementX = offset;

            // FIN_POSTE is the physical interface between the two profiles. It lands on the separator boundary,
            // centering the reinforced pair there instead of starting the whole assembly at that boundary.
            Assert.Contains(layout.OfRole(HeaderBlockRole.BasePlate), p => Math.Abs(p.ConnectionAnchor.X - primaryX) < 1e-3);

            // The post and its full-height reinforcement (mated at FIN_POSTE).
            Assert.Contains(layout.OfRole(HeaderBlockRole.Post), p => Math.Abs(p.ConnectionAnchor.X - primaryX) < 1e-3);
            Assert.Contains(layout.OfRole(HeaderBlockRole.Post), p => Math.Abs(p.ConnectionAnchor.X - reinforcementX) < 1e-3);
            Assert.DoesNotContain(layout.OfRole(HeaderBlockRole.Post), p => Math.Abs(p.ConnectionAnchor.X - (offset + finPosteX)) < 1e-3);
        }

        [Fact]
        public void Build_DerivedPost_NotReinforced_OmitsReinforcement()
        {
            var catalog = Catalog;
            var system = StandardSystem();
            system.DerivedPostReinforced = false;
            var offset = system.GetDerivedPostOffsets()[0];
            var finPosteX = catalog.ConnectionLayout
                .FindConnectionLayout(CatalogIds.StandardPost, "FIN_POSTE", "LATERAL").LocalX;

            var layout = new DynamicSystemLateralBuilder().Build(system, catalog).Flatten();

            // The post and plate are still there, but there is no reinforcement post at FIN_POSTE.
            Assert.Contains(layout.OfRole(HeaderBlockRole.Post), p => Math.Abs(p.ConnectionAnchor.X - offset) < 1e-3);
            Assert.DoesNotContain(layout.OfRole(HeaderBlockRole.Post), p => Math.Abs(p.ConnectionAnchor.X - (offset + finPosteX)) < 1e-3);
        }

        [Fact]
        public void Build_DerivedPost_ReinforcementHeightOverride_SetsLongitud()
        {
            var catalog = Catalog;
            var system = StandardSystem();
            system.DerivedPostReinforcementHeight = 60.0;
            var offset = system.GetDerivedPostOffsets()[0];
            var finPosteX = catalog.ConnectionLayout
                .FindConnectionLayout(CatalogIds.StandardPost, "FIN_POSTE", "LATERAL").LocalX;
            var reinforcementX = offset;

            var layout = new DynamicSystemLateralBuilder().Build(system, catalog).Flatten();
            var reinforcement = layout.OfRole(HeaderBlockRole.Post)
                .First(p => Math.Abs(p.ConnectionAnchor.X - reinforcementX) < 1e-3
                            && Math.Abs(p.DynamicParameters["LONGITUD"] - 60.0) < 1e-3);

            Assert.Equal(60.0, reinforcement.DynamicParameters["LONGITUD"], 4);
        }

        [Fact]
        public void Build_GroupsIdenticalHeaders_SharingOneDefinition()
        {
            // Pallets-deep 4 → both headers are end headers (length 54) → one shared definition, two placements.
            var plan = new DynamicSystemLateralBuilder().Build(StandardSystem(), Catalog);

            Assert.Single(plan.Headers);
            Assert.Equal(2, plan.Headers[0].Placements.Count);
        }

        [Fact]
        public void Build_AlternatesHeaderMirroringAlongTheLine()
        {
            // 6 pallets deep gives 3 headers: 1st normal, 2nd mirrored, 3rd normal (celosía alternates).
            var system = new DynamicRackSystemBuilder(Catalog).BuildDefault(
                new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                palletsDeep: 6,
                headerTemplate: RackFrameTemplateCatalog.Default,
                headerPostCatalogId: CatalogIds.StandardPost,
                headerHeight: 132.0);

            var plan = new DynamicSystemLateralBuilder().Build(system, Catalog);

            // Placements across all groups, ordered along the run, must alternate the mirror flag.
            var placements = plan.Headers
                .SelectMany(g => g.Placements)
                .OrderBy(p => p.InsertionX)
                .ToList();

            Assert.True(placements.Count >= 3);
            for (var i = 1; i < placements.Count; i++)
            {
                Assert.NotEqual(placements[i - 1].Mirrored, placements[i].Mirrored);
            }
        }

        [Fact]
        public void Build_AddsCompleteEntranceAndExitBeamAtEveryResolvedLoadLevel()
        {
            var system = ResolvedSystem();

            var beams = new DynamicSystemLateralBuilder().Build(system, Catalog).Flatten()
                .OfRole(HeaderBlockRole.Beam)
                .Where(beam => beam.PieceId == DynamicRackDefaults.InOutBeamCatalogId)
                .ToList();

            Assert.Equal(6, beams.Count);
            Assert.All(beams, beam =>
            {
                Assert.Equal(DynamicRackDefaults.InOutBeamCatalogId, beam.PieceId);
                Assert.Equal("LARGUERO_IN_OUT_C6_LATERAL", beam.BlockName);
                Assert.Equal(DynamicRackDefaults.InOutBeamView, beam.View);
                Assert.Equal(beam.ConnectionAnchor.X, beam.Insertion.X, 4);
                Assert.Equal(beam.ConnectionAnchor.Y, beam.Insertion.Y, 4);
                Assert.Empty(beam.DynamicParameters);
            });

            var firstEntrance = beams.Single(beam => Math.Abs(beam.Insertion.X - system.TotalLength) < 1e-6
                                                     && Math.Abs(beam.Insertion.Y - system.LoadBeamLevels[0].EntranceElevation) < 1e-6);
            var firstExit = beams.Single(beam => Math.Abs(beam.Insertion.X) < 1e-6
                                                 && Math.Abs(beam.Insertion.Y - system.LoadBeamLevels[0].ExitElevation) < 1e-6);
            Assert.True(firstEntrance.MirroredX);
            Assert.False(firstExit.MirroredX);
        }

        [Fact]
        public void Build_AddsOneSharedCompleteBedDefinitionAtAllResolvedLevels()
        {
            var system = ResolvedSystem();

            var plan = new DynamicSystemLateralBuilder().Build(system, Catalog);
            var bedGroup = Assert.Single(plan.Headers, group =>
                group.Instances.Any(instance => instance.Role == HeaderBlockRole.Rail));

            Assert.Equal(system.LoadBeamLevels.Count, bedGroup.Placements.Count);
            var flat = plan.Flatten();
            Assert.Equal(system.LoadBeamLevels.Count, flat.OfRole(HeaderBlockRole.Rail).Count());
            Assert.Equal(
                system.LoadBeamLevels.Count * bedGroup.Instances.Count(instance => instance.Role == HeaderBlockRole.Roller),
                flat.OfRole(HeaderBlockRole.Roller).Count());
        }

        [Fact]
        public void Build_AddsOneIntermediateBeamAtEveryInternalPost_MatedToRailOriginLine()
        {
            var catalog = Catalog;
            var system = ResolvedSystem();
            var leftMate = catalog.ConnectionLayout.FindConnectionLayout(
                "LARGUERO_ESCALON_INFINITO",
                "INICIO_IZQUIERDO",
                "LATERAL");
            var rightMate = catalog.ConnectionLayout.FindConnectionLayout(
                "LARGUERO_ESCALON_INFINITO",
                "INICIO_DERECHO",
                "LATERAL");
            var finPoste = catalog.ConnectionLayout.FindConnectionLayout(
                CatalogIds.StandardPost,
                "FIN_POSTE",
                "LATERAL");

            var plan = new DynamicSystemLateralBuilder().Build(system, catalog);
            var intermediateGroups = plan.Headers
                .Where(group => group.Instances.Any(instance => instance.PieceId == "LARGUERO_ESCALON_INFINITO"))
                .ToList();
            var intermediate = plan.Flatten()
                .OfRole(HeaderBlockRole.Beam)
                .Where(instance => instance.PieceId == "LARGUERO_ESCALON_INFINITO")
                .ToList();
            var rails = plan.Flatten().OfRole(HeaderBlockRole.Rail)
                .OrderBy(instance => instance.Insertion.Y)
                .ToList();

            Assert.Equal(2, intermediateGroups.Count);
            Assert.All(intermediateGroups, group =>
            {
                Assert.Single(group.Instances);
            });
            Assert.Equal((system.Modules.Count - 1) * system.LoadBeamLevels.Count, intermediate.Count);
            Assert.Equal(system.LoadBeamLevels.Count, rails.Count);

            for (var moduleIndex = 1; moduleIndex < system.Modules.Count; moduleIndex++)
            {
                var previous = system.Modules[moduleIndex - 1];
                var current = system.Modules[moduleIndex];
                var boundaryX = current.StartX;

                // End/second post of a header uses the mirrored/right mate; start/first post uses normal/left.
                // A separator-separator boundary is the reinforced derived post and keeps one normal beam on the
                // primary profile, whose origin sits FIN_POSTE.X to the left of the separator boundary.
                var expectedMirrored = previous.IsHeader;
                var expectedPostX = !previous.IsHeader && !current.IsHeader
                    ? boundaryX - finPoste.LocalX
                    : boundaryX;
                var mate = expectedMirrored ? rightMate : leftMate;
                var atPost = intermediate
                    .Where(instance => Math.Abs(instance.Insertion.X - expectedPostX) < 1e-6)
                    .ToList();

                Assert.Equal(system.LoadBeamLevels.Count, atPost.Count);
                Assert.All(atPost, instance => Assert.Equal(expectedMirrored, instance.MirroredX));

                foreach (var rail in rails)
                {
                    var match = Assert.Single(atPost, instance =>
                    {
                        var contactX = instance.Insertion.X + (instance.MirroredX ? -mate.LocalX : mate.LocalX);
                        var contactY = instance.Insertion.Y + mate.LocalY;
                        var railOriginY = rail.Insertion.Y
                                          + (contactX - rail.Insertion.X) * Math.Tan(rail.RotationRadians);
                        return Math.Abs(contactY - railOriginY) < 1e-4;
                    });

                    Assert.Equal(expectedPostX, match.Insertion.X, 4);
                }
            }

            Assert.All(intermediate, instance => Assert.Equal(0.0, instance.RotationRadians, 6));
        }

        [Fact]
        public void Build_AppliesTheCatalogedIntermediateBeamPeralteOfEachLevel()
        {
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 3,
                FirstLevelHeight = 6.0,
                BeamDepth = 6.0,
                HeaderPostCatalogId = CatalogIds.StandardPost
            };
            var firstFront = new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 3 };
            firstFront.IntermediateBeamDepths.Add(3.0);
            firstFront.IntermediateBeamDepths.Add(4.0);
            firstFront.IntermediateBeamDepths.Add(5.0);
            design.Fronts.Add(firstFront);
            var secondFront = new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 3 };
            secondFront.IntermediateBeamDepths.Add(3.5);
            secondFront.IntermediateBeamDepths.Add(4.5);
            secondFront.IntermediateBeamDepths.Add(6.0);
            design.Fronts.Add(secondFront);
            var system = new DynamicRackSystemResolver(Catalog).Resolve(design).System;
            var supportCount = DynamicIntermediateBeamGeometry.Supports(
                system,
                CatalogLookup.Local(Catalog, CatalogIds.StandardPost, "FIN_POSTE", "LATERAL")).Count;

            var intermediate = new DynamicSystemLateralBuilder().Build(system, Catalog).Flatten()
                .OfRole(HeaderBlockRole.Beam)
                .Where(instance => instance.PieceId == DynamicRackDefaults.IntermediateBeamCatalogId)
                .ToList();

            Assert.Equal(new[] { 3.0, 4.0, 5.0 }, system.Fronts[0].IntermediateBeamDepths);
            Assert.Equal(new[] { 3.5, 4.5, 6.0 }, system.Fronts[1].IntermediateBeamDepths);
            Assert.Equal(supportCount, intermediate.Count(instance =>
                Math.Abs(instance.DynamicParameters[SelectiveRackDefaults.PeralteParam] - 3.5) < 1e-6));
            Assert.Equal(supportCount, intermediate.Count(instance =>
                Math.Abs(instance.DynamicParameters[SelectiveRackDefaults.PeralteParam] - 4.5) < 1e-6));
            Assert.Equal(supportCount, intermediate.Count(instance =>
                Math.Abs(instance.DynamicParameters[SelectiveRackDefaults.PeralteParam] - 6.0) < 1e-6));
        }

        [Fact]
        public void Build_SafetyBootsUseEndpointPlateOrigins_AndLateralReplacesThem()
        {
            var system = ResolvedSystem();
            system.SafetySelections.Add(new SelectiveSafetySelection
            {
                ElementId = "PROTECTOR_BOTA_C_6",
                Quantity = 1,
                Side = SafetySide.Both
            });

            var withBoots = new DynamicSystemLateralBuilder().Build(system, Catalog).Flatten();
            var boots = withBoots.OfRole(HeaderBlockRole.Safety)
                .Where(instance => instance.PieceId == "PROTECTOR_BOTA_C_6")
                .OrderBy(instance => instance.Insertion.X)
                .ToList();
            var endpointPlates = withBoots.OfRole(HeaderBlockRole.BasePlate)
                .Where(instance => Math.Abs(instance.ConnectionAnchor.X) < 1e-4
                                   || Math.Abs(instance.ConnectionAnchor.X - system.TotalLength) < 1e-4)
                .OrderBy(instance => instance.ConnectionAnchor.X)
                .ToList();

            Assert.Equal(2, boots.Count);
            Assert.Equal(2, endpointPlates.Count);
            Assert.Equal(endpointPlates[0].Insertion.X, boots[0].Insertion.X, 4);
            Assert.Equal(endpointPlates[1].Insertion.X, boots[1].Insertion.X, 4);
            Assert.False(boots[0].MirroredX);
            Assert.True(boots[1].MirroredX);

            var lateral = new SelectiveSafetySelection
            {
                ElementId = "PROTECTOR_LATERAL_BOTA_C_6",
                Quantity = 1,
                Side = SafetySide.None
            };
            lateral.PostSides.Add(new SafetyPostSide { PostIndex = 0, Side = SafetySide.Left });
            system.SafetySelections.Add(lateral);

            var withLateral = new DynamicSystemLateralBuilder().Build(system, Catalog).Flatten();
            Assert.DoesNotContain(withLateral.OfRole(HeaderBlockRole.Safety), instance => instance.PieceId == "PROTECTOR_BOTA_C_6");
            var guard = Assert.Single(withLateral.OfRole(HeaderBlockRole.Safety),
                instance => instance.PieceId == "PROTECTOR_LATERAL_BOTA_C_6");
            Assert.Equal(system.TotalLength, guard.DynamicParameters[SelectiveRackDefaults.LengthParam], 4);
            Assert.False(guard.MirroredX);
        }

        [Fact]
        public void Build_ForkliftDefenseUsesPostOriginOffsetAndMirrorsAtTheEntrance()
        {
            var system = ResolvedSystem();
            var defense = new SelectiveSafetySelection
            {
                ElementId = "DEFENSA_MONTACARGAS",
                Quantity = 1,
                Side = SafetySide.None
            };
            defense.DefensaPosts.Add(new SafetyPostDefense
            {
                PostIndex = 0,
                ExitLength = 18.0,
                EntranceLength = 24.0
            });
            system.SafetySelections.Add(defense);

            var pieces = new DynamicSystemLateralBuilder().Build(system, Catalog).Flatten()
                .OfRole(HeaderBlockRole.Safety)
                .Where(instance => instance.PieceId == "DEFENSA_MONTACARGAS")
                .OrderBy(instance => instance.Insertion.X)
                .ToList();

            Assert.Equal(2, pieces.Count);
            Assert.Equal(-4.75, pieces[0].Insertion.X, 4);
            Assert.Equal(system.TotalLength + 4.75, pieces[1].Insertion.X, 4);
            var endpointPlates = new DynamicSystemLateralBuilder().Build(system, Catalog).Flatten()
                .OfRole(HeaderBlockRole.BasePlate)
                .Where(instance => Math.Abs(instance.ConnectionAnchor.X) < 1e-4
                                   || Math.Abs(instance.ConnectionAnchor.X - system.TotalLength) < 1e-4)
                .OrderBy(instance => instance.ConnectionAnchor.X)
                .ToList();
            Assert.Equal(endpointPlates[0].Insertion.Y, pieces[0].Insertion.Y, 4);
            Assert.Equal(endpointPlates[1].Insertion.Y, pieces[1].Insertion.Y, 4);
            Assert.False(pieces[0].MirroredX);
            Assert.True(pieces[1].MirroredX);
            Assert.Equal(18.0, pieces[0].DynamicParameters[SelectiveRackDefaults.LengthParam], 4);
            Assert.Equal(24.0, pieces[1].DynamicParameters[SelectiveRackDefaults.LengthParam], 4);
        }

        [Fact]
        public void Build_EntranceGuideUsesSelectedLevelsEightInchesAboveEntranceBeam()
        {
            var system = ResolvedSystem();
            var guide = new SelectiveSafetySelection { ElementId = "GUIA_ENTRADA", Quantity = 1 };
            guide.GuiaEntradaOffCells.Add(new SelectiveGridCell { Frente = 0, Level = 1 });
            system.SafetySelections.Add(guide);

            var pieces = new DynamicSystemLateralBuilder().Build(system, Catalog).Flatten()
                .OfRole(HeaderBlockRole.Safety)
                .Where(instance => instance.PieceId == "GUIA_ENTRADA")
                .OrderBy(instance => instance.Insertion.Y)
                .ToList();

            Assert.Equal(2, pieces.Count);
            Assert.Equal(system.LoadBeamLevels[0].EntranceElevation + 8.0, pieces[0].Insertion.Y, 4);
            Assert.Equal(system.LoadBeamLevels[2].EntranceElevation + 8.0, pieces[1].Insertion.Y, 4);
            Assert.All(pieces, piece => Assert.True(piece.MirroredX));
            Assert.All(pieces, piece => Assert.Equal(
                DynamicEntranceGuidePlan.EntranceSegmentLength(system, system.Fronts[0]),
                piece.DynamicParameters[SelectiveRackDefaults.LengthParam], 4));
        }

        [Fact]
        public void Build_DesviadoresUseSelectiveVerticalContractAndOneCutGrid()
        {
            var catalog = Catalog;
            var system = ResolvedSystem();
            var selection = new SelectiveSafetySelection
            {
                ElementId = "DESVIADOR_A_3",
                Quantity = 1,
                Side = SafetySide.Both,
                DesviadorLongitud = 18.0,
                DesviadorPrimerNivelAltura = 18.0
            };
            selection.DesviadorOffCells.Add(new SelectiveGridCell { Frente = 0, Level = 1 });
            system.SafetySelections.Add(selection);

            var pieces = new DynamicSystemLateralBuilder().Build(system, catalog).Flatten()
                .OfRole(HeaderBlockRole.Safety)
                .Where(instance => instance.PieceId == "DESVIADOR_A_3")
                .ToList();
            var troquel = catalog.ConnectionLayout.FindConnectionLayout(
                CatalogIds.StandardPost,
                SelectiveRackDefaults.PostBeamPoint,
                SelectiveRackDefaults.View);
            var firstY = troquel.LocalY + 18.0;

            Assert.Equal(4, pieces.Count); // 3 levels x 2 ends, with the middle grid cell disabled on both faces.
            Assert.Contains(pieces, piece => Math.Abs(piece.Insertion.X) < 1e-4 && Math.Abs(piece.Insertion.Y - firstY) < 1e-4 && !piece.MirroredX);
            Assert.Contains(pieces, piece => Math.Abs(piece.Insertion.X - system.TotalLength) < 1e-4 && Math.Abs(piece.Insertion.Y - firstY) < 1e-4 && piece.MirroredX);
            Assert.Contains(pieces, piece => Math.Abs(piece.Insertion.X) < 1e-4
                                             && Math.Abs(piece.Insertion.Y - (system.LoadBeamLevels[2].ExitElevation - SelectiveDesviadorPlan.BeamYOffset)) < 1e-4);
            Assert.Contains(pieces, piece => Math.Abs(piece.Insertion.X - system.TotalLength) < 1e-4
                                             && Math.Abs(piece.Insertion.Y - (system.LoadBeamLevels[2].EntranceElevation - SelectiveDesviadorPlan.BeamYOffset)) < 1e-4);
            Assert.All(pieces, piece => Assert.Equal(18.0, piece.DynamicParameters[SelectiveRackDefaults.LengthParam], 4));
        }

        [Fact]
        public void Build_NullSystem_ReturnsEmptyPlan()
        {
            var layout = new DynamicSystemLateralBuilder().Build(null, Catalog).Flatten();
            Assert.Empty(layout.Instances);
        }

        [Fact]
        public void Cortes_UseOnlyTheHeightAndLevelsOfEachPostsAdjacentFronts()
        {
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 3,
                FirstLevelHeight = 6.0,
                BeamDepth = 6.0,
                HeaderPostCatalogId = CatalogIds.StandardPost
            };
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 3 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 5 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 3 });
            var system = new DynamicRackSystemResolver(Catalog).Resolve(design).System;

            var cortes = new DynamicSystemLateralBuilder().Cortes(system, Catalog);

            Assert.Equal(4, cortes.Count);
            Assert.Equal(system.Fronts[0].Height, MaxPostLength(cortes[0].Plan), 4);
            Assert.Equal(system.Fronts[1].Height, MaxPostLength(cortes[1].Plan), 4);
            Assert.Equal(6, LoadBeamCount(cortes[0].Plan));
            Assert.Equal(10, LoadBeamCount(cortes[1].Plan));
            Assert.Equal(10, LoadBeamCount(cortes[2].Plan));
            Assert.Equal(6, LoadBeamCount(cortes[3].Plan));
        }

        private static double MaxPostLength(DynamicSystemPlan plan)
            => plan.Flatten().OfRole(HeaderBlockRole.Post)
                .Select(instance => instance.DynamicParameters.TryGetValue(
                    SelectiveRackDefaults.LengthParam,
                    out var length) ? length : 0.0)
                .DefaultIfEmpty(0.0)
                .Max();

        private static int LoadBeamCount(DynamicSystemPlan plan)
            => plan.Flatten().OfRole(HeaderBlockRole.Beam)
                .Count(instance => instance.PieceId == DynamicRackDefaults.InOutBeamCatalogId);
    }
}
