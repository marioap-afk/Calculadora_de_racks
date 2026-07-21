using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Catalogs.Validation;
using RackCad.Application.Headers;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Defect 3: the builder→manifest anti-divergence guard is NOT limited to the rail. It runs the REAL
    /// production builders for every family that writes dynamic parameters — poste, placa, larguero, celosía,
    /// separador, riel, tarima, tope, parrilla, protector lateral, desviador, guía y defensa — across the three
    /// views, and asserts every parameter key a builder actually writes to a block is expected by the manifest
    /// for that same PieceId + View + BlockName. Plus explicit regressions against FALSE requirements.
    /// </summary>
    public class CatalogManifestGuardTests
    {
        private const string Longitud = SelectiveRackDefaults.LengthParam;
        private const string PostId = TestCatalogIds.Profiles.Posts.Standard;
        private const string BeamId = TestCatalogIds.Profiles.Beams.SelectiveThreeRivet;

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        [Fact]
        public void Manifest_ExpectsEveryDynamicParameterProductionBuildersWrite()
        {
            var catalog = Catalog;
            var manifest = CatalogBlockManifest.BuildExpected(catalog);
            var byBlock = manifest.Blocks
                .GroupBy(block => block.BlockName, System.StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First().Parameters, System.StringComparer.OrdinalIgnoreCase);

            var instances = new List<HeaderBlockInstance>();
            instances.AddRange(SelectiveInstances(catalog, PerFondoSafetyDesign()));
            instances.AddRange(SelectiveInstances(catalog, ParrillaDesign()));
            instances.AddRange(DynamicInstances(catalog));
            instances.AddRange(HeaderInstances(catalog));
            instances.AddRange(FlowBedInstances(catalog));

            var failures = new List<string>();
            var covered = new HashSet<string>();

            foreach (var instance in instances)
            {
                if (instance == null
                    || instance.DynamicParameters.Count == 0
                    || string.IsNullOrWhiteSpace(instance.PieceId)
                    || string.IsNullOrWhiteSpace(instance.View))
                {
                    continue;
                }

                var expected = CatalogBlockParameters.ExpectedParameters(catalog, instance.PieceId, instance.View);

                foreach (var key in instance.DynamicParameters.Keys)
                {
                    covered.Add(instance.Role + "/" + instance.View + "/" + key);

                    if (!expected.Contains(key))
                    {
                        failures.Add(instance.Role + " " + instance.PieceId + " @ " + instance.View
                            + ": builder writes '" + key + "' but CatalogBlockParameters does not expect it");
                    }

                    // The same key must be present in the manifest entry for the block the builder inserts.
                    if (!string.IsNullOrWhiteSpace(instance.BlockName)
                        && byBlock.TryGetValue(instance.BlockName.Trim(), out var manifestParams)
                        && !manifestParams.Contains(key, System.StringComparer.OrdinalIgnoreCase))
                    {
                        failures.Add(instance.BlockName + " (" + instance.PieceId + " @ " + instance.View
                            + "): manifest entry lacks builder parameter '" + key + "'");
                    }
                }
            }

            Assert.True(failures.Count == 0, string.Join("\n", failures.Distinct().OrderBy(x => x)));

            // Breadth: the guard actually exercised the parametric families in more than one view (not just the rail).
            Assert.True(covered.Count >= 12,
                "guard exercised too few builder parameter/view combinations: " + covered.Count + "\n"
                + string.Join("\n", covered.OrderBy(x => x)));
        }

        [Fact]
        public void Manifest_DoesNotImposeFalseRequirements()
        {
            var catalog = Catalog;

            // Separator: no production builder writes its LONGITUD in a LATERAL block.
            Assert.Empty(CatalogBlockParameters.ExpectedParameters(catalog, TestCatalogIds.Profiles.Spacers.Header, "LATERAL"));

            // Ménsulas are fixed end connectors: no dynamic parameter in any view.
            foreach (var view in Views)
            {
                Assert.Empty(CatalogBlockParameters.ExpectedParameters(catalog, TestCatalogIds.Mensulas.ThreeRivet, view));
            }

            // Botas (protector de bota) are fixed blocks, unlike the LATERAL protector.
            foreach (var view in Views)
            {
                Assert.Empty(CatalogBlockParameters.ExpectedParameters(catalog, TestCatalogIds.Safety.Boots.H3_16_18, view));
            }
        }

        private static readonly string[] Views = { "FRONTAL", "LATERAL", "PLANTA" };

        // ---- Selective ---------------------------------------------------------------------------------

        private static IEnumerable<HeaderBlockInstance> SelectiveInstances(RackCatalog catalog, SelectivePalletDesign design)
        {
            var system = new SelectiveGeometryResolver().Resolve(design, catalog);
            var list = new List<HeaderBlockInstance>();

            list.AddRange(new SelectiveFrontalBuilder().Build(SelectiveDepthLayout.FondoSystemView(system, 0), catalog));
            list.AddRange(new SelectivePlantaBuilder().Build(system, catalog));

            foreach (var corte in new SelectiveLateralBuilder().Cortes(system, catalog))
            {
                list.AddRange(corte.Largueros);
                var configuration = corte.Cabecera;
                var layout = new LateralHeaderLayoutBuilder().Build(
                    configuration, LateralHeaderParametersFactory.FromConfiguration(configuration), catalog);
                list.AddRange(layout.Instances);
            }

            return list;
        }

        private static SelectiveBayDesign Bay(int quantity)
        {
            var bay = new SelectiveBayDesign();
            bay.Levels.Add(new SelectiveCell
            {
                Pallet = new Tarima { Frente = 42.0, Alto = 60.0 },
                PalletCount = quantity,
                BeamId = BeamId,
                BeamPeralte = 4.0
            });
            return bay;
        }

        private static SelectivePalletDesign PerFondoSafetyDesign()
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId,
                PostPeralte = 3.0,
                PalletTolerance = 4.0,
                VerticalClearance = 6.0,
                PalletDepth = 48.0,
                DepthCount = 2,
                DrawPallets = true
            };
            design.SeparatorLengths.Add(8.0);
            design.Bays.Add(Bay(3));
            design.Bays.Add(Bay(3));
            design.ExtraFondoBays.Add(new List<SelectiveBayDesign> { Bay(2), Bay(2) });

            // Tope (larguero tope): SAQUE + LONGITUD.
            design.SafetySelections.Add(new SelectiveSafetySelection
            {
                ElementId = TestCatalogIds.Safety.Stops.Beam,
                Side = SafetySide.Both,
                TopeFrontal = true,
                TopeSaque = 3.0
            });

            // Protector lateral (safety type LATERAL): LONGITUD in every view.
            var lateral = new SelectiveSafetySelection { ElementId = TestCatalogIds.Safety.SideProtectors.H3_16_18, Side = SafetySide.None };
            lateral.PostSides.Add(new SafetyPostSide { PostIndex = 0, Side = SafetySide.Left });
            design.SafetySelections.Add(lateral);

            // Bota (fixed, no dynamic param) — proves the guard tolerates families without parameters.
            design.SafetySelections.Add(new SelectiveSafetySelection
            {
                ElementId = TestCatalogIds.Safety.Boots.H3_16_18,
                Quantity = 1,
                Side = SafetySide.Left
            });

            // Desviador: LONGITUD.
            design.SafetySelections.Add(new SelectiveSafetySelection
            {
                ElementId = TestCatalogIds.Safety.Deviators.A3,
                Quantity = 1,
                Side = SafetySide.Both,
                DesviadorLongitud = 18.0,
                DesviadorPrimerNivelAltura = 18.0
            });

            return design;
        }

        private static SelectivePalletDesign ParrillaDesign()
        {
            var design = new SelectivePalletDesign { PostId = PostId, PostPeralte = 3.0, PalletDepth = 48.0, DrawPallets = true };
            var bay = new SelectiveBayDesign { FloorBeam = true };
            bay.Levels.Add(new SelectiveCell
            {
                Pallet = new Tarima { Frente = 40.0, Alto = 45.0 },
                PalletCount = 2,
                BeamId = BeamId,
                BeamPeralte = 4.0
            });
            design.Bays.Add(bay);
            design.SafetySelections.Add(new SelectiveSafetySelection
            {
                ElementId = TestCatalogIds.Safety.Decks.Generic,
                Side = SafetySide.Both,
                Quantity = 1,
                ParrillaFrontal = true,
                ParrillaLateral = true
            });
            return design;
        }

        // ---- Dynamic -----------------------------------------------------------------------------------

        private static IEnumerable<HeaderBlockInstance> DynamicInstances(RackCatalog catalog)
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

            var system = new DynamicRackSystemResolver(catalog).Resolve(design).System;
            foreach (var selection in DynamicSafetyDefaults.Build(catalog))
            {
                system.SafetySelections.Add(selection);
            }

            var list = new List<HeaderBlockInstance>();
            list.AddRange(new DynamicSystemFrontalBuilder().Build(system, catalog, DynamicRackEnd.Entrance));
            list.AddRange(new DynamicSystemFrontalBuilder().Build(system, catalog, DynamicRackEnd.Exit));
            list.AddRange(new DynamicSystemPlantaBuilder().Build(system, catalog));
            list.AddRange(new DynamicSystemLateralBuilder().Build(system, catalog).Flatten().Instances);
            return list;
        }

        // ---- Headers (celosía / posts / plates) --------------------------------------------------------

        private static IEnumerable<HeaderBlockInstance> HeaderInstances(RackCatalog catalog)
        {
            var template = RackFrameTemplateCatalog.FindById(TestCatalogIds.Templates.Standard) ?? RackFrameTemplateCatalog.Default;
            var configuration = new RackFrameConfigurationFactory(catalog).Build(template, PostId, 132.0, 48.0);

            var list = new List<HeaderBlockInstance>();
            list.AddRange(new PlantaHeaderLayoutBuilder().Build(configuration, catalog));
            list.AddRange(new LateralHeaderLayoutBuilder()
                .Build(configuration, LateralHeaderParametersFactory.FromConfiguration(configuration), catalog)
                .Instances);
            return list;
        }

        // ---- Flow bed rail -----------------------------------------------------------------------------

        private static IEnumerable<HeaderBlockInstance> FlowBedInstances(RackCatalog catalog)
        {
            return new FlowBedLateralBuilder().Build(
                new FlowBedConfiguration
                {
                    BedType = FlowBedType.Dynamic,
                    LaneDepth = 140.0,
                    PalletDepth = 40.0,
                    RollerId = TestCatalogIds.FlowBed.Roller1Point9
                },
                catalog);
        }
    }
}
