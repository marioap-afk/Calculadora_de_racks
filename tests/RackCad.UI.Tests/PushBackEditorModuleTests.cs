using System;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems;
using RackCad.UI.Editor;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-18b increment 3b — the Push Back editor module, its registration and the library dispatch. Pure metadata + the
    /// real <see cref="EditorModuleRegistry.Default"/>: opening a window is not exercised here (that is STA), but the
    /// module's contract, its <c>MatchesLibrary</c> rules and the registry resolution are.
    /// </summary>
    public sealed class PushBackEditorModuleTests
    {
        private static PushBackDesign PushBackDesign()
            => new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 4,
                    LoadLevels = 3,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            };

        private static DynamicRackDesign DynamicDesign()
            => new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 3,
                FirstLevelHeight = 6.0,
                BeamDepth = 4.0
            };

        private static RackDesignLibraryEntry Entry(RackSystemKind kind)
            => new RackDesignLibraryEntry("x" + RackProjectStore.FileExtension, "Test", kind, "Etiqueta", default);

        // ---- Module contract ----

        [Fact]
        public void Contract_IsFrozen()
        {
            var module = new PushBackEditorModule();

            Assert.Equal(RackSystemKind.PushBack, module.Kind);
            Assert.True(module.CanInsert);
            Assert.False(module.IsLibraryFallback);
            Assert.Equal("No se pudo abrir el sistema Push Back: ", module.OpenFailureMessage);
        }

        [Fact]
        public void MatchesLibrary_TrueOnly_ForAPushBackEntryWithAPushBackPayload()
        {
            var module = new PushBackEditorModule();
            var pushBack = RackProject.ForPushBack(PushBackDesign());

            Assert.True(module.MatchesLibrary(Entry(RackSystemKind.PushBack), pushBack)); // the one accepting case

            Assert.False(module.MatchesLibrary(null, pushBack));                                   // entry null
            Assert.False(module.MatchesLibrary(Entry(RackSystemKind.PushBack), null));             // project null
            Assert.False(module.MatchesLibrary(Entry(RackSystemKind.PalletFlow), pushBack));       // a different kind
            Assert.False(module.MatchesLibrary(Entry(RackSystemKind.PushBack), RackProject.ForPushBack(null))); // PushBack wrapper, no payload
            Assert.False(module.MatchesLibrary(Entry(RackSystemKind.PalletFlow), RackProject.ForDynamic(DynamicDesign()))); // a dynamic design
        }

        // ---- Library dispatch through the real Default registry ----

        [Fact]
        public void Default_ResolvesAPushBackProject_ToThePushBackModule()
        {
            var module = EditorModuleRegistry.Default.ResolveForLibrary(
                Entry(RackSystemKind.PushBack), RackProject.ForPushBack(PushBackDesign()));

            Assert.IsType<PushBackEditorModule>(module);
            Assert.Equal(RackSystemKind.PushBack, module.Kind);
        }

        [Fact]
        public void Default_DoesNotConfusePushBackWithDynamic()
        {
            var pushBackModule = EditorModuleRegistry.Default.ResolveForLibrary(
                Entry(RackSystemKind.PushBack), RackProject.ForPushBack(PushBackDesign()));
            var dynamicModule = EditorModuleRegistry.Default.ResolveForLibrary(
                Entry(RackSystemKind.PalletFlow), RackProject.ForDynamic(DynamicDesign()));

            Assert.IsType<PushBackEditorModule>(pushBackModule);
            Assert.IsType<DynamicEditorModule>(dynamicModule);
            Assert.NotEqual(pushBackModule.Kind, dynamicModule.Kind);
        }

        [Fact]
        public void Default_PushBackEntryWithoutPayload_ResolvesToNoKindSpecificModule()
        {
            // A PushBack entry over a project with no PushBack payload: no kind-specific module matches, and the header
            // fallback only catches projects with a header — a cama has none.
            var module = EditorModuleRegistry.Default.ResolveForLibrary(
                Entry(RackSystemKind.PushBack), RackProject.ForCama(new FlowBedConfiguration()));

            Assert.Null(module);
        }

        [Fact]
        public void Default_HeaderProjectStillResolvesToTheFallback_AndOtherKindsToTheirModules()
        {
            var header = EditorModuleRegistry.Default.ResolveForLibrary(
                Entry(RackSystemKind.Selective), RackProject.ForSelective(new RackCad.Application.RackFrames.HardcodedStandardRackFrameService().CreateDefault()));
            Assert.Equal(RackSystemKind.Selective, header.Kind);
            Assert.True(header.IsLibraryFallback);

            Assert.Equal(RackSystemKind.Cama, EditorModuleRegistry.Default.ResolveForLibrary(Entry(RackSystemKind.Cama), RackProject.ForCama(new FlowBedConfiguration())).Kind);
            Assert.Equal(RackSystemKind.Larguero, EditorModuleRegistry.Default.ResolveForLibrary(Entry(RackSystemKind.Larguero), RackProject.ForLarguero(new LargueroDesign())).Kind);
        }

        // ---- Registry lookup + uniqueness ----

        [Fact]
        public void Default_GetAndTryGet_ReturnThePushBackModule()
        {
            Assert.IsType<PushBackEditorModule>(EditorModuleRegistry.Default.Get(RackSystemKind.PushBack));
            Assert.True(EditorModuleRegistry.Default.TryGet(RackSystemKind.PushBack, out var module));
            Assert.IsType<PushBackEditorModule>(module);
        }

        [Fact]
        public void DuplicatePushBackModule_Throws()
        {
            var ex = Assert.Throws<ArgumentException>(() => new EditorModuleRegistry(new IRackEditorModule[]
            {
                new PushBackEditorModule(),
                new PushBackEditorModule(),
            }));

            Assert.Contains("PushBack", ex.Message);
        }
    }
}
