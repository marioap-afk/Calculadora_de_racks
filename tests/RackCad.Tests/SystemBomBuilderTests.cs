using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    public class SystemBomBuilderTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static DynamicRackSystem StandardSystem()
        {
            return new DynamicRackSystemBuilder(Catalog).BuildDefault(
                new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                palletsDeep: 4,
                headerTemplate: RackFrameTemplateCatalog.Default,
                headerPostCatalogId: "POSTE_OMEGA_3X3",
                headerHeight: 132.0);
        }

        [Fact]
        public void Build_CountsHeaderDerivedAndSeparatorPieces()
        {
            var system = StandardSystem();
            var singleHeader = BomBuilder.Build(system.Modules.First().AssociatedFrameConfiguration, Catalog);
            var systemBom = SystemBomBuilder.Build(system, Catalog);
            var derivedSupports = system.GetDerivedPostOffsets().Count;
            var separators = systemBom.Components
                .Where(component => component.Category == SystemBomBuilder.Separator)
                .Sum(component => component.Quantity);

            Assert.Equal(2 * singleHeader.TotalPieces + 4 * derivedSupports + separators, systemBom.TotalPieces);
            Assert.Equal(32, systemBom.TotalPieces); // 22 de cabeceras + 4 del apoyo reforzado + 6 separadores.

            var post = systemBom.Lines.Single(l => l.Category == BomBuilder.Post && l.ProfileId == "POSTE_OMEGA_3X3");
            Assert.Equal(6, post.Quantity);
            Assert.Equal(132.0, post.Length);
            Assert.Equal(6, systemBom.Lines.Single(l => l.Category == BomBuilder.BasePlate).Quantity);
            Assert.Equal(derivedSupports, Assert.Single(systemBom.Components,
                component => component.Category == SystemBomBuilder.ReinforcedPost).Quantity);
        }

        [Fact]
        public void Build_NullSystem_ReturnsEmptyBom()
        {
            Assert.Empty(SystemBomBuilder.Build(null, Catalog).Lines);
        }

        [Fact]
        public void Build_ResolvedDynamicSystem_CountsOneCompleteBedPerLevelWithoutInternalBreakdown()
        {
            var system = new DynamicRackSystemResolver(Catalog).Resolve(new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 3,
                FirstLevelHeight = 6.0,
                BeamDepth = 6.0,
                HeaderPostCatalogId = "POSTE_OMEGA_3X3"
            }).System;

            var bom = SystemBomBuilder.Build(system, Catalog);
            var bed = Assert.Single(bom.Components, component => component.Category == SystemBomBuilder.Cama);

            Assert.Equal(3, bed.Quantity);
            Assert.StartsWith(SystemBomBuilder.Cama, bed.Description);
            Assert.Equal(DynamicFlowBedGeometry.ResolveBedLength(system), bed.Length, 4);
            Assert.Contains("BFR 44", bed.Description);
            Assert.Empty(bed.Pieces);
            Assert.DoesNotContain(bom.Lines, line => line.ProfileId == FlowBedDefaults.RailId);
        }

        [Fact]
        public void Build_ResolvedSystem_CountsHeaderAndDerivedPostsAcrossEveryFrontLine()
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
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 2, LoadLevels = 3 });
            var system = new DynamicRackSystemResolver(Catalog).Resolve(design).System;
            var postLines = system.Fronts.Count + 1;
            var regularPosts = system.Modules.Count(module => module.IsHeader) * 2 * postLines;
            var derivedPosts = system.GetDerivedPostOffsets().Count * 2 * postLines;

            var bom = SystemBomBuilder.Build(system, Catalog);

            Assert.Equal(regularPosts + derivedPosts, bom.Lines
                .Where(line => line.Category == BomBuilder.Post && line.ProfileId == CatalogIds.StandardPost)
                .Sum(line => line.Quantity));
            var reinforced = Assert.Single(bom.Components,
                component => component.Category == "Poste reforzado");
            Assert.Equal(system.GetDerivedPostOffsets().Count * postLines, reinforced.Quantity);
        }

        [Fact]
        public void Build_AddsIntermediateBeamsWithTheirLengthPeralteAndPhysicalQuantity()
        {
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 2,
                FirstLevelHeight = 6.0,
                BeamDepth = 6.0,
                HeaderPostCatalogId = CatalogIds.StandardPost
            };
            var front = new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2 };
            front.IntermediateBeamDepths.Add(3.5);
            front.IntermediateBeamDepths.Add(5.0);
            design.Fronts.Add(front);
            var system = new DynamicRackSystemResolver(Catalog).Resolve(design).System;
            var supportCount = DynamicIntermediateBeamGeometry.Supports(
                system,
                CatalogLookup.Local(Catalog, CatalogIds.StandardPost, "FIN_POSTE", "LATERAL")).Count;

            var beams = SystemBomBuilder.Build(system, Catalog).Components
                .Where(component => component.Category == "Larguero intermedio")
                .OrderBy(component => component.Description)
                .ToList();

            Assert.Equal(2, beams.Count);
            Assert.All(beams, component => Assert.Equal(system.Fronts[0].BeamLength, component.Length, 4));
            Assert.All(beams, component => Assert.Equal(supportCount, component.Quantity));
            Assert.Contains(beams, component => component.Description.Contains("Peralte 3.5"));
            Assert.Contains(beams, component => component.Description.Contains("Peralte 5"));
        }

        [Fact]
        public void Build_AddsEveryInOutBeamByFrontAndLevel_WithLengthAndPeralte()
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
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 3, LoadLevels = 3 });
            var system = new DynamicRackSystemResolver(Catalog).Resolve(design).System;

            var beams = SystemBomBuilder.Build(system, Catalog).Components
                .Where(component => component.Category == SystemBomBuilder.InOutBeam)
                .OrderBy(component => component.Length)
                .ToList();

            Assert.Equal(2, beams.Count);
            Assert.Equal(system.Fronts[0].BeamLength, beams[0].Length, 4);
            Assert.Equal(4, beams[0].Quantity); // Entrada + salida por cada uno de los 2 niveles.
            Assert.Equal(system.Fronts[1].BeamLength, beams[1].Length, 4);
            Assert.Equal(6, beams[1].Quantity); // Entrada + salida por cada uno de los 3 niveles.
            Assert.All(beams, component => Assert.Equal(DynamicRackDefaults.InOutBeamCatalogId, component.ProfileId));
            Assert.All(beams, component => Assert.Contains("Peralte 6", component.Description));
        }

        [Fact]
        public void Build_CellSpecificPalletsAndBeams_GroupBedsByBfrAndKeepPhysicalFrontLength()
        {
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 2,
                FirstLevelHeight = 6.0,
                BeamDepth = 6.0,
                HeaderPostCatalogId = CatalogIds.StandardPost
            };
            var front = new DynamicRackFrontDesign { PalletCount = 2, LoadLevels = 2 };
            front.Levels.Add(new DynamicRackLevelDesign
            {
                PalletFront = 40.0,
                InOutBeamDepth = 6.0,
                IntermediateBeamDepth = 3.5
            });
            front.Levels.Add(new DynamicRackLevelDesign
            {
                PalletFront = 44.0,
                InOutBeamDepth = 6.0,
                IntermediateBeamDepth = 5.0
            });
            design.Fronts.Add(front);
            var system = new DynamicRackSystemResolver(Catalog).Resolve(design).System;

            var bom = SystemBomBuilder.Build(system, Catalog);
            var beds = bom.Components
                .Where(component => component.Category == SystemBomBuilder.Cama)
                .OrderBy(component => component.Description)
                .ToList();
            var inOut = bom.Components.Where(component => component.Category == SystemBomBuilder.InOutBeam).ToList();
            var intermediate = bom.Components.Where(component => component.Category == SystemBomBuilder.IntermediateBeam).ToList();

            Assert.Equal(2, beds.Count);
            Assert.All(beds, bed => Assert.Equal(2, bed.Quantity));
            Assert.Contains(beds, bed => bed.Description.Contains("BFR 42"));
            Assert.Contains(beds, bed => bed.Description.Contains("BFR 46"));
            Assert.Single(inOut);
            Assert.Equal(4, inOut[0].Quantity);
            Assert.Equal(98.0, inOut[0].Length, 4);
            Assert.Equal(2, intermediate.Count);
            Assert.Contains(intermediate, beam => beam.Description.Contains("Peralte 3.5"));
            Assert.Contains(intermediate, beam => beam.Description.Contains("Peralte 5"));
        }

        [Fact]
        public void Build_CountsEveryDrawnDynamicSeparatorAcrossAllPostSections()
        {
            var system = new DynamicRackSystemResolver(Catalog).Resolve(new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 3,
                FirstLevelHeight = 6.0,
                BeamDepth = 6.0,
                HeaderPostCatalogId = CatalogIds.StandardPost
            }).System;
            var expected = new DynamicSystemLateralBuilder().Cortes(system, Catalog)
                .SelectMany(corte => corte.Plan.Flatten().Instances)
                .Count(instance => instance.Role == HeaderBlockRole.Separator
                                   && instance.PieceId == DynamicRackDefaults.SeparatorCatalogId);

            var separators = SystemBomBuilder.Build(system, Catalog).Components
                .Where(component => component.Category == SystemBomBuilder.Separator)
                .ToList();

            Assert.True(expected > 0);
            Assert.Equal(expected, separators.Sum(component => component.Quantity));
            Assert.All(separators, component => Assert.True(component.Length > 0.0));
            Assert.All(separators, component => Assert.Equal(
                Catalog.SpacerProfiles.FindProfile(DynamicRackDefaults.SeparatorCatalogId).Label,
                component.Description));
        }

        [Fact]
        public void Build_MultipleFrontWidths_CountsOneBedPerLaneAndLevel()
        {
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 3,
                FirstLevelHeight = 6.0,
                BeamDepth = 6.0
            };
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 3 });
            var system = new DynamicRackSystemResolver(Catalog).Resolve(design).System;

            var bed = Assert.Single(SystemBomBuilder.Build(system, Catalog).Components,
                component => component.Category == SystemBomBuilder.Cama);

            Assert.Equal(12, bed.Quantity);
        }

        [Fact]
        public void Build_FrontSpecificLevels_CountsOnlyTheBedsThatExistInEachFront()
        {
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 4,
                FirstLevelHeight = 6.0,
                BeamDepth = 6.0
            };
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 3, LoadLevels = 4 });
            var system = new DynamicRackSystemResolver(Catalog).Resolve(design).System;

            var bed = Assert.Single(SystemBomBuilder.Build(system, Catalog).Components,
                component => component.Category == SystemBomBuilder.Cama);

            Assert.Equal(14, bed.Quantity);
        }

        [Fact]
        public void Build_DynamicSafetyCountsEveryPhysicalPostAndEnd_WithoutDuplicatingViews()
        {
            var system = new DynamicRackSystemResolver(Catalog).Resolve(new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 3,
                FirstLevelHeight = 6.0,
                BeamDepth = 6.0,
                HeaderPostCatalogId = CatalogIds.StandardPost
            }).System;
            system.SafetySelections.Add(new SelectiveSafetySelection
            {
                ElementId = "PROTECTOR_BOTA_C_6",
                Quantity = 1,
                Side = SafetySide.Both
            });
            var lateral = new SelectiveSafetySelection
            {
                ElementId = "PROTECTOR_LATERAL_BOTA_C_6",
                Quantity = 1,
                Side = SafetySide.None
            };
            lateral.PostSides.Add(new SafetyPostSide { PostIndex = 0, Side = SafetySide.Left });
            system.SafetySelections.Add(lateral);
            var desviador = new SelectiveSafetySelection
            {
                ElementId = "DESVIADOR_A_3",
                Quantity = 1,
                Side = SafetySide.Both,
                DesviadorLongitud = 18.0,
                DesviadorPrimerNivelAltura = 18.0
            };
            desviador.DesviadorOffCells.Add(new SelectiveGridCell { Frente = 0, Level = 1 });
            system.SafetySelections.Add(desviador);

            var bom = SystemBomBuilder.Build(system, Catalog);
            var boot = Assert.Single(bom.Components, component => component.ProfileId == "PROTECTOR_BOTA_C_6");
            Assert.Equal(2, boot.Quantity); // La guarda completa sustituye las dos botas de la linea del poste 0.
            var guard = Assert.Single(bom.Components, component => component.ProfileId == "PROTECTOR_LATERAL_BOTA_C_6");
            Assert.Equal(1, guard.Quantity);
            Assert.Equal(system.TotalLength + 4.0, guard.Length, 4);
            var diverter = Assert.Single(bom.Components, component => component.ProfileId == "DESVIADOR_A_3");
            Assert.Equal(8, diverter.Quantity); // 2 postes x 2 extremos x 2 niveles habilitados.
            Assert.Equal(18.0, diverter.Length, 4);
        }

        [Fact]
        public void Build_DynamicForkliftDefenseCountsPhysicalPairsGroupedByPostLength()
        {
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 3,
                HeaderPostCatalogId = CatalogIds.StandardPost
            };
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1 });
            var system = new DynamicRackSystemResolver(Catalog).Resolve(design).System;
            var defense = new SelectiveSafetySelection
            {
                ElementId = "DEFENSA_MONTACARGAS",
                Quantity = 1,
                Side = SafetySide.None
            };
            defense.DefensaPosts.Add(new SafetyPostDefense { PostIndex = 1, ExitLength = 0.0, EntranceLength = 0.0 });
            defense.DefensaPosts.Add(new SafetyPostDefense { PostIndex = 2, ExitLength = 48.0, EntranceLength = 60.0 });
            system.SafetySelections.Add(defense);

            var components = SystemBomBuilder.Build(system, Catalog).Components
                .Where(component => component.ProfileId == "DEFENSA_MONTACARGAS")
                .OrderBy(component => component.Length)
                .ToList();

            Assert.Equal(3, components.Count);
            Assert.Equal(12.0, components[0].Length, 4);
            Assert.Equal(2, components[0].Quantity);
            Assert.Equal(48.0, components[1].Length, 4);
            Assert.Equal(1, components[1].Quantity);
            Assert.Equal(60.0, components[2].Length, 4);
            Assert.Equal(1, components[2].Quantity);
        }

        [Fact]
        public void Build_DynamicEntranceGuideCountsBothSidesOfEveryEnabledFrontLevel()
        {
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 3,
                HeaderPostCatalogId = CatalogIds.StandardPost
            };
            design.Fronts.Add(new DynamicRackFrontDesign { LoadLevels = 2, PalletsDeep = 2 });
            design.Fronts.Add(new DynamicRackFrontDesign { LoadLevels = 3, PalletsDeep = 3 });
            var system = new DynamicRackSystemResolver(Catalog).Resolve(design).System;
            var guide = new SelectiveSafetySelection { ElementId = "GUIA_ENTRADA", Quantity = 1 };
            guide.GuiaEntradaOffCells.Add(new SelectiveGridCell { Frente = 0, Level = 1 });
            system.SafetySelections.Add(guide);

            var expected = DynamicEntranceGuidePlan.Build(system, guide);
            var components = SystemBomBuilder.Build(system, Catalog).Components
                .Where(component => component.ProfileId == "GUIA_ENTRADA")
                .ToList();

            Assert.Equal(expected.Count, components.Sum(component => component.Quantity));
            Assert.All(components, component => Assert.True(component.Length > 0.0));
        }

        [Fact]
        public void Build_NewDynamicSafetyDefaultsProduceEveryPhysicalFamily()
        {
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 3,
                HeaderPostCatalogId = CatalogIds.StandardPost
            };
            design.Fronts.Add(new DynamicRackFrontDesign { LoadLevels = 3 });
            design.Fronts.Add(new DynamicRackFrontDesign { LoadLevels = 3 });
            var system = new DynamicRackSystemResolver(Catalog).Resolve(design).System;
            foreach (var selection in DynamicSafetyDefaults.Build(Catalog))
            {
                system.SafetySelections.Add(selection.DeepCopy());
            }

            var bomIds = SystemBomBuilder.Build(system, Catalog).Components
                .Select(component => component.ProfileId)
                .ToHashSet(System.StringComparer.OrdinalIgnoreCase);

            Assert.All(DynamicSafetyDefaults.Build(Catalog), selection => Assert.Contains(selection.ElementId, bomIds));
        }

        [Fact]
        public void Build_DynamicLateralGuardsReplaceBothEndBootsAtEachProtectedPost()
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
            for (var front = 0; front < 4; front++)
            {
                design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 3 });
            }

            var system = new DynamicRackSystemResolver(Catalog).Resolve(design).System;
            system.SafetySelections.Add(new SelectiveSafetySelection
            {
                ElementId = "PROTECTOR_BOTA_C_6",
                Quantity = 1,
                Side = SafetySide.Both
            });
            var lateral = new SelectiveSafetySelection
            {
                ElementId = "PROTECTOR_LATERAL_BOTA_C_6",
                Quantity = 1,
                Side = SafetySide.None
            };
            lateral.PostSides.Add(new SafetyPostSide { PostIndex = 0, Side = SafetySide.Left });
            lateral.PostSides.Add(new SafetyPostSide { PostIndex = 4, Side = SafetySide.Left });
            system.SafetySelections.Add(lateral);

            var bom = SystemBomBuilder.Build(system, Catalog);

            Assert.Equal(2, Assert.Single(bom.Components,
                component => component.ProfileId == "PROTECTOR_LATERAL_BOTA_C_6").Quantity);
            Assert.Equal(6, Assert.Single(bom.Components,
                component => component.ProfileId == "PROTECTOR_BOTA_C_6").Quantity);
        }

        [Fact]
        public void Build_DynamicSafetyCountsPostSpecificBootsAndDesviadores_WhenPostZeroIsOff()
        {
            var system = new DynamicRackSystemResolver(Catalog).Resolve(new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 3,
                FirstLevelHeight = 6.0,
                BeamDepth = 6.0,
                HeaderPostCatalogId = CatalogIds.StandardPost
            }).System;
            var boot = new SelectiveSafetySelection
            {
                ElementId = "PROTECTOR_BOTA_C_6",
                Quantity = 1,
                Side = SafetySide.None
            };
            boot.PostSides.Add(new SafetyPostSide { PostIndex = 1, Side = SafetySide.Both });
            system.SafetySelections.Add(boot);
            var diverter = new SelectiveSafetySelection
            {
                ElementId = "DESVIADOR_A_3",
                Quantity = 1,
                Side = SafetySide.None,
                DesviadorLongitud = 18.0,
                DesviadorPrimerNivelAltura = 18.0
            };
            diverter.PostSides.Add(new SafetyPostSide { PostIndex = 1, Side = SafetySide.Both });
            system.SafetySelections.Add(diverter);

            var bom = SystemBomBuilder.Build(system, Catalog);

            Assert.Equal(2, Assert.Single(bom.Components,
                component => component.ProfileId == "PROTECTOR_BOTA_C_6").Quantity);
            Assert.Equal(6, Assert.Single(bom.Components,
                component => component.ProfileId == "DESVIADOR_A_3").Quantity);
        }
    }
}
