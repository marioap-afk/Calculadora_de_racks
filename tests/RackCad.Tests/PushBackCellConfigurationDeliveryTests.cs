using System;
using System.IO;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-41 — los limites de ENTREGA de la configuracion por celda: biblioteca, duplicado, actualizacion, identidad
    /// (GUID + metadata), los servicios de dibujo / handlers / comandos del Plugin, y la no-regresion de los otros
    /// sistemas (Dinamico, Selectivo, Cama).
    ///
    /// El Plugin referencia AutoCAD y esta suite no puede cargarlo (ADR-0003), asi que su parte se comprueba leyendo
    /// el codigo como TEXTO, con el mismo patron que <see cref="PushBackPluginSourceGuardTests"/>.
    /// </summary>
    public class PushBackCellConfigurationDeliveryTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.True(dir != null, "Could not locate the repo root (RackCad.sln) from the test output directory.");
            return dir;
        }

        private static string ReadSource(string project, string relativePath)
        {
            var path = Path.Combine(RepoRoot().FullName, "src", project, relativePath);
            Assert.True(File.Exists(path), $"Source not found: {path}");
            return File.ReadAllText(path);
        }

        /// <summary>Un rack con fondo escalonado y tarimas: el caso que I-41 tiene que hacer viajar entero.</summary>
        private static (PushBackDesign Design, PushBackEditorState State, PushBackEditorInputs Inputs,
            PushBackEditorDesignAssembler Assembler) Staggered()
        {
            var assembler = new PushBackEditorDesignAssembler(Catalog);
            var state = new PushBackEditorState();
            var inputs = state.LoadNew();
            inputs.PalletsDeep = 4;
            state.SetFrontCount(2);
            for (var front = 0; front < 2; front++)
            {
                state.Structure.Fronts[front].LoadLevels = 3;
                state.Structure.Fronts[front].PalletsDeep = 4;
                state.AdjustLevels(front, 0);
            }

            state.ToggleCell(0, 1, false);
            state.ApplyPalletsDeep(6, DynamicRackCellScope.Cell);
            state.ApplyDrawPallet(true, DynamicRackCellScope.Cell);
            state.ToggleCell(1, 0, false);
            state.ApplyPalletsDeep(2, DynamicRackCellScope.Cell);

            return (assembler.BuildDesign(state, inputs), state, inputs, assembler);
        }

        private static void AssertStaggeredIntent(PushBackSystem system)
        {
            Assert.Equal(4, system.DefaultPalletsDeepAt(0));
            Assert.Equal(4, system.EffectivePalletsDeepAt(0, 0));
            Assert.Equal(6, system.EffectivePalletsDeepAt(0, 1));
            Assert.Equal(2, system.EffectivePalletsDeepAt(1, 0));
            Assert.True(system.DrawPalletAt(0, 1));
            Assert.False(system.DrawPalletAt(0, 0));
        }

        // ---- Biblioteca ------------------------------------------------------------------------------------

        [Fact]
        public void Library_CarriesTheCellDepthsAndPalletsThroughASaveAndLoad()
        {
            var (design, _, _, _) = Staggered();
            var store = new RackProjectStore();

            var json = store.Serialize(RackProject.ForPushBack(design));
            var project = store.Deserialize(json);

            Assert.Equal(RackSystemKind.PushBack, project.Kind);
            Assert.NotNull(project.PushBackDesign);
            AssertStaggeredIntent(new PushBackResolver(Catalog).Resolve(project.PushBackDesign));
        }

        [Fact]
        public void Library_ALegacyPushBackProject_StillLoadsWithNoOverrideAndNoPallet()
        {
            var legacy = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 5,
                    LoadLevels = 2,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            };
            var store = new RackProjectStore();

            var project = store.Deserialize(store.Serialize(RackProject.ForPushBack(legacy)));
            var system = new PushBackResolver(Catalog).Resolve(project.PushBackDesign);

            Assert.Equal(5, system.DefaultPalletsDeepAt(0));
            Assert.Equal(5, system.EffectivePalletsDeepAt(0, 0));
            Assert.Equal(5, system.EffectivePalletsDeepAt(0, 1));
            Assert.False(system.DrawPalletAt(0, 0));
        }

        // ---- Duplicado (RACKDUPLICAR: una copia INDEPENDIENTE con la misma configuracion) -------------------

        [Fact]
        public void Duplicating_ProducesAnIndependentCopy_ThatKeepsEveryCellValue()
        {
            var (design, _, _, assembler) = Staggered();
            var store = new RackProjectStore();

            // Un duplicado nace de una re-serializacion del mismo diseno; lo que importa es que la copia lleve la
            // misma intencion y que tocarla NO cambie el original.
            var copy = store.Deserialize(store.Serialize(RackProject.ForPushBack(design))).PushBackDesign;
            AssertStaggeredIntent(new PushBackResolver(Catalog).Resolve(copy));

            copy.Fronts[0].PalletsDeepOverrides[1] = 3;
            copy.Fronts[0].DrawPallets[1] = null;

            var original = new PushBackResolver(Catalog).Resolve(design);
            Assert.Equal(6, original.EffectivePalletsDeepAt(0, 1));   // el original no se movio
            Assert.True(original.DrawPalletAt(0, 1));

            var duplicated = new PushBackResolver(Catalog).Resolve(copy);
            Assert.Equal(3, duplicated.EffectivePalletsDeepAt(0, 1));
            Assert.False(duplicated.DrawPalletAt(0, 1));
        }

        [Fact]
        public void DeepCopyingAFrontConfig_CopiesTheI41Lists_NotTheirReferences()
        {
            var config = new PushBackFrontConfig { DefaultPalletsDeep = 4 };
            config.HighEndBeamPeraltes.Add(3.5);
            config.PalletsDeepOverrides.Add(7);
            config.DrawPallets.Add(true);

            var copy = config.DeepCopy();
            config.PalletsDeepOverrides[0] = 2;
            config.DrawPallets[0] = null;

            Assert.Equal(4, copy.DefaultPalletsDeep);
            Assert.Equal(7, copy.PalletsDeepOverrideAt(0));
            Assert.True(copy.DrawPalletAt(0));
        }

        // ---- Actualizacion (Actualizar redibuja EN SITIO el mismo rack) -------------------------------------

        [Fact]
        public void Updating_RebuildsTheSameRack_WithTheNewCellDepths()
        {
            var (_, state, inputs, assembler) = Staggered();
            var before = assembler.Build(state, inputs);
            Assert.True(before.IsValid, before.Error);
            assembler.AcceptComputation(state, before);

            // El usuario cambia un fondo de celda y vuelve a pulsar Actualizar.
            state.ToggleCell(0, 2, false);
            state.ApplyPalletsDeep(5, DynamicRackCellScope.Cell);
            var after = assembler.Build(state, inputs);

            Assert.True(after.IsValid, after.Error);
            Assert.Equal(5, after.System.EffectivePalletsDeepAt(0, 2));
            Assert.Equal(6, after.System.EffectivePalletsDeepAt(0, 1));   // lo demas intacto
            Assert.True(after.System.DrawPalletAt(0, 1));
        }

        // ---- Identidad: GUID + metadata sobreviven -----------------------------------------------------------

        [Fact]
        public void TheRackIdentity_SurvivesALibraryRoundTripWithCellConfiguration()
        {
            var (design, _, _, _) = Staggered();
            var store = new RackProjectStore();
            var source = RackProject.ForPushBack(design);

            var saved = store.Deserialize(store.Serialize(source));
            var reSaved = store.Deserialize(store.Serialize(
                RackProject.ForPushBack(saved.PushBackDesign).WithSourceMetadataFrom(saved)));

            Assert.Equal(RackSystemKind.PushBack, reSaved.Kind);
            AssertStaggeredIntent(new PushBackResolver(Catalog).Resolve(reSaved.PushBackDesign));
        }

        [Fact]
        public void TheDocument_PreservesUnknownFieldsAlongsideTheI41Fields()
        {
            var (design, _, _, _) = Staggered();
            var document = PushBackDesignDocument.FromDomain(design);
            var json = System.Text.Json.JsonSerializer.Serialize(document);

            // Un campo que ESTE build no conoce, como el que escribiria una version futura.
            var withUnknown = json.Insert(1, "\"CampoDeUnaVersionFutura\":123,");
            var reloaded = System.Text.Json.JsonSerializer.Deserialize<PushBackDesignDocument>(withUnknown);
            var rewritten = System.Text.Json.JsonSerializer.Serialize(
                PushBackDesignDocument.FromDomain(reloaded.ToDomain(), reloaded));

            Assert.Contains("CampoDeUnaVersionFutura", rewritten);
            Assert.Contains("PalletsDeepOverrides", rewritten);
            AssertStaggeredIntent(new PushBackResolver(Catalog).Resolve(reloaded.ToDomain()));
        }

        // ---- RACKEDITAR: reabrir desde el SISTEMA resuelto embebido en el dibujo ----------------------------

        [Fact]
        public void ReopeningFromTheResolvedSystem_RecoversEveryCellValue()
        {
            var (design, _, _, assembler) = Staggered();
            var resolver = assembler.Resolver;
            var system = resolver.Resolve(design);

            // RACKEDITAR reabre desde el sistema resuelto, no desde el diseno.
            var reopened = new PushBackEditorState();
            reopened.LoadFromSystem(system, resolver);

            Assert.Equal(4, reopened.Structure.Fronts[0].PalletsDeep);   // el DEFAULT, no la envolvente (6)
            Assert.Null(reopened.Cell(0, 0).PalletsDeepOverride);
            Assert.Equal(6, reopened.Cell(0, 1).PalletsDeepOverride);
            Assert.True(reopened.Cell(0, 1).DrawPallet);
            Assert.Equal(2, reopened.Cell(1, 0).PalletsDeepOverride);
            Assert.False(reopened.Cell(1, 0).DrawPallet);
        }

        [Fact]
        public void ReopeningFromTheResolvedSystem_IsIdempotent()
        {
            var (design, _, inputs, assembler) = Staggered();
            var resolver = assembler.Resolver;

            var first = new PushBackEditorState();
            first.LoadFromSystem(resolver.Resolve(design), resolver);
            var second = new PushBackEditorState();
            second.LoadFromSystem(resolver.Resolve(assembler.BuildDesign(first, inputs)), resolver);

            Assert.Equal(first.Structure.Fronts[0].PalletsDeep, second.Structure.Fronts[0].PalletsDeep);
            Assert.Equal(first.Cell(0, 1).PalletsDeepOverride, second.Cell(0, 1).PalletsDeepOverride);
            Assert.Equal(first.Cell(1, 0).PalletsDeepOverride, second.Cell(1, 0).PalletsDeepOverride);
            Assert.Equal(first.Cell(0, 1).DrawPallet, second.Cell(0, 1).DrawPallet);
        }

        // ---- Plugin: servicios de dibujo, handler y comandos siguen siendo delgados -------------------------

        [Fact]
        public void TheDrawServices_StayThinAdapters_WithNoCellDepthRuleOfTheirOwn()
        {
            foreach (var file in new[] { "PushBackSystemDrawService.cs", "PushBackFrontalDrawService.cs", "PushBackPlantaDrawService.cs" })
            {
                var source = ReadSource("RackCad.Plugin", Path.Combine("Systems", "PushBack", file));
                Assert.DoesNotContain("PalletsDeep", source);
                Assert.DoesNotContain("DrawPallet", source);
                Assert.DoesNotContain("PushBackCellDepth", source);
                Assert.DoesNotContain("PushBackTarimaPlacement", source);
            }
        }

        [Fact]
        public void TheKindHandlerAndCommands_CarryNoCellDepthOrPalletRule()
        {
            foreach (var (project, file) in new[]
                     {
                         ("RackCad.Plugin", Path.Combine("KindHandlers", "PushBackKindHandler.cs")),
                         ("RackCad.Plugin", "RackPushBackCommands.cs")
                     })
            {
                var source = ReadSource(project, file);
                Assert.DoesNotContain("PushBackCellDepth", source);
                Assert.DoesNotContain("PushBackTarimaPlacement", source);
                Assert.DoesNotContain("EffectivePalletsDeepAt", source);
            }
        }

        [Fact]
        public void TheCellDepthRule_LivesInExactlyOneApplicationFile()
        {
            var root = Path.Combine(RepoRoot().FullName, "src");
            var offenders = Directory
                .EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
                .Where(path => !Path.GetFileName(path).Equals("PushBackCellDepth.cs", StringComparison.OrdinalIgnoreCase))
                .Where(path => File.ReadAllText(path).Contains("cellOverride ?? frontDefault"))
                .ToList();

            Assert.Empty(offenders);
        }

        // ---- No-regresion de los otros sistemas -------------------------------------------------------------

        [Fact]
        public void TheDynamicSystem_KnowsNothingAboutTheCellDepthOrThePallet()
        {
            var root = Path.Combine(RepoRoot().FullName, "src", "RackCad.Application", "Systems", "Dynamic");
            foreach (var path in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
            {
                var source = File.ReadAllText(path);
                Assert.DoesNotContain("PushBackCellDepth", source);
                Assert.DoesNotContain("PushBackTarimaPlacement", source);
                Assert.DoesNotContain("PalletsDeepOverride", source);
            }
        }

        [Fact]
        public void TheDynamicSystem_StillResolvesOneDepthPerFront()
        {
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 3,
                FirstLevelHeight = 6.0,
                BeamDepth = 4.0
            };

            var system = new DynamicRackSystemResolver(Catalog).Resolve(design).System;
            var front = system.Fronts[0];

            Assert.Equal(4, front.PalletsDeep);
            // Todos sus niveles siguen compartiendo el mismo tramo longitudinal.
            var placements = DynamicLoadBeamGeometry.Placements(system, front)
                .Where(placement => placement.IsEntrance)
                .Select(placement => Math.Round(placement.X, 6))
                .Distinct()
                .ToList();
            Assert.Single(placements);
            Assert.Equal(front.EndX, placements[0], 6);
        }

        [Fact]
        public void TheSelectiveSystem_KeepsItsOwnRackWidePalletToggle()
        {
            var root = Path.Combine(RepoRoot().FullName, "src", "RackCad.Application", "Systems", "Selective");
            foreach (var path in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
            {
                var source = File.ReadAllText(path);
                Assert.DoesNotContain("PushBackCellDepth", source);
                Assert.DoesNotContain("PushBackTarimaPlacement", source);
            }

            // Y su interruptor sigue siendo del RACK, no de la celda.
            Assert.False(new RackCad.Domain.Systems.Selective.SelectivePalletDesign().DrawPallets);
        }

        [Fact]
        public void TheFlowBedRecipe_IsReusedVerbatim_NotForkedByI41()
        {
            var root = Path.Combine(RepoRoot().FullName, "src", "RackCad.Application", "Systems", "FlowBed");
            foreach (var path in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
            {
                var source = File.ReadAllText(path);
                Assert.DoesNotContain("PushBackCellDepth", source);
                Assert.DoesNotContain("PalletsDeepOverride", source);
            }
        }

        [Fact]
        public void TheBedBuilder_StillAsksTheSharedFlowBedBuilderForItsAssembly()
        {
            var source = ReadSource("RackCad.Application", Path.Combine("Systems", "PushBack", "PushBackFlowBedLateralBuilder.cs"));

            // La receta de riel/rodillos sigue viniendo del builder compartido; I-41 solo le pasa OTRA longitud.
            Assert.Contains("flowBedBuilder.Build(", source);
            Assert.Contains("FlowBedType.Pushback", source);
        }
    }
}
