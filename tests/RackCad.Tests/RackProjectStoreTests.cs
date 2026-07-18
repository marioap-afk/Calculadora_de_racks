using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    public class RackProjectStoreTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static DynamicRackSystem DynamicSystem()
        {
            return new DynamicRackSystemBuilder(Catalog).BuildDefault(
                new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                palletsDeep: 4,
                headerTemplate: RackFrameTemplateCatalog.Default,
                headerPostCatalogId: "POSTE_OMEGA_3X3",
                headerHeight: 132.0);
        }

        private static RackFrameConfiguration SelectiveHeader()
        {
            return new HardcodedStandardRackFrameService(Catalog).CreateDefault();
        }

        [Fact]
        public void RoundTrip_DynamicSystem_PreservesPalletHeaderAndKind()
        {
            var store = new RackProjectStore();

            var loaded = store.Deserialize(store.Serialize(RackProject.ForDynamic(DynamicSystem())));

            Assert.Equal(RackSystemKind.PalletFlow, loaded.Kind);
            var system = loaded.DynamicSystem;
            Assert.NotNull(system);
            Assert.Equal(48.0, system.Pallet.Depth);
            Assert.Equal(42.0, system.Pallet.Front);
            Assert.Equal(1000.0, system.Pallet.Weight);
            Assert.Equal("kg", system.Pallet.WeightUnit);
            Assert.Equal(4, system.PalletsDeep);
            Assert.Equal(4, system.LengthBearingModuleCount);

            var startHeader = system.Modules.First();
            Assert.Equal(DynamicRackModuleKind.HeaderStart, startHeader.Kind);
            Assert.Equal(54.0, startHeader.AssociatedFrameConfiguration.Depth);
            Assert.Equal(5, startHeader.AssociatedFrameConfiguration.Horizontals.Count);
            Assert.Equal(4, startHeader.AssociatedFrameConfiguration.BracingPanels.Count);
            Assert.NotEmpty(startHeader.AssociatedFrameConfiguration.Members); // members rebuilt on load
        }

        [Fact]
        public void RoundTrip_DynamicSystem_PreservesSeparatorAndDerivedPostOptions()
        {
            var store = new RackProjectStore();
            var original = DynamicSystem();
            original.SeparatorCountOverride = 4;
            original.SeparatorSpacingOverride = 50.0;
            original.DerivedPostReinforced = false;
            original.DerivedPostReinforcementHeight = 72.0;

            var loaded = store.Deserialize(store.Serialize(RackProject.ForDynamic(original))).DynamicSystem;

            Assert.Equal(4, loaded.SeparatorCountOverride);
            Assert.Equal(50.0, loaded.SeparatorSpacingOverride);
            Assert.False(loaded.DerivedPostReinforced);
            Assert.Equal(72.0, loaded.DerivedPostReinforcementHeight);
        }

        [Fact]
        public void RoundTrip_DynamicDesign_PreservesHeightInputsAndHeaderProvenance()
        {
            var resolver = new DynamicRackSystemResolver(Catalog);
            var system = DynamicSystem();
            system.Modules.First(m => m.IsHeader).UseCalculatedHeaderConfiguration = false;
            var design = resolver.Snapshot(system, 5, 18.0, 6.0, "POSTE_OMEGA_3X3");

            var loaded = new RackProjectStore()
                .Deserialize(new RackProjectStore().Serialize(RackProject.ForDynamic(design, system)));

            Assert.NotNull(loaded.DynamicDesign);
            Assert.Equal(5, loaded.DynamicDesign.LoadLevels);
            Assert.Equal(18.0, loaded.DynamicDesign.FirstLevelHeight);
            Assert.Equal(6.0, loaded.DynamicDesign.BeamDepth);
            Assert.Equal(TestCatalogIds.Profiles.Beams.DynamicInOut, loaded.DynamicDesign.InOutBeamCatalogId);
            Assert.Equal("POSTE_OMEGA_3X3", loaded.DynamicDesign.HeaderPostCatalogId);
            Assert.False(loaded.DynamicDesign.Modules.First(m => m.IsHeader).UseCalculatedHeaderConfiguration);
        }

        [Fact]
        public void RoundTrip_DynamicDesign_PreservesFrontsToleranceAndManualWidth()
        {
            var resolver = new DynamicRackSystemResolver(Catalog);
            var design = resolver.Snapshot(DynamicSystem(), 3, 6.0, 6.0, "POSTE_OMEGA_3X3");
            design.PalletTolerance = 5.0;
            design.Fronts.Clear();
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 3, BeamLengthOverride = 150.0 });

            var store = new RackProjectStore();
            var loaded = store.Deserialize(store.Serialize(RackProject.ForDynamic(design))).DynamicDesign;

            Assert.Equal(5.0, loaded.PalletTolerance);
            Assert.Equal(new[] { 1, 3 }, loaded.Fronts.Select(front => front.PalletCount));
            Assert.Null(loaded.Fronts[0].BeamLengthOverride);
            Assert.Equal(150.0, loaded.Fronts[1].BeamLengthOverride);
        }

        [Fact]
        public void RoundTrip_DynamicDesign_PreservesAnnotationsAndStoresResolvedBfr()
        {
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(40.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                PostPeralte = 4.5,
                NumberFronts = true,
                NumberLevels = true,
                DrawRackName = true,
                AnnotationScale = 1.5,
                Dimensions = DimensionDetail.Detailed,
                DimensionStyle = "RackCad"
            };
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 2, LoadLevels = 2 });

            var document = DynamicRackSystemDocument.From(design);
            var restored = document.ToDesign();
            var resolved = document.ToDomain();

            Assert.True(restored.NumberFronts);
            Assert.True(restored.NumberLevels);
            Assert.True(restored.DrawRackName);
            Assert.Equal(1.5, restored.AnnotationScale);
            Assert.Equal(DimensionDetail.Detailed, restored.Dimensions);
            Assert.Equal("RackCad", restored.DimensionStyle);
            Assert.Equal(4.5, restored.PostPeralte, 4);
            Assert.Equal(4.5, resolved.PostPeralte, 4);
            Assert.Equal(42.0, Assert.Single(document.Fronts).Bfr);
            Assert.Equal(42.0, Assert.Single(resolved.Fronts).Bfr);
            Assert.Equal(2, Assert.Single(resolved.Fronts).LoadLevels);
            Assert.Equal(90.0, Assert.Single(resolved.Fronts).BeamLength);

            document.NumberFronts = null;
            document.NumberLevels = null;
            document.DrawRackName = null;
            document.AnnotationScale = null;
            document.Dimensions = null;
            document.PostPeralte = null;
            var legacy = document.ToDesign();
            Assert.False(legacy.NumberFronts);
            Assert.False(legacy.NumberLevels);
            Assert.False(legacy.DrawRackName);
            Assert.Equal(1.0, legacy.AnnotationScale);
            Assert.Equal(DimensionDetail.None, legacy.Dimensions);
            Assert.Equal(0.0, legacy.PostPeralte);
            Assert.Equal(Catalog.PostProfiles.FindProfile(Catalog.Defaults.Post).Width,
                new DynamicRackSystemResolver(Catalog).Resolve(legacy).System.PostPeralte,
                4);
        }

        [Fact]
        public void RoundTrip_DynamicDesign_PreservesIntermediateBeamPeralteByLevel_WithLegacyFallback()
        {
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(40.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 3
            };
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

            var document = DynamicRackSystemDocument.From(design);
            var restored = document.ToDesign();
            var resolved = new DynamicRackSystemResolver(Catalog).Resolve(restored).System;

            Assert.Equal(new[] { 3.0, 4.0, 5.0 }, restored.Fronts[0].IntermediateBeamDepths);
            Assert.Equal(new[] { 3.5, 4.5, 6.0 }, restored.Fronts[1].IntermediateBeamDepths);
            Assert.Equal(new[] { 3.0, 4.0, 5.0 }, resolved.Fronts[0].IntermediateBeamDepths);
            Assert.Equal(new[] { 3.5, 4.5, 6.0 }, resolved.Fronts[1].IntermediateBeamDepths);

            // Compatibility with the immediately previous DLL: it wrote one rack-wide list.
            document.IntermediateBeamDepths = new List<double> { 3.0, 4.5, 6.0 };
            document.Fronts.ForEach(front => front.IntermediateBeamDepths = null);
            var previous = new DynamicRackSystemResolver(Catalog).Resolve(document.ToDesign()).System;
            Assert.All(previous.Fronts, front =>
                Assert.Equal(new[] { 3.0, 4.5, 6.0 }, front.IntermediateBeamDepths));

            document.IntermediateBeamDepths = null;
            var legacy = new DynamicRackSystemResolver(Catalog).Resolve(document.ToDesign()).System;
            Assert.All(legacy.Fronts, front => Assert.Equal(
                Enumerable.Repeat(DynamicRackDefaults.DefaultIntermediateBeamDepth, 3),
                front.IntermediateBeamDepths));
        }

        [Fact]
        public void RoundTrip_DynamicDesign_PreservesFrontAndCellContracts_WithLegacyFallbacks()
        {
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 2,
                FirstLevelHeight = 6.0,
                BeamDepth = 6.0
            };
            var front = new DynamicRackFrontDesign
            {
                PalletCount = 2,
                LoadLevels = 2,
                FirstLevelHeight = 18.0
            };
            front.Levels.Add(new DynamicRackLevelDesign
            {
                PalletFront = 40.0,
                PalletHeight = 55.0,
                PalletWeight = 900.0,
                ClearHeight = 5.0,
                InOutBeamCatalogId = TestCatalogIds.Profiles.Beams.DynamicInOut,
                InOutBeamDepth = 6.0,
                BeamLengthOverride = 100.0,
                IntermediateBeamCatalogId = TestCatalogIds.Profiles.Beams.DynamicIntermediate,
                IntermediateBeamDepth = 4.0
            });
            front.Levels.Add(new DynamicRackLevelDesign
            {
                PalletFront = 44.0,
                PalletHeight = 65.0,
                PalletWeight = 1100.0,
                ClearHeight = 7.0,
                InOutBeamCatalogId = TestCatalogIds.Profiles.Beams.DynamicInOut,
                InOutBeamDepth = 6.0,
                IntermediateBeamCatalogId = TestCatalogIds.Profiles.Beams.DynamicIntermediate,
                IntermediateBeamDepth = 5.0
            });
            design.Fronts.Add(front);

            var store = new RackProjectStore();
            var resolver = new DynamicRackSystemResolver(Catalog);
            var system = resolver.Resolve(design).System;
            design = resolver.Snapshot(system, 2, 6.0, 6.0, TestCatalogIds.Profiles.Posts.Standard);
            var restored = store.Deserialize(store.Serialize(RackProject.ForDynamic(design, system))).DynamicDesign;
            var restoredFront = Assert.Single(restored.Fronts);

            Assert.Equal(18.0, restoredFront.FirstLevelHeight);
            Assert.Equal(2, restoredFront.Levels.Count);
            Assert.Equal(40.0, restoredFront.Levels[0].PalletFront);
            Assert.Equal(100.0, restoredFront.Levels[0].BeamLengthOverride);
            Assert.Equal(5.0, restoredFront.Levels[1].IntermediateBeamDepth);

            var document = DynamicRackSystemDocument.From(design);
            document.Fronts[0].FirstLevelHeight = null;
            document.Fronts[0].Levels = null;
            var legacy = document.ToDesign();
            Assert.Null(legacy.Fronts[0].FirstLevelHeight);
            Assert.Empty(legacy.Fronts[0].Levels);
            var resolvedLegacy = new DynamicRackSystemResolver(Catalog).Resolve(legacy).System;
            Assert.Equal(6.0, resolvedLegacy.Fronts[0].FirstLevelHeight, 4);
            Assert.Equal(42.0, resolvedLegacy.Fronts[0].Levels[0].Pallet.Front, 4);
        }

        [Fact]
        public void RoundTrip_DynamicDesign_PreservesSafetyAndLegacyMissingSafetyIsEmpty()
        {
            var resolver = new DynamicRackSystemResolver(Catalog);
            var design = resolver.Snapshot(DynamicSystem(), 3, 6.0, 6.0, "POSTE_OMEGA_3X3");
            var safety = new SelectiveSafetySelection
            {
                ElementId = TestCatalogIds.Safety.Deviators.A3,
                Quantity = 1,
                Side = SafetySide.Both,
                DesviadorLongitud = 20.0,
                DesviadorPrimerNivelAltura = 22.0
            };
            safety.DesviadorOffCells.Add(new SelectiveGridCell { Frente = 0, Level = 1 });
            safety.DefensaPosts.Add(new SafetyPostDefense { PostIndex = 0, ExitLength = 0.0, EntranceLength = 0.0 });
            safety.DefensaPosts.Add(new SafetyPostDefense { PostIndex = 1, ExitLength = 48.0, EntranceLength = 24.0 });
            safety.GuiaEntradaOffCells.Add(new SelectiveGridCell { Frente = 1, Level = 1 });
            design.SafetySelections.Add(safety);

            var store = new RackProjectStore();
            var loaded = store.Deserialize(store.Serialize(RackProject.ForDynamic(design))).DynamicDesign;
            var restored = Assert.Single(loaded.SafetySelections);
            Assert.Equal(TestCatalogIds.Safety.Deviators.A3, restored.ElementId);
            Assert.Equal(20.0, restored.DesviadorLongitud);
            Assert.Equal(22.0, restored.DesviadorPrimerNivelAltura);
            Assert.Single(restored.DesviadorOffCells);
            Assert.Equal(2, restored.DefensaPosts.Count);
            Assert.Equal(0.0, restored.DefensaPosts[0].ExitLength);
            Assert.Equal(0.0, restored.DefensaPosts[0].EntranceLength);
            Assert.Equal(48.0, restored.DefensaPosts[1].ExitLength);
            Assert.Equal(24.0, restored.DefensaPosts[1].EntranceLength);
            Assert.Single(restored.GuiaEntradaOffCells);
            Assert.Equal(1, restored.GuiaEntradaOffCells[0].Frente);
            Assert.Equal(1, restored.GuiaEntradaOffCells[0].Level);

            var legacy = DynamicRackSystemDocument.From(design);
            legacy.SafetySelections = null;
            Assert.Empty(legacy.ToDesign().SafetySelections);
            Assert.Empty(legacy.ToDomain().SafetySelections);
        }

        [Fact]
        public void DynamicDocument_LegacyMissingHeightInputs_UsesHistoricalDefaults()
        {
            var legacy = DynamicRackSystemDocument.From(DynamicSystem());
            legacy.LoadLevels = null;
            legacy.FirstLevelHeight = null;
            legacy.BeamDepth = null;

            var design = legacy.ToDesign();

            Assert.Equal(DynamicRackDefaults.DefaultLoadLevels, design.LoadLevels);
            Assert.Equal(DynamicRackDefaults.DefaultFirstLevelHeight, design.FirstLevelHeight);
            Assert.Equal(DynamicRackDefaults.LegacyDefaultBeamDepth, design.BeamDepth);
            Assert.Equal(TestCatalogIds.Profiles.Beams.DynamicInOut, design.InOutBeamCatalogId);
        }

        [Fact]
        public void DynamicDocument_LegacyMissingFronts_UsesOneSinglePositionFront()
        {
            var legacy = DynamicRackSystemDocument.From(DynamicSystem());
            legacy.Fronts = null;
            legacy.PalletTolerance = null;

            var design = legacy.ToDesign();
            var system = legacy.ToDomain();

            Assert.Equal(DynamicRackDefaults.DefaultPalletTolerance, design.PalletTolerance);
            Assert.Equal(DynamicRackDefaults.DefaultPalletsWide, Assert.Single(design.Fronts).PalletCount);
            Assert.Equal(50.0, Assert.Single(system.Fronts).BeamLength);
        }

        [Fact]
        public void DynamicDocument_LegacyHeaderWithoutProvenance_IsPreservedAsCustom()
        {
            var legacy = DynamicRackSystemDocument.From(DynamicSystem());
            var header = legacy.Modules.First(m => m.Kind == DynamicRackModuleKind.HeaderStart);
            header.UseCalculatedHeaderConfiguration = null;
            header.IsManualOverride = false;
            header.Header.Height = 150.0;

            var design = legacy.ToDesign();
            design.LoadLevels = 5;
            var resolved = new DynamicRackSystemResolver(Catalog).Resolve(design).System;

            Assert.False(design.Modules.First(m => m.IsHeader).UseCalculatedHeaderConfiguration);
            Assert.Equal(150.0, resolved.Modules.First(m => m.IsHeader).AssociatedFrameConfiguration.Height, 4);
        }

        [Fact]
        public void RoundTrip_SelectiveHeader_PreservesHeader()
        {
            var store = new RackProjectStore();

            var loaded = store.Deserialize(store.Serialize(RackProject.ForSelective(SelectiveHeader())));

            Assert.Equal(RackSystemKind.Selective, loaded.Kind);
            Assert.NotNull(loaded.Header);
            Assert.Equal(5, loaded.Header.Horizontals.Count);
            Assert.NotEmpty(loaded.Header.Members);
        }

        [Fact]
        public void Load_LegacyBareHeaderFile_IsTreatedAsSelective()
        {
            // A file produced by the old single-header store has no "kind" property.
            var legacyJson = new RackFrameProjectStore().Serialize(SelectiveHeader());

            var loaded = new RackProjectStore().Deserialize(legacyJson);

            Assert.Equal(RackSystemKind.Selective, loaded.Kind);
            Assert.NotNull(loaded.Header);
            Assert.Equal(5, loaded.Header.Horizontals.Count);
        }

        [Fact]
        public void Deserialize_InvalidJson_Throws()
        {
            Assert.Throws<System.InvalidOperationException>(() => new RackProjectStore().Deserialize("{ not json"));
        }

        [Fact]
        public void Deserialize_EmptyObject_Throws()
        {
            // "{}" used to load as a header with height 0 (a degenerate, un-drawable rack); now it is rejected.
            Assert.Throws<System.InvalidOperationException>(() => new RackProjectStore().Deserialize("{}"));
        }

        [Fact]
        public void Deserialize_KindWithoutPayload_Throws()
        {
            // A wrapper that declares a type but omits its data is corrupt/truncated — fail clearly, not silently.
            Assert.Throws<System.InvalidOperationException>(() => new RackProjectStore().Deserialize("{\"kind\":\"Cama\"}"));
        }

        [Fact]
        public void Deserialize_FutureSchemaVersion_Throws()
        {
            var ex = Assert.Throws<System.InvalidOperationException>(
                () => new RackProjectStore().Deserialize("{\"schemaVersion\":\"99.0\",\"kind\":\"Selective\"}"));
            Assert.Contains("más nueva", ex.Message);
        }

        [Fact]
        public void SaveLoad_Dynamic_RoundTripsThroughDisk()
        {
            var store = new RackProjectStore();
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                "rackcad-sys-" + System.Guid.NewGuid().ToString("N") + RackProjectStore.FileExtension);

            try
            {
                store.Save(RackProject.ForDynamic(DynamicSystem()), path);
                var loaded = store.Load(path);
                Assert.Equal(RackSystemKind.PalletFlow, loaded.Kind);
                Assert.Equal(4, loaded.DynamicSystem.PalletsDeep);
            }
            finally
            {
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }
        }
    }
}
