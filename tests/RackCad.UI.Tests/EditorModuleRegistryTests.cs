using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Domain.Systems;
using RackCad.UI.Editor;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>The explicit editor-module registry the menu and library consume (I-15): registration order/uniqueness,
    /// kind lookup, and the library dispatch (including the cabecera fallback resolved LAST). Uses fake modules for the
    /// resolution mechanics and the real <see cref="EditorModuleRegistry.Default"/> for the frozen metadata + dispatch.</summary>
    public sealed class EditorModuleRegistryTests
    {
        /// <summary>A controllable module: a kind, a fallback flag and a fixed <see cref="MatchesLibrary"/> answer.</summary>
        private sealed class FakeModule : IRackEditorModule
        {
            private readonly bool matches;

            public FakeModule(RackSystemKind kind, bool isFallback = false, bool matches = false)
            {
                Kind = kind;
                IsLibraryFallback = isFallback;
                this.matches = matches;
            }

            public RackSystemKind Kind { get; }
            public bool CanInsert => true;
            public bool IsLibraryFallback { get; }
            public string OpenFailureMessage => "fake: ";
            public bool MatchesLibrary(RackDesignLibraryEntry entry, RackProject project) => matches;
            public RackInsertionRequest OpenForNew(RackEditorLaunchContext context) => null;
            public RackInsertionRequest OpenFromLibrary(RackProject project, RackDesignLibraryEntry entry, RackEditorLaunchContext context) => null;
        }

        private static RackDesignLibraryEntry Entry(RackSystemKind kind)
            => new RackDesignLibraryEntry("x" + RackProjectStore.FileExtension, "Test", kind, "Etiqueta", default);

        private static RackProject AnyProject() => RackProject.ForCama(new FlowBedConfiguration());

        // ---- Registration mechanics (fakes) ----

        [Fact]
        public void PreservesRegistrationOrder()
        {
            var registry = new EditorModuleRegistry(new IRackEditorModule[]
            {
                new FakeModule(RackSystemKind.Cama),
                new FakeModule(RackSystemKind.Larguero),
                new FakeModule(RackSystemKind.Selective),
            });

            Assert.Equal(
                new[] { RackSystemKind.Cama, RackSystemKind.Larguero, RackSystemKind.Selective },
                registry.Modules.Select(m => m.Kind));
        }

        [Fact]
        public void DuplicateKind_Throws()
        {
            var ex = Assert.Throws<ArgumentException>(() => new EditorModuleRegistry(new IRackEditorModule[]
            {
                new FakeModule(RackSystemKind.Cama),
                new FakeModule(RackSystemKind.Cama),
            }));

            Assert.Contains("Cama", ex.Message);
        }

        [Fact]
        public void NullModuleInList_Throws()
            => Assert.Throws<ArgumentException>(() => new EditorModuleRegistry(new IRackEditorModule[] { null }));

        [Fact]
        public void NullList_Throws()
            => Assert.Throws<ArgumentNullException>(() => new EditorModuleRegistry(null));

        [Fact]
        public void Get_ReturnsRegistered_AndThrowsForUnregistered()
        {
            var camaModule = new FakeModule(RackSystemKind.Cama);
            var registry = new EditorModuleRegistry(new IRackEditorModule[] { camaModule });

            Assert.Same(camaModule, registry.Get(RackSystemKind.Cama));
            Assert.True(registry.TryGet(RackSystemKind.Cama, out var found));
            Assert.Same(camaModule, found);

            Assert.Throws<InvalidOperationException>(() => registry.Get(RackSystemKind.Larguero));
            Assert.False(registry.TryGet(RackSystemKind.Larguero, out _));
        }

        [Fact]
        public void Modules_IsImmutableSnapshot()
        {
            var source = new List<IRackEditorModule> { new FakeModule(RackSystemKind.Cama) };
            var registry = new EditorModuleRegistry(source);

            source.Add(new FakeModule(RackSystemKind.Larguero)); // mutate after construction

            Assert.Single(registry.Modules); // the registry copied defensively
        }

        // ---- Library dispatch order (fakes): the fallback is resolved LAST ----

        [Fact]
        public void ResolveForLibrary_PrefersAMatchingNonFallback()
        {
            var selective = new FakeModule(RackSystemKind.SelectiveRack, matches: true);
            var registry = new EditorModuleRegistry(new IRackEditorModule[]
            {
                selective,
                new FakeModule(RackSystemKind.Cama, matches: false),
            });

            Assert.Same(selective, registry.ResolveForLibrary(Entry(RackSystemKind.SelectiveRack), AnyProject()));
        }

        [Fact]
        public void ResolveForLibrary_FallbackIsTriedLast_EvenWhenRegisteredFirst()
        {
            var fallback = new FakeModule(RackSystemKind.Selective, isFallback: true, matches: true);
            var kindSpecific = new FakeModule(RackSystemKind.Cama, isFallback: false, matches: true);

            // The fallback is registered FIRST and also matches, but a kind-specific match must win.
            var registry = new EditorModuleRegistry(new IRackEditorModule[] { fallback, kindSpecific });

            Assert.Same(kindSpecific, registry.ResolveForLibrary(Entry(RackSystemKind.Cama), AnyProject()));
        }

        [Fact]
        public void ResolveForLibrary_UsesFallbackWhenNoKindSpecificMatches()
        {
            var fallback = new FakeModule(RackSystemKind.Selective, isFallback: true, matches: true);
            var registry = new EditorModuleRegistry(new IRackEditorModule[]
            {
                new FakeModule(RackSystemKind.Cama, matches: false),
                fallback,
            });

            Assert.Same(fallback, registry.ResolveForLibrary(Entry(RackSystemKind.Cama), AnyProject()));
        }

        [Fact]
        public void ResolveForLibrary_ReturnsNullWhenNothingMatches()
        {
            var registry = new EditorModuleRegistry(new IRackEditorModule[]
            {
                new FakeModule(RackSystemKind.Cama, matches: false),
                new FakeModule(RackSystemKind.Selective, isFallback: true, matches: false),
            });

            Assert.Null(registry.ResolveForLibrary(Entry(RackSystemKind.Cama), AnyProject()));
        }

        [Fact]
        public void ResolveForLibrary_NullProject_ReturnsNull()
            => Assert.Null(EditorModuleRegistry.Default.ResolveForLibrary(Entry(RackSystemKind.Cama), null));

        // ---- The real Default registry: frozen metadata (menu order) ----

        [Fact]
        public void Default_HasTheFiveModulesInMenuOrder()
        {
            Assert.Equal(
                new[]
                {
                    RackSystemKind.SelectiveRack, // "Diseñar sistema selectivo"
                    RackSystemKind.PalletFlow,    // "Diseñar sistema dinámico (Pallet Flow)"
                    RackSystemKind.Selective,     // "Diseñar cabecera"
                    RackSystemKind.Cama,          // "Diseñar cama de rodamiento"
                    RackSystemKind.Larguero,      // "Diseñar larguero"
                },
                EditorModuleRegistry.Default.Modules.Select(m => m.Kind));
        }

        [Fact]
        public void Default_OnlyLargueroCannotInsert()
        {
            Assert.False(EditorModuleRegistry.Default.Get(RackSystemKind.Larguero).CanInsert);
            foreach (var kind in new[] { RackSystemKind.SelectiveRack, RackSystemKind.PalletFlow, RackSystemKind.Selective, RackSystemKind.Cama })
            {
                Assert.True(EditorModuleRegistry.Default.Get(kind).CanInsert);
            }
        }

        [Fact]
        public void Default_OnlyCabeceraIsTheLibraryFallback()
        {
            Assert.True(EditorModuleRegistry.Default.Get(RackSystemKind.Selective).IsLibraryFallback);
            foreach (var kind in new[] { RackSystemKind.SelectiveRack, RackSystemKind.PalletFlow, RackSystemKind.Cama, RackSystemKind.Larguero })
            {
                Assert.False(EditorModuleRegistry.Default.Get(kind).IsLibraryFallback);
            }
        }

        [Theory]
        [InlineData(RackSystemKind.SelectiveRack, "No se pudo abrir el sistema selectivo: ")]
        [InlineData(RackSystemKind.PalletFlow, "No se pudo abrir el sistema dinámico: ")]
        [InlineData(RackSystemKind.Selective, "No se pudo abrir el configurador de cabeceras: ")]
        [InlineData(RackSystemKind.Cama, "No se pudo abrir la cama de rodamiento: ")]
        [InlineData(RackSystemKind.Larguero, "No se pudo abrir el editor de largueros: ")]
        public void Default_OpenFailureMessages_AreFrozenVerbatim(RackSystemKind kind, string expected)
            => Assert.Equal(expected, EditorModuleRegistry.Default.Get(kind).OpenFailureMessage);

        // ---- The real Default registry + real modules: library dispatch by project payload ----

        [Fact]
        public void Default_ResolvesACabeceraProject_ToTheHeaderModule_ViaFallback()
        {
            var project = RackProject.ForSelective(new HardcodedStandardRackFrameService().CreateDefault());

            var module = EditorModuleRegistry.Default.ResolveForLibrary(Entry(RackSystemKind.Selective), project);

            Assert.Equal(RackSystemKind.Selective, module.Kind);
            Assert.True(module.IsLibraryFallback);
        }

        [Fact]
        public void Default_ResolvesACamaProject_ToTheFlowBedModule()
        {
            var project = RackProject.ForCama(new FlowBedConfiguration());

            var module = EditorModuleRegistry.Default.ResolveForLibrary(Entry(RackSystemKind.Cama), project);

            Assert.Equal(RackSystemKind.Cama, module.Kind);
        }

        [Fact]
        public void Default_ResolvesASelectiveRackProject_ToTheSelectiveModule()
        {
            var project = RackProject.ForSelectiveRack(new SelectivePalletDesignDocument());

            var module = EditorModuleRegistry.Default.ResolveForLibrary(Entry(RackSystemKind.SelectiveRack), project);

            Assert.Equal(RackSystemKind.SelectiveRack, module.Kind);
        }

        [Fact]
        public void Default_ResolvesALargueroProject_ToTheLargueroModule()
        {
            var project = RackProject.ForLarguero(new LargueroDesign());

            var module = EditorModuleRegistry.Default.ResolveForLibrary(Entry(RackSystemKind.Larguero), project);

            Assert.Equal(RackSystemKind.Larguero, module.Kind);
        }

        [Fact]
        public void Default_CamaProjectDoesNotMatchTheHeaderFallback_NorAMismatchedKind()
        {
            var camaProject = RackProject.ForCama(new FlowBedConfiguration());

            // The header fallback only catches projects with a header; a cama has none.
            Assert.False(EditorModuleRegistry.Default.Get(RackSystemKind.Selective).MatchesLibrary(Entry(RackSystemKind.Cama), camaProject));

            // The dynamic module needs BOTH a PalletFlow entry AND a dynamic payload — a cama has neither, so a
            // (synthetic) PalletFlow entry over a cama project resolves to nothing.
            Assert.False(EditorModuleRegistry.Default.Get(RackSystemKind.PalletFlow).MatchesLibrary(Entry(RackSystemKind.PalletFlow), camaProject));
            Assert.Null(EditorModuleRegistry.Default.ResolveForLibrary(Entry(RackSystemKind.PalletFlow), camaProject));
        }
    }
}
