using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Characterization + equivalence tests for the dynamic editor's recompute core extracted to Application (I-21):
    /// the rebuild decision, header-fondo preservation, in-place header-height update and the design assembly. The final
    /// tests run the real pipeline (matrix -> BuildDesign -> resolver.Resolve) to prove the assembled design is a valid,
    /// self-consistent input to the same resolver the drawing/BOM consume — i.e. the extraction preserves behavior.
    /// </summary>
    public class DynamicEditorDesignAssemblerTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();
        private const string PostId = "POSTE_OMEGA_3X3";

        private static PalletSpecification Pallet48()
            => new PalletSpecification(front: 42.0, depth: 48.0, height: 60.0, weight: 1000.0, weightUnit: "kg");

        private static (DynamicRackSystemBuilder Builder, DynamicRackSystemResolver Resolver, DynamicEditorDesignAssembler Assembler) Services()
        {
            var catalog = Catalog;
            var builder = new DynamicRackSystemBuilder(catalog);
            var resolver = new DynamicRackSystemResolver(catalog);
            var assembler = new DynamicEditorDesignAssembler(catalog, builder, resolver);
            return (builder, resolver, assembler);
        }

        private static DynamicRackSystem BuildSystem(DynamicRackSystemBuilder builder, int palletsDeep)
            => builder.BuildDefault(Pallet48(), palletsDeep, RackFrameTemplateCatalog.Default, PostId, 132.0, 3.0);

        [Fact]
        public void MustRebuild_SamePalletAndLayout_IsFalse()
        {
            var (builder, _, _) = Services();
            var system = BuildSystem(builder, 5);
            var layout = DynamicDepthGeometry.Resolve(system);

            Assert.False(DynamicEditorDesignAssembler.MustRebuild(system, system.Pallet, layout));
        }

        [Fact]
        public void MustRebuild_DifferentPallet_IsTrue()
        {
            var (builder, _, _) = Services();
            var system = BuildSystem(builder, 5);
            var layout = DynamicDepthGeometry.Resolve(system);
            var otherPallet = new PalletSpecification(front: 40.0, depth: 48.0, height: 60.0, weight: 1000.0, weightUnit: "kg");

            Assert.True(DynamicEditorDesignAssembler.MustRebuild(system, otherPallet, layout));
        }

        [Fact]
        public void MustRebuild_NullOrEmptySystem_IsTrue()
        {
            var (builder, _, _) = Services();
            var system = BuildSystem(builder, 5);
            var layout = DynamicDepthGeometry.Resolve(system);

            Assert.True(DynamicEditorDesignAssembler.MustRebuild(null, system.Pallet, layout));
            Assert.True(DynamicEditorDesignAssembler.MustRebuild(new DynamicRackSystem(), system.Pallet, layout));
        }

        [Fact]
        public void SnapshotAndRestoreHeaderFondos_PreservesCustomFondosByHeaderOrder()
        {
            var (builder, _, assembler) = Services();
            var system = BuildSystem(builder, 5); // headers at positions 1, 3, 5

            var headers = system.Modules.Where(m => m.IsHeader).ToList();
            Assert.True(headers.Count >= 2);
            // Give the SECOND header a custom fondo.
            headers[1].IsManualOverride = true;
            headers[1].Length = 61.0;

            var snapshot = assembler.SnapshotHeaderFondos(system);
            Assert.Equal(headers.Count, snapshot.Count);
            Assert.Null(snapshot[0]);
            Assert.Equal(61.0, snapshot[1]);

            // Rebuild a fresh standard system and restore the snapshot onto it.
            var rebuilt = BuildSystem(builder, 5);
            var restored = assembler.RestoreHeaderFondos(rebuilt, snapshot, 132.0, PostId);

            Assert.Equal(1, restored);
            var rebuiltHeaders = rebuilt.Modules.Where(m => m.IsHeader).ToList();
            Assert.Equal(61.0, rebuiltHeaders[1].Length);
            Assert.True(rebuiltHeaders[1].IsManualOverride);
            Assert.False(rebuiltHeaders[0].IsManualOverride);
        }

        [Fact]
        public void UpdateHeaderHeightInPlace_RebuildsCalculatedHeaders_LeavesCustomOnes()
        {
            var (builder, _, assembler) = Services();
            var system = BuildSystem(builder, 5);
            var headers = system.Modules.Where(m => m.IsHeader).ToList();

            // Mark the first header as custom (must NOT be touched); the rest stay calculated.
            headers[0].UseCalculatedHeaderConfiguration = false;
            var customHeightBefore = headers[0].AssociatedFrameConfiguration.Height;

            assembler.UpdateHeaderHeightInPlace(system, 150.0, PostId);

            Assert.Equal(customHeightBefore, headers[0].AssociatedFrameConfiguration.Height); // untouched
            foreach (var calculated in headers.Skip(1))
            {
                Assert.Equal(150.0, calculated.AssociatedFrameConfiguration.Height, 3);
            }
        }

        [Fact]
        public void BuildDesign_SetsScalarInputsAndAnnotations_AndCopiesOnlyDrawableSafety()
        {
            var (builder, _, assembler) = Services();
            var system = BuildSystem(builder, 5);
            var matrix = new DynamicFrontMatrix();

            var annotations = new DynamicAnnotationOptions
            {
                NumberFronts = true,
                NumberLevels = true,
                DrawRackName = true,
                AnnotationScale = 1.5,
                Dimensions = DimensionDetail.Detailed,
                DimensionStyle = "MI-ESTILO"
            };
            var safety = new List<SelectiveSafetySelection>
            {
                new SelectiveSafetySelection { ElementId = "DRAWS", Quantity = 1, Side = SafetySide.None },
                new SelectiveSafetySelection { ElementId = "SILENT", Quantity = 0, Side = SafetySide.None }
            };

            var design = assembler.BuildDesign(
                system, matrix,
                levels: 3, firstLevel: 6.0, beamDepth: DynamicRackDefaults.DefaultBeamDepth,
                headerPostCatalogId: PostId,
                palletsDeep: 5, postPeralte: 3.0, palletTolerance: DynamicRackDefaults.DefaultPalletTolerance,
                annotations: annotations, safetySelections: safety);

            Assert.Equal(5, design.PalletsDeep);
            Assert.Equal(3.0, design.PostPeralte);
            Assert.Equal(DynamicRackDefaults.DefaultPalletTolerance, design.PalletTolerance);
            Assert.True(design.NumberFronts);
            Assert.True(design.NumberLevels);
            Assert.True(design.DrawRackName);
            Assert.Equal(1.5, design.AnnotationScale);
            Assert.Equal(DimensionDetail.Detailed, design.Dimensions);
            Assert.Equal("MI-ESTILO", design.DimensionStyle);
            Assert.Single(design.SafetySelections);
            Assert.Equal("DRAWS", design.SafetySelections[0].ElementId);
        }

        [Fact]
        public void BuildDesign_FrontsComeFromTheMatrix()
        {
            var (builder, _, assembler) = Services();
            var system = BuildSystem(builder, 5);
            var matrix = new DynamicFrontMatrix();
            matrix.SetFrontCount(3);
            matrix.Fronts[1].PalletCount = 4;

            var design = assembler.BuildDesign(
                system, matrix,
                levels: 3, firstLevel: 6.0, beamDepth: DynamicRackDefaults.DefaultBeamDepth,
                headerPostCatalogId: PostId,
                palletsDeep: 5, postPeralte: 3.0, palletTolerance: DynamicRackDefaults.DefaultPalletTolerance,
                annotations: new DynamicAnnotationOptions(), safetySelections: null);

            var expected = matrix.BuildFrontDesigns();
            Assert.Equal(expected.Count, design.Fronts.Count);
            Assert.Equal(expected.Select(f => f.PalletCount).ToArray(), design.Fronts.Select(f => f.PalletCount).ToArray());
        }

        [Fact]
        public void BuildDesign_ThenResolve_ProducesAConsistentSystem()
        {
            // Equivalence with the real pipeline: the assembled design must resolve back into a system with the same
            // number of fronts, exactly as the window's Recompose relied on (design -> resolver -> system -> drawing/BOM).
            var (builder, resolver, assembler) = Services();
            var system = BuildSystem(builder, 5);
            var matrix = new DynamicFrontMatrix();
            matrix.SetFrontCount(2);

            var design = assembler.BuildDesign(
                system, matrix,
                levels: 3, firstLevel: 6.0, beamDepth: DynamicRackDefaults.DefaultBeamDepth,
                headerPostCatalogId: PostId,
                palletsDeep: 5, postPeralte: 3.0, palletTolerance: DynamicRackDefaults.DefaultPalletTolerance,
                annotations: new DynamicAnnotationOptions(), safetySelections: null);

            var resolution = resolver.Resolve(design);

            // The resolver owns the depth layout (PalletsDeep is derived from the fronts, not the legacy scalar), so we
            // assert the invariants the drawing/BOM rely on: a system exists, the front count survives, and it has real
            // geometry. This proves the assembled design is valid, self-consistent input to the same resolver.
            Assert.NotNull(resolution.System);
            Assert.Equal(matrix.Count, resolution.System.Fronts.Count);
            Assert.NotEmpty(resolution.System.Modules);
            Assert.True(resolution.System.TotalLength > 0.0);
        }
    }
}
