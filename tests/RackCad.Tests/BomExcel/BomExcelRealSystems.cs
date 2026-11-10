using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Geometry;
using RackCad.Application.RackFrames;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Cantilever;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.FlowBed;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Cantilever;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Tests
{
    /// <summary>I-68: el BOM de cada sistema de RACKBOMTOTAL, producido por su constructor real.</summary>
    internal static class BomExcelRealSystems
    {
        private const string Bota = "PROTECTOR_BOTA_H_3_16_18";
        private const string ColumnW = "AISC-W-W10X33";
        private const string BaseW = "AISC-W-W12X26";
        private const string ArmHss = "AISC-HSS-RECT-HSS4X4X_250";

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static readonly StructuralSectionCatalog Sections = new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        public static readonly string[] Names = { "Selectivo", "Dinámico", "Push Back", "Cantilever", "Cabecera", "Cama" };

        public static BillOfMaterials Of(string system)
        {
            switch (system)
            {
                case "Selectivo": return Selective(2);
                case "Dinámico": return Dynamic();
                case "Push Back": return PushBack();
                case "Cantilever": return Cantilever(2);
                case "Cabecera": return Cabecera();
                default: return Cama();
            }
        }

        public static ConsolidatedRackBom Rack(string system, int copies)
            => new ConsolidatedRackBom { Name = system + " 1", Kind = system, Copies = copies, Bom = Of(system) };

        public static BillOfMaterials Selective(int bays)
        {
            var system = new SelectiveRackSystem { Height = 240.0, PostId = TestCatalogIds.Profiles.Posts.Standard, PostPeralte = 3.0, PalletDepth = 48.0 };
            for (var b = 0; b < bays; b++)
            {
                var bay = new SelectiveBay { BeamLength = 100.0, Height = 240.0 };
                bay.Levels.Add(new SelectiveLevel { Y = 48.0, BeamId = TestCatalogIds.Profiles.Beams.SelectiveThreeRivet, BeamPeralte = 4.0 });
                bay.Levels.Add(new SelectiveLevel { Y = 96.0, BeamId = TestCatalogIds.Profiles.Beams.SelectiveThreeRivet, BeamPeralte = 4.0 });
                system.Bays.Add(bay);
            }

            return SelectiveBomBuilder.Build(system, Catalog);
        }

        public static BillOfMaterials Dynamic()
        {
            var catalog = Catalog;
            var system = new DynamicRackSystemResolver(catalog).Resolve(new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 3,
                FirstLevelHeight = 6.0,
                BeamDepth = 6.0,
                HeaderPostCatalogId = "POSTE_OMEGA_3X3"
            }).System;
            return SystemBomBuilder.Build(system, catalog);
        }

        public static BillOfMaterials PushBack()
        {
            var catalog = Catalog;
            var structure = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 3,
                FirstLevelHeight = 6.0,
                BeamDepth = 4.0
            };
            structure.SafetySelections.Add(new SelectiveSafetySelection { ElementId = Bota, Quantity = 1, Side = SafetySide.Both });
            return PushBackBomBuilder.Build(new PushBackResolver(catalog).Resolve(new PushBackDesign { Structure = structure }), catalog);
        }

        public static BillOfMaterials Cantilever(int punchCount)
        {
            var columnBase = CantileverColumnBaseSectionPolicy.Create(
                new[] { new CantileverColumnBaseVariant(CantileverColumnBaseVariantKind.WFlangeConnected, StructuralSectionId.Parse(ColumnW), StructuralSectionId.Parse(BaseW)) },
                new[] { StructuralSectionFamily.W });
            var arms = CantileverArmSectionPolicy.Create(new[] { new CantileverArmVariant(StructuralSectionId.Parse(ArmHss), CantileverArmBodyArrangement.Single) });

            var topology = new CantileverLineStationTopologyDesign
            {
                FaceMode = CantileverStationFaceMode.Single,
                SingleSide = CantileverArmSide.PositiveY,
                LevelCount = 3,
                RequestedClearHeight = 24.0,
                ColumnHeight = new CantileverStationColumnHeightDesign { Mode = CantileverStationColumnHeightMode.Automatic },
                ColumnBaseTemplate = new CantileverStationColumnBaseTemplateDesign
                {
                    ColumnSectionId = ColumnW,
                    Base = new CantileverBaseDesign { SectionId = BaseW, Length = 48.0 }
                }
            };
            topology.ColumnBaseTemplate.Connection.Punches.ColumnBottomPlateEndOffset = 1.5;
            topology.ColumnBaseTemplate.Connection.Punches.ColumnTopPunchOffset = 4.0;

            var design = new CantileverLineDesign
            {
                Name = "Linea",
                StationCount = 3,
                ColumnCentreSpacing = 96.0,
                StationTopology = topology,
                DefaultArmTemplate = new CantileverArmTemplateDesign
                {
                    Body = new CantileverArmBodyDesign { Arrangement = CantileverArmBodyArrangement.Single, SectionId = ArmHss, CutLength = 36.0, SlopeRisePer12 = 0.0 },
                    MountingPlate = new CantileverArmMountingPlateTemplateDesign { VerticalPunchCount = punchCount, VerticalEndOffset = 1.5 },
                    EndPlate = new CantileverArmEndPlateDesign { Mode = CantileverArmEndPlateMode.None }
                },
                Bracing = new CantileverBracingDesign()
            };

            return CantileverLineBomBuilder.Build(CantileverLineResolver.Resolve(design, Sections, new StructuralSectionGeometryFactory(Sections), columnBase, arms));
        }

        public static BillOfMaterials Cabecera()
        {
            var configuration = new RackFrameConfigurationFactory(Catalog)
                .Build(RackFrameTemplateCatalog.Default, TestCatalogIds.Profiles.Posts.Standard, 132.0, 42.0);
            new BracingPanelMemberBuilder().RefreshPhysicalModel(configuration);
            return BomBuilder.Build(configuration, Catalog);
        }

        public static BillOfMaterials Cama()
        {
            HeaderBlockInstance Piece(HeaderBlockRole role, string id) => new HeaderBlockInstance { Role = role, PieceId = id, Insertion = new Point2D(0, 0) };
            var rail = Piece(HeaderBlockRole.Rail, "RIEL");
            rail.DynamicParameters["LONGITUD"] = 240.0;
            var instances = new List<HeaderBlockInstance>
            {
                rail,
                Piece(HeaderBlockRole.Stop, "TOPE"),
                Piece(HeaderBlockRole.Roller, "ROD"),
                Piece(HeaderBlockRole.Roller, "ROD"),
                Piece(HeaderBlockRole.Brake, "FRENO")
            };

            return FlowBedBomBuilder.Build(instances, null);
        }

        public static ConsolidatedBom AllTogether(int copies)
            => ConsolidatedBomBuilder.Build(Names.Select(name => Rack(name, copies)).ToList());
    }
}
