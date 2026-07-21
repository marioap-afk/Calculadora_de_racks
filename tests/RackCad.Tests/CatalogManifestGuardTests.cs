using System;
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
    /// Defects 2–4: the builder→manifest guard is an EXACT comparison keyed by the full
    /// <c>PieceId + View + BlockName</c> triple. It runs the REAL production builders for every family that
    /// writes dynamic parameters (including the manual per-cabecera plate PERALTE override), groups the
    /// parameters they actually write per triple, and asserts each triple's set equals BOTH
    /// <see cref="CatalogBlockParameters.ExpectedParameters"/> and the exact manifest entry for that block —
    /// catching a missing parameter AND an over-declared one. A parametrized instance with a blank block name,
    /// or one whose block has no manifest entry, fails. Family coverage is a declared matrix (keyed by
    /// PieceId + View + Parameter) checked in BOTH directions: every declared case is observed, and every
    /// observed parameter-bearing case is declared.
    /// </summary>
    public class CatalogManifestGuardTests
    {
        private const string LO = SelectiveRackDefaults.LengthParam;       // LONGITUD
        private const string PE = SelectiveRackDefaults.PeralteParam;      // PERALTE
        private const string AL = SelectiveRackDefaults.PalletAltoParam;   // ALTURA
        private const string SA = SelectiveSafetyDefaults.SaqueParam;      // SAQUE
        private const string FR = SelectiveSafetyDefaults.ParrillaFrenteParam; // FRENTE
        private const string FO = SelectiveSafetyDefaults.ParrillaFondoParam;  // FONDO

        private const string PostId = TestCatalogIds.Profiles.Posts.Standard;
        private const string BeamId = TestCatalogIds.Profiles.Beams.SelectiveThreeRivet;

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        /// <summary>The canonical family/view cases the production builders must keep writing, with the EXACT
        /// parameter set observed. Keyed by PieceId so distinct safety families do not collapse into one row.
        /// Empty rows are relevant contracts declared on purpose (the in/out beam draws in LATERAL with none).</summary>
        private static readonly (string Piece, string View, string[] Params)[] Coverage =
        {
            (PostId, "FRONTAL", new[] { LO, PE }),
            (PostId, "LATERAL", new[] { LO }),
            (PostId, "PLANTA", new[] { PE }),
            (TestCatalogIds.BasePlates.Standard, "FRONTAL", new[] { PE }),
            (TestCatalogIds.BasePlates.Standard, "LATERAL", new[] { PE }),   // via the manual peralte override
            (TestCatalogIds.BasePlates.Standard, "PLANTA", new[] { PE }),
            (TestCatalogIds.Profiles.Truss.Standard, "LATERAL", new[] { LO }),
            (TestCatalogIds.Profiles.Truss.Standard, "PLANTA", new[] { LO, PE }),
            (BeamId, "FRONTAL", new[] { LO, PE }),
            (BeamId, "LATERAL", new[] { PE }),                               // regular larguero: PERALTE only
            (BeamId, "PLANTA", new[] { LO, PE }),
            (TestCatalogIds.Profiles.Beams.DynamicIntermediate, "LATERAL", new[] { PE }),
            (TestCatalogIds.Profiles.Beams.DynamicIntermediate, "PLANTA", new[] { LO, PE }),
            (TestCatalogIds.Profiles.Beams.DynamicInOut, "FRONTAL", new[] { LO, PE }),
            (TestCatalogIds.Profiles.Beams.DynamicInOut, "LATERAL", new string[0]), // in/out: NOTHING in LATERAL
            (TestCatalogIds.Profiles.Beams.DynamicInOut, "PLANTA", new[] { LO, PE }),
            (TestCatalogIds.Profiles.Spacers.Header, "FRONTAL", new[] { LO }),
            (TestCatalogIds.Profiles.Spacers.Header, "PLANTA", new[] { LO }),
            (TestCatalogIds.FlowBed.Rail, "LATERAL", new[] { LO }),
            (TestCatalogIds.BlockOnlyPieces.Pallet, "FRONTAL", new[] { LO, AL }),
            (TestCatalogIds.BlockOnlyPieces.Pallet, "LATERAL", new[] { LO, AL }),
            (TestCatalogIds.Safety.Stops.Beam, "FRONTAL", new[] { LO, SA }),
            (TestCatalogIds.Safety.Stops.Beam, "LATERAL", new[] { SA }),
            (TestCatalogIds.Safety.Stops.Beam, "PLANTA", new[] { LO, SA }),
            (TestCatalogIds.Safety.Decks.Generic, "FRONTAL", new[] { FR }),
            (TestCatalogIds.Safety.Decks.Generic, "LATERAL", new[] { FO }),
            (TestCatalogIds.Safety.SideProtectors.H3_16_18, "FRONTAL", new[] { LO }),
            (TestCatalogIds.Safety.SideProtectors.H3_16_18, "LATERAL", new[] { LO }),
            (TestCatalogIds.Safety.SideProtectors.H3_16_18, "PLANTA", new[] { LO }),
            (TestCatalogIds.Safety.Deviators.A3, "FRONTAL", new[] { LO }),
            (TestCatalogIds.Safety.Deviators.A3, "LATERAL", new[] { LO }),
            (TestCatalogIds.Safety.Deviators.A3, "PLANTA", new[] { LO }),
            (TestCatalogIds.Safety.Dynamic.EntranceGuide, "FRONTAL", new[] { LO }),
            (TestCatalogIds.Safety.Dynamic.EntranceGuide, "LATERAL", new[] { LO }),
            (TestCatalogIds.Safety.Dynamic.EntranceGuide, "PLANTA", new[] { LO }),
            (TestCatalogIds.Safety.Dynamic.ForkliftDefense, "FRONTAL", new[] { LO }),
            (TestCatalogIds.Safety.Dynamic.ForkliftDefense, "LATERAL", new[] { LO }),
            (TestCatalogIds.Safety.Dynamic.ForkliftDefense, "PLANTA", new[] { LO })
        };

        [Fact]
        public void Guard_ObservedParameters_ExactlyEqualExpectedAndManifest_PerTriple()
        {
            var catalog = Catalog;
            var manifest = CatalogBlockManifest.BuildExpected(catalog);
            var byBlock = manifest.Blocks
                .GroupBy(block => block.BlockName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => new HashSet<string>(group.First().Parameters, StringComparer.OrdinalIgnoreCase),
                    StringComparer.OrdinalIgnoreCase);

            // Key by the EXACT PieceId + View + BlockName triple — never merge several block names into one entry.
            var observed = new Dictionary<(string Piece, string View, string Block), HashSet<string>>();
            var failures = new List<string>();

            foreach (var instance in AllInstances(catalog))
            {
                if (instance == null || string.IsNullOrWhiteSpace(instance.PieceId) || string.IsNullOrWhiteSpace(instance.View))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(instance.BlockName))
                {
                    // Defect 4: a parametrized instance with a blank block name cannot be validated.
                    if (instance.DynamicParameters.Count > 0)
                    {
                        failures.Add(instance.PieceId + " @ " + instance.View + ": parametrized instance with EMPTY BlockName");
                    }

                    continue;
                }

                var key = (instance.PieceId.Trim(), instance.View.Trim(), instance.BlockName.Trim());
                if (!observed.TryGetValue(key, out var set))
                {
                    set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    observed[key] = set;
                }

                foreach (var parameter in instance.DynamicParameters.Keys)
                {
                    set.Add(parameter);
                }
            }

            foreach (var pair in observed)
            {
                var (piece, view, block) = pair.Key;
                var label = piece + " | " + view + " | " + block;
                var observedParams = pair.Value;

                var expected = new HashSet<string>(CatalogBlockParameters.ExpectedParameters(catalog, piece, view), StringComparer.OrdinalIgnoreCase);
                if (!expected.SetEquals(observedParams))
                {
                    failures.Add(label + ": observed=[" + Join(observedParams) + "] != ExpectedParameters=[" + Join(expected) + "]");
                }

                if (!byBlock.TryGetValue(block, out var manifestParams))
                {
                    // Defect 4: do NOT skip when the block is absent from the manifest.
                    if (observedParams.Count > 0)
                    {
                        failures.Add(label + ": block has NO manifest entry");
                    }

                    continue;
                }

                if (!manifestParams.SetEquals(observedParams))
                {
                    failures.Add(label + ": observed=[" + Join(observedParams) + "] != manifest=[" + Join(manifestParams) + "]");
                }
            }

            Assert.True(failures.Count == 0, string.Join("\n", failures.Distinct().OrderBy(x => x, StringComparer.Ordinal)));
        }

        [Fact]
        public void Guard_CoverageMatrix_MatchesObservedInBothDirections()
        {
            var catalog = Catalog;
            var observed = ObservedByPieceView(catalog);

            var declared = new HashSet<string>(Coverage.Select(row => row.Piece + " | " + row.View), StringComparer.Ordinal);
            var problems = new List<string>();

            // 1. Every declared family/view is observed with exactly the declared parameters.
            foreach (var (piece, view, expectedParams) in Coverage)
            {
                var key = piece + " | " + view;
                if (!observed.TryGetValue(key, out var got))
                {
                    problems.Add(key + ": NOT observed by any production builder");
                    continue;
                }

                var expected = new HashSet<string>(expectedParams, StringComparer.OrdinalIgnoreCase);
                if (!expected.SetEquals(got))
                {
                    problems.Add(key + ": observed=[" + Join(got) + "] != declared=[" + Join(expected) + "]");
                }
            }

            // 2. Every observed parameter-bearing case is declared (the matrix cannot silently miss a family).
            foreach (var pair in observed)
            {
                if (pair.Value.Count > 0 && !declared.Contains(pair.Key))
                {
                    problems.Add(pair.Key + ": observed=[" + Join(pair.Value) + "] is NOT declared in the coverage matrix");
                }
            }

            Assert.True(problems.Count == 0, string.Join("\n", problems.OrderBy(x => x, StringComparer.Ordinal)));
        }

        [Fact]
        public void Manifest_DoesNotImposeFalseRequirements()
        {
            var catalog = Catalog;

            // Relevant empty contracts declared explicitly (not left to accidental absence):
            // separator has no LATERAL block written; the in/out beam draws in LATERAL with no parameter.
            Assert.Empty(CatalogBlockParameters.ExpectedParameters(catalog, TestCatalogIds.Profiles.Spacers.Header, "LATERAL"));
            Assert.Empty(CatalogBlockParameters.ExpectedParameters(catalog, TestCatalogIds.Profiles.Beams.DynamicInOut, "LATERAL"));

            // Ménsulas are fixed end connectors; botas are fixed protectors — no dynamic parameter in any view.
            foreach (var view in new[] { "FRONTAL", "LATERAL", "PLANTA" })
            {
                Assert.Empty(CatalogBlockParameters.ExpectedParameters(catalog, TestCatalogIds.Mensulas.ThreeRivet, view));
                Assert.Empty(CatalogBlockParameters.ExpectedParameters(catalog, TestCatalogIds.Safety.Boots.H3_16_18, view));
            }
        }

        private static Dictionary<string, HashSet<string>> ObservedByPieceView(RackCatalog catalog)
        {
            var observed = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
            foreach (var instance in AllInstances(catalog))
            {
                if (instance == null || string.IsNullOrWhiteSpace(instance.PieceId) || string.IsNullOrWhiteSpace(instance.View))
                {
                    continue;
                }

                var key = instance.PieceId.Trim() + " | " + instance.View.Trim();
                if (!observed.TryGetValue(key, out var set))
                {
                    set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    observed[key] = set;
                }

                foreach (var parameter in instance.DynamicParameters.Keys)
                {
                    set.Add(parameter);
                }
            }

            return observed;
        }

        private static string Join(IEnumerable<string> values) =>
            string.Join(",", values.OrderBy(v => v, StringComparer.OrdinalIgnoreCase));

        // ---- Instance collection from the real production builders (focused designs per family) --------------

        private static List<HeaderBlockInstance> AllInstances(RackCatalog catalog)
        {
            var list = new List<HeaderBlockInstance>();
            list.AddRange(SelectiveInstances(catalog, BaseDesign()));            // poste, placa, larguero, separador, tarima, celosía
            list.AddRange(SelectiveInstances(catalog, TopeDesign()));            // tope
            list.AddRange(SelectiveInstances(catalog, LateralProtectorDesign())); // protector lateral, bota
            list.AddRange(SelectiveInstances(catalog, DesviadorDesign()));       // desviador
            list.AddRange(SelectiveInstances(catalog, ParrillaDesign()));        // parrilla
            list.AddRange(DynamicInstances(catalog));                            // in/out beam, intermediate beam, rail, guía, defensa
            list.AddRange(HeaderInstances(catalog));                             // poste, placa, celosía
            list.AddRange(HeaderInstancesWithPlateOverride(catalog));            // placa LATERAL con PERALTE (override manual)
            list.AddRange(FlowBedInstances(catalog));                            // riel
            return list;
        }

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
                list.AddRange(new LateralHeaderLayoutBuilder()
                    .Build(configuration, LateralHeaderParametersFactory.FromConfiguration(configuration), catalog)
                    .Instances);
            }

            return list;
        }

        private static SelectiveBayDesign Bay(int quantity)
        {
            var bay = new SelectiveBayDesign { FloorBeam = true };
            bay.Levels.Add(new SelectiveCell { Pallet = new Tarima { Frente = 42.0, Alto = 60.0 }, PalletCount = quantity, BeamId = BeamId, BeamPeralte = 4.0 });
            return bay;
        }

        private static SelectivePalletDesign PerFondo()
        {
            var design = new SelectivePalletDesign { PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0, PalletDepth = 48.0, DepthCount = 2, DrawPallets = true };
            design.SeparatorLengths.Add(8.0);
            design.Bays.Add(Bay(3));
            design.Bays.Add(Bay(3));
            design.ExtraFondoBays.Add(new List<SelectiveBayDesign> { Bay(2), Bay(2) });
            return design;
        }

        private static SelectivePalletDesign BaseDesign() => PerFondo();

        private static SelectivePalletDesign TopeDesign()
        {
            var design = PerFondo();
            design.SafetySelections.Add(new SelectiveSafetySelection { ElementId = TestCatalogIds.Safety.Stops.Beam, Side = SafetySide.Both, TopeFrontal = true, TopeSaque = 3.0 });
            return design;
        }

        private static SelectivePalletDesign LateralProtectorDesign()
        {
            var design = PerFondo();
            var lateral = new SelectiveSafetySelection { ElementId = TestCatalogIds.Safety.SideProtectors.H3_16_18, Side = SafetySide.None };
            lateral.PostSides.Add(new SafetyPostSide { PostIndex = 0, Side = SafetySide.Left });
            design.SafetySelections.Add(lateral);
            design.SafetySelections.Add(new SelectiveSafetySelection { ElementId = TestCatalogIds.Safety.Boots.H3_16_18, Quantity = 1, Side = SafetySide.Left });
            return design;
        }

        private static SelectivePalletDesign DesviadorDesign()
        {
            var design = PerFondo();
            design.SafetySelections.Add(new SelectiveSafetySelection { ElementId = TestCatalogIds.Safety.Deviators.A3, Quantity = 1, Side = SafetySide.Both, DesviadorLongitud = 18.0, DesviadorPrimerNivelAltura = 18.0 });
            return design;
        }

        private static SelectivePalletDesign ParrillaDesign()
        {
            var design = new SelectivePalletDesign { PostId = PostId, PostPeralte = 3.0, PalletDepth = 48.0, DrawPallets = true };
            var bay = new SelectiveBayDesign { FloorBeam = true };
            bay.Levels.Add(new SelectiveCell { Pallet = new Tarima { Frente = 40.0, Alto = 45.0 }, PalletCount = 2, BeamId = BeamId, BeamPeralte = 4.0 });
            design.Bays.Add(bay);
            design.SafetySelections.Add(new SelectiveSafetySelection { ElementId = TestCatalogIds.Safety.Decks.Generic, Side = SafetySide.Both, Quantity = 1, ParrillaFrontal = true, ParrillaLateral = true });
            return design;
        }

        private static IEnumerable<HeaderBlockInstance> DynamicInstances(RackCatalog catalog)
        {
            var design = new DynamicRackDesign { Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"), PalletsDeep = 4, LoadLevels = 3, FirstLevelHeight = 6.0, BeamDepth = 6.0 };
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

        private static IEnumerable<HeaderBlockInstance> HeaderInstances(RackCatalog catalog)
        {
            var list = new List<HeaderBlockInstance>();
            list.AddRange(new PlantaHeaderLayoutBuilder().Build(HeaderConfig(catalog), catalog));
            var configuration = HeaderConfig(catalog);
            list.AddRange(new LateralHeaderLayoutBuilder()
                .Build(configuration, LateralHeaderParametersFactory.FromConfiguration(configuration), catalog)
                .Instances);
            return list;
        }

        /// <summary>The manual per-cabecera plate PERALTE override — a supported production path — makes the
        /// LATERAL plate carry PERALTE. Runs the REAL <see cref="LateralHeaderLayoutBuilder"/>.</summary>
        private static IEnumerable<HeaderBlockInstance> HeaderInstancesWithPlateOverride(RackCatalog catalog)
        {
            var configuration = HeaderConfig(catalog);
            configuration.LeftBasePlate ??= new BasePlatePlacement();
            configuration.LeftBasePlate.PeralteOverride = 4.0;
            configuration.RightBasePlate ??= new BasePlatePlacement();
            configuration.RightBasePlate.PeralteOverride = 4.0;
            return new LateralHeaderLayoutBuilder()
                .Build(configuration, LateralHeaderParametersFactory.FromConfiguration(configuration), catalog)
                .Instances;
        }

        private static RackFrameConfiguration HeaderConfig(RackCatalog catalog)
        {
            var template = RackFrameTemplateCatalog.FindById(TestCatalogIds.Templates.Standard) ?? RackFrameTemplateCatalog.Default;
            return new RackFrameConfigurationFactory(catalog).Build(template, PostId, 132.0, 48.0);
        }

        private static IEnumerable<HeaderBlockInstance> FlowBedInstances(RackCatalog catalog)
        {
            return new FlowBedLateralBuilder().Build(
                new FlowBedConfiguration { BedType = FlowBedType.Dynamic, LaneDepth = 140.0, PalletDepth = 40.0, RollerId = TestCatalogIds.FlowBed.Roller1Point9 },
                catalog);
        }
    }
}
