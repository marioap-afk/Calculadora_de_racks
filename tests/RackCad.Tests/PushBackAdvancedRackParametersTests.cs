using System;
using System.Linq;
using System.Text.Json.Nodes;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-35, segunda ronda del Owner — los CUATRO residuos de la validación manual: altura manual de cabecera,
    /// refuerzo del poste derivado (activación + longitud opcional), cantidad de separadores y separación de
    /// separadores.
    ///
    /// Son parámetros GLOBALES DEL RACK, no propiedades del módulo <c>Separator</c>, y reutilizan EXCLUSIVAMENTE las
    /// autoridades que el diseño dinámico compuesto ya tiene: <c>ManualHeaderHeightOverride</c>,
    /// <c>DerivedPostReinforced</c>, <c>DerivedPostReinforcementHeight</c>, <c>SeparatorCountOverride</c> y
    /// <c>SeparatorSpacingOverride</c>. Aquí no se declara ninguna regla nueva: se comprueba que Push Back conduce
    /// la intención del usuario hasta esas autoridades y que el sistema resuelto la refleja.
    /// </summary>
    public class PushBackAdvancedRackParametersTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackEditorDesignAssembler Assembler() => new PushBackEditorDesignAssembler(Catalog);

        private static PushBackEditorInputs Inputs()
            => new PushBackEditorInputs
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 6
            };

        /// <summary>A live editor with a baseline, so the advanced parameters act on a real structure.</summary>
        private static PushBackEditorState Live(PushBackEditorDesignAssembler assembler, out PushBackEditorInputs inputs)
        {
            var state = new PushBackEditorState();
            state.Structure.Fronts[0].PalletsDeep = 6;
            inputs = Inputs();
            var seed = assembler.Build(state, inputs);
            Assert.True(seed.IsValid, seed.Error);
            assembler.AcceptComputation(state, seed);
            return state;
        }

        private static double ResolvedHeaderHeight(PushBackEditorComputation computation)
            => computation.System.Structure.Modules
                .First(module => module.IsHeader && module.AssociatedFrameConfiguration != null)
                .AssociatedFrameConfiguration.Height;

        // ===== Residuo 1 — altura personalizada de cabecera =====================================================

        [Fact]
        public void Residuo1_ManualHeaderHeight_ReachesTheResolvedSystem_ThroughTheExistingAuthority()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var calculated = ResolvedHeaderHeight(assembler.Build(state, inputs));
            var manual = calculated + 24.0;

            inputs.ManualHeaderHeightOverride = manual;
            var computation = assembler.Build(state, inputs);

            Assert.True(computation.IsValid, computation.Error);
            Assert.Equal(manual, computation.Design.Structure.ManualHeaderHeightOverride);
            Assert.Equal(manual, computation.System.Structure.ManualHeaderHeightOverride);
            Assert.Equal(manual, ResolvedHeaderHeight(computation), 4);
        }

        [Fact]
        public void Residuo1_ManualHeaderHeightNull_KeepsTheStandingCalculation()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var calculated = ResolvedHeaderHeight(assembler.Build(state, inputs));

            inputs.ManualHeaderHeightOverride = null;
            var computation = assembler.Build(state, inputs);

            Assert.True(computation.IsValid, computation.Error);
            Assert.Null(computation.Design.Structure.ManualHeaderHeightOverride);
            Assert.Equal(calculated, ResolvedHeaderHeight(computation), 4);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-10.0)]
        public void Residuo1_ManualHeaderHeightNotPositive_IsRejectedWithAVisibleError(double invalid)
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);

            inputs.ManualHeaderHeightOverride = invalid;
            var computation = assembler.Build(state, inputs);

            Assert.False(computation.IsValid);
            Assert.False(string.IsNullOrWhiteSpace(computation.Error));
        }

        /// <summary>Una cabecera personalizada conserva configuración y procedencia cuando cambia la altura global:
        /// el cambio pasa por la adaptación y la validación de I-35, no por un camino paralelo.</summary>
        [Fact]
        public void Residuo1_ACustomCabecera_KeepsConfigurationAndProvenance_WhenTheGlobalHeightChanges()
        {
            const double marker = 40.0;
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headerId = state.ModuleSession.Modules.First(module => module.IsHeader).ModuleId;

            state.ModuleSession.SetHeaderConfiguration(headerId, CustomHeader(marker));
            state.CommitModuleEdits();
            assembler.AcceptComputation(state, assembler.Build(state, inputs));
            state.ClearModuleCommit();

            inputs.ManualHeaderHeightOverride = ResolvedHeaderHeight(assembler.Build(state, inputs)) + 24.0;
            var computation = assembler.Build(state, inputs);

            Assert.True(computation.IsValid, computation.Error);
            var survivor = computation.System.Structure.Modules.First(module => module.ModuleId == headerId);
            Assert.False(survivor.UseCalculatedHeaderConfiguration);
            Assert.Equal(marker, survivor.AssociatedFrameConfiguration.PanelClear, 4);
            Assert.Contains(headerId, state.LastModuleReconciliation.Preserved);
        }

        // ===== Residuo 2 — refuerzo del poste derivado ==========================================================

        [Fact]
        public void Residuo2_ReinforcementOn_WithNoHeight_IsFullHeight()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);

            inputs.DerivedPostReinforced = true;
            inputs.DerivedPostReinforcementHeight = null;
            var computation = assembler.Build(state, inputs);

            Assert.True(computation.IsValid, computation.Error);
            Assert.True(computation.System.Structure.DerivedPostReinforced);
            Assert.Null(computation.System.Structure.DerivedPostReinforcementHeight);
            Assert.Null(computation.Design.Structure.DerivedPostReinforcementHeight);
        }

        [Fact]
        public void Residuo2_ReinforcementOn_WithAHeight_IsPartial_AndReachesTheResolvedSystem()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var partial = ResolvedHeaderHeight(assembler.Build(state, inputs)) / 2.0;

            inputs.DerivedPostReinforced = true;
            inputs.DerivedPostReinforcementHeight = partial;
            var computation = assembler.Build(state, inputs);

            Assert.True(computation.IsValid, computation.Error);
            Assert.True(computation.System.Structure.DerivedPostReinforced);
            Assert.Equal(partial, computation.System.Structure.DerivedPostReinforcementHeight);
            Assert.Equal(partial, computation.Design.Structure.DerivedPostReinforcementHeight);
        }

        /// <summary>Desactivar el refuerzo elimina SOLO el refuerzo: el poste derivado es consecuencia estructural de
        /// dos separadores consecutivos y sigue existiendo.</summary>
        [Fact]
        public void Residuo2_ReinforcementOff_RemovesOnlyTheReinforcement_NotTheDerivedPost()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var withReinforcement = assembler.Build(state, inputs);
            var derivedPostsBefore = withReinforcement.System.Structure.GetDerivedPostOffsets().Count;

            inputs.DerivedPostReinforced = false;
            var computation = assembler.Build(state, inputs);

            Assert.True(computation.IsValid, computation.Error);
            Assert.False(computation.System.Structure.DerivedPostReinforced);
            Assert.Equal(derivedPostsBefore, computation.System.Structure.GetDerivedPostOffsets().Count);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-5.0)]
        public void Residuo2_ReinforcementHeightNotPositive_IsRejectedWithAVisibleError(double invalid)
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);

            inputs.DerivedPostReinforced = true;
            inputs.DerivedPostReinforcementHeight = invalid;
            var computation = assembler.Build(state, inputs);

            Assert.False(computation.IsValid);
            Assert.False(string.IsNullOrWhiteSpace(computation.Error));
        }

        [Fact]
        public void Residuo2_ReinforcementHeightAboveThePost_IsRejectedWithAVisibleError()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var postHeight = ResolvedHeaderHeight(assembler.Build(state, inputs));

            inputs.DerivedPostReinforced = true;
            inputs.DerivedPostReinforcementHeight = postHeight + 12.0;
            var computation = assembler.Build(state, inputs);

            Assert.False(computation.IsValid);
            Assert.False(string.IsNullOrWhiteSpace(computation.Error));
        }

        /// <summary>
        /// Una recomputación que REDUCE la altura disponible vuelve inválida una altura antes válida. Debe BLOQUEAR
        /// con error visible: nada se recorta ni se restaura en silencio.
        /// </summary>
        [Fact]
        public void Residuo2_ARecomputeThatShrinksThePost_BlocksInsteadOfSilentlyClampingAValidHeight()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var tallHeight = ResolvedHeaderHeight(assembler.Build(state, inputs));

            inputs.DerivedPostReinforcementHeight = tallHeight - 2.0;   // valid right now
            var valid = assembler.Build(state, inputs);
            Assert.True(valid.IsValid, valid.Error);

            // Force the post to shrink: a manual header height well below the reinforcement.
            inputs.ManualHeaderHeightOverride = tallHeight / 3.0;
            var shrunk = assembler.Build(state, inputs);

            Assert.False(shrunk.IsValid);
            Assert.False(string.IsNullOrWhiteSpace(shrunk.Error));
            // Nothing was clamped behind the user's back: the requested value is still the requested value.
            Assert.Equal(tallHeight - 2.0, inputs.DerivedPostReinforcementHeight);
        }

        [Fact]
        public void Residuo2_ReinforcementHeightIsIgnoredWhenTheReinforcementIsOff_SoItCannotBlock()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var postHeight = ResolvedHeaderHeight(assembler.Build(state, inputs));

            inputs.DerivedPostReinforced = false;
            inputs.DerivedPostReinforcementHeight = postHeight + 100.0;   // absurd, but there is no reinforcement
            var computation = assembler.Build(state, inputs);

            Assert.True(computation.IsValid, computation.Error);
            Assert.False(computation.System.Structure.DerivedPostReinforced);
        }

        // ===== Residuos 3 y 4 — cantidad y separación de separadores ============================================

        [Fact]
        public void Residuo3_SeparatorCount_ReachesTheResolvedSystem_ThroughTheExistingAuthority()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);

            inputs.SeparatorCountOverride = 5;
            var computation = assembler.Build(state, inputs);

            Assert.True(computation.IsValid, computation.Error);
            Assert.Equal(5, computation.Design.Structure.SeparatorCountOverride);
            Assert.Equal(5, computation.System.Structure.SeparatorCountOverride);
        }

        [Fact]
        public void Residuo4_SeparatorSpacing_ReachesTheResolvedSystem_ThroughTheExistingAuthority()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);

            inputs.SeparatorSpacingOverride = 21.5;
            var computation = assembler.Build(state, inputs);

            Assert.True(computation.IsValid, computation.Error);
            Assert.Equal(21.5, computation.Design.Structure.SeparatorSpacingOverride);
            Assert.Equal(21.5, computation.System.Structure.SeparatorSpacingOverride);
        }

        [Fact]
        public void Residuos34_BothNull_MeanTheAutomaticCalculation()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);

            inputs.SeparatorCountOverride = null;
            inputs.SeparatorSpacingOverride = null;
            var computation = assembler.Build(state, inputs);

            Assert.True(computation.IsValid, computation.Error);
            Assert.Null(computation.System.Structure.SeparatorCountOverride);
            Assert.Null(computation.System.Structure.SeparatorSpacingOverride);
        }

        /// <summary>Cantidad y separación son INDEPENDIENTES: fijar una no obliga ni altera la otra.</summary>
        [Fact]
        public void Residuos34_CountAndSpacing_AreIndependent()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);

            inputs.SeparatorCountOverride = 4;
            inputs.SeparatorSpacingOverride = null;
            var onlyCount = assembler.Build(state, inputs);
            Assert.True(onlyCount.IsValid, onlyCount.Error);
            Assert.Equal(4, onlyCount.System.Structure.SeparatorCountOverride);
            Assert.Null(onlyCount.System.Structure.SeparatorSpacingOverride);

            inputs.SeparatorCountOverride = null;
            inputs.SeparatorSpacingOverride = 18.0;
            var onlySpacing = assembler.Build(state, inputs);
            Assert.True(onlySpacing.IsValid, onlySpacing.Error);
            Assert.Null(onlySpacing.System.Structure.SeparatorCountOverride);
            Assert.Equal(18.0, onlySpacing.System.Structure.SeparatorSpacingOverride);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-2)]
        public void Residuo3_SeparatorCountNotPositive_IsRejectedWithAVisibleError(int invalid)
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);

            inputs.SeparatorCountOverride = invalid;
            var computation = assembler.Build(state, inputs);

            Assert.False(computation.IsValid);
            Assert.False(string.IsNullOrWhiteSpace(computation.Error));
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-3.0)]
        public void Residuo4_SeparatorSpacingNotPositive_IsRejectedWithAVisibleError(double invalid)
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);

            inputs.SeparatorSpacingOverride = invalid;
            var computation = assembler.Build(state, inputs);

            Assert.False(computation.IsValid);
            Assert.False(string.IsNullOrWhiteSpace(computation.Error));
        }

        // ===== Restauración explícita devuelve los cuatro ámbitos al cálculo/default ============================

        [Fact]
        public void AStandardRestore_ReturnsTheFourAdvancedScopes_ToTheirCalculationOrDefault()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);

            inputs.ManualHeaderHeightOverride = ResolvedHeaderHeight(assembler.Build(state, inputs)) + 24.0;
            inputs.DerivedPostReinforced = false;
            inputs.DerivedPostReinforcementHeight = 30.0;
            inputs.SeparatorCountOverride = 5;
            inputs.SeparatorSpacingOverride = 21.5;
            assembler.AcceptComputation(state, assembler.Build(state, inputs));

            state.RestoreAdvancedRackParameters(inputs);
            var restored = assembler.Build(state, inputs);

            Assert.True(restored.IsValid, restored.Error);
            Assert.Null(inputs.ManualHeaderHeightOverride);
            Assert.True(inputs.DerivedPostReinforced);            // the default is reinforced
            Assert.Null(inputs.DerivedPostReinforcementHeight);
            Assert.Null(inputs.SeparatorCountOverride);
            Assert.Null(inputs.SeparatorSpacingOverride);
            Assert.Null(restored.System.Structure.ManualHeaderHeightOverride);
            Assert.True(restored.System.Structure.DerivedPostReinforced);
            Assert.Null(restored.System.Structure.SeparatorCountOverride);
            Assert.Null(restored.System.Structure.SeparatorSpacingOverride);
        }

        // ===== Persistencia: round-trip, biblioteca, Xrecord, legacy y campos desconocidos ======================

        [Fact]
        public void TheFourScopes_SurviveTheProjectRoundTrip()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var manual = ResolvedHeaderHeight(assembler.Build(state, inputs)) + 24.0;
            var partial = manual / 2.0;

            inputs.ManualHeaderHeightOverride = manual;
            inputs.DerivedPostReinforced = true;
            inputs.DerivedPostReinforcementHeight = partial;
            inputs.SeparatorCountOverride = 5;
            inputs.SeparatorSpacingOverride = 21.5;
            var design = assembler.BuildDesign(state, inputs);

            var store = new RackProjectStore();
            var reloaded = store.Deserialize(store.Serialize(RackProject.ForPushBack(design))).PushBackDesign;

            Assert.Equal(manual, reloaded.Structure.ManualHeaderHeightOverride);
            Assert.True(reloaded.Structure.DerivedPostReinforced);
            Assert.Equal(partial, reloaded.Structure.DerivedPostReinforcementHeight);
            Assert.Equal(5, reloaded.Structure.SeparatorCountOverride);
            Assert.Equal(21.5, reloaded.Structure.SeparatorSpacingOverride);
        }

        /// <summary>
        /// Apagar el refuerzo NO persiste una medida muerta: la estructura guarda «sin refuerzo» y ninguna longitud,
        /// de modo que ningun lector posterior pueda encontrar una altura de refuerzo en un rack que no lo dibuja.
        /// </summary>
        [Fact]
        public void ReinforcementOff_PersistsNoLength_SoNoDeadMeasurementSurvives()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);

            inputs.DerivedPostReinforced = false;
            inputs.DerivedPostReinforcementHeight = 30.0;   // leftover from a previous edit
            var design = assembler.BuildDesign(state, inputs);

            var store = new RackProjectStore();
            var reloaded = store.Deserialize(store.Serialize(RackProject.ForPushBack(design))).PushBackDesign;

            Assert.False(reloaded.Structure.DerivedPostReinforced);
            Assert.Null(reloaded.Structure.DerivedPostReinforcementHeight);
        }

        [Fact]
        public void TheFourScopes_ComeBackThroughLoad_SoTheWindowRepopulatesItsPanel()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var manual = ResolvedHeaderHeight(assembler.Build(state, inputs)) + 24.0;

            var partial = manual / 2.0;
            inputs.ManualHeaderHeightOverride = manual;
            inputs.DerivedPostReinforced = true;
            inputs.DerivedPostReinforcementHeight = partial;
            inputs.SeparatorCountOverride = 5;
            inputs.SeparatorSpacingOverride = 21.5;
            var design = assembler.BuildDesign(state, inputs);

            var reopened = new PushBackEditorState();
            var recovered = reopened.LoadFromDesign(design, assembler.Resolver);

            Assert.Equal(manual, recovered.ManualHeaderHeightOverride);
            Assert.True(recovered.DerivedPostReinforced);
            Assert.Equal(partial, recovered.DerivedPostReinforcementHeight);
            Assert.Equal(5, recovered.SeparatorCountOverride);
            Assert.Equal(21.5, recovered.SeparatorSpacingOverride);
        }

        [Fact]
        public void TheFourScopes_SurviveTheXrecordEnvelope_WithTheSameGuid()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            inputs.SeparatorCountOverride = 5;
            inputs.DerivedPostReinforced = false;
            var design = assembler.BuildDesign(state, inputs);

            var store = new RackProjectStore();
            var embedStore = new RackEmbedStore();
            var guid = Guid.NewGuid().ToString();
            var payload = embedStore.Serialize(RackEmbedComposer.Compose(
                null, RackEmbedDocument.KindPushBack, guid, "PB-35b", RackEmbedDocument.ViewLateral, 0,
                store.Serialize(RackProject.ForPushBack(design))));

            var embed = embedStore.Deserialize(payload);
            var reloaded = store.Deserialize(embed.Design).PushBackDesign;

            Assert.Equal(guid, embed.Id);
            Assert.Equal(5, reloaded.Structure.SeparatorCountOverride);
            Assert.False(reloaded.Structure.DerivedPostReinforced);
        }

        [Fact]
        public void ALegacyDocument_WithoutTheFourScopes_LoadsWithTheStandingCalculation()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var design = assembler.BuildDesign(state, inputs);

            var store = new RackProjectStore();
            var node = JsonNode.Parse(store.Serialize(RackProject.ForPushBack(design)));
            var structure = node["PushBack"]["Structure"].AsObject();
            foreach (var key in new[]
                     {
                         "ManualHeaderHeightOverride", "DerivedPostReinforced", "DerivedPostReinforcementHeight",
                         "SeparatorCountOverride", "SeparatorSpacingOverride"
                     })
            {
                structure.Remove(key);
            }

            var reloaded = store.Deserialize(node.ToJsonString()).PushBackDesign;

            Assert.Null(reloaded.Structure.ManualHeaderHeightOverride);
            Assert.True(reloaded.Structure.DerivedPostReinforced);   // legacy default
            Assert.Null(reloaded.Structure.DerivedPostReinforcementHeight);
            Assert.Null(reloaded.Structure.SeparatorCountOverride);
            Assert.Null(reloaded.Structure.SeparatorSpacingOverride);
        }

        [Fact]
        public void TheFourScopes_PreserveUnknownJsonFields_I11()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            inputs.SeparatorSpacingOverride = 21.5;
            var design = assembler.BuildDesign(state, inputs);

            var store = new RackProjectStore();
            var node = JsonNode.Parse(store.Serialize(RackProject.ForPushBack(design)));
            node["PushBack"]["CampoDesconocidoDeOtraVersion"] = "conservame";

            var loaded = store.Deserialize(node.ToJsonString());
            var rewritten = JsonNode.Parse(
                store.Serialize(RackProject.ForPushBack(loaded.PushBackDesign).WithSourceMetadataFrom(loaded)));

            Assert.Equal("conservame", rewritten["PushBack"]["CampoDesconocidoDeOtraVersion"].GetValue<string>());
            Assert.Equal(
                21.5,
                store.Deserialize(rewritten.ToJsonString()).PushBackDesign.Structure.SeparatorSpacingOverride);
        }

        // ===== Preview, cuatro vistas y BOM consumen el MISMO sistema resuelto ==================================

        [Fact]
        public void TheFourScopes_ReachTheFourViewsAndTheBom_FromOneResolvedSystem()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var before = assembler.Build(state, inputs);

            inputs.SeparatorCountOverride = 5;
            inputs.DerivedPostReinforced = false;
            var after = assembler.Build(state, inputs);

            Assert.True(after.IsValid, after.Error);
            Assert.NotNull(after.LateralPlan);
            Assert.NotNull(after.FrontalEntradaSalida);
            Assert.NotNull(after.FrontalPosterior);
            Assert.NotNull(after.PlantaPlan);
            Assert.NotNull(after.Bom);

            // The lateral view and the BOM both change, and both read the same resolved system.
            Assert.False(after.System.Structure.DerivedPostReinforced);
            Assert.NotEqual(PlanSignature(before.LateralPlan), PlanSignature(after.LateralPlan));
            Assert.NotEqual(BomSignature(before.Bom), BomSignature(after.Bom));
        }

        [Fact]
        public void APartialReinforcement_ChangesTheBom_AgainstTheFullHeightOne()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var full = assembler.Build(state, inputs);

            inputs.DerivedPostReinforcementHeight = ResolvedHeaderHeight(full) / 2.0;
            var partial = assembler.Build(state, inputs);

            Assert.True(partial.IsValid, partial.Error);
            Assert.NotEqual(BomSignature(full.Bom), BomSignature(partial.Bom));
        }

        // ===== Fixtures ========================================================================================

        private static RackFrameConfiguration CustomHeader(double panelClear)
        {
            var catalog = Catalog;
            var configuration = new DynamicRackSystemBuilder(catalog).BuildHeaderConfiguration(
                RackFrameTemplateCatalog.Default, catalog.Defaults?.Post, 120.0, 48.0);
            configuration.PanelClear = panelClear;
            return configuration;
        }

        private static string PlanSignature(HeaderRunPlan plan)
            => plan == null
                ? string.Empty
                : string.Join("|", plan.Flatten().Instances.Select(instance => string.Join(
                    ",", instance.PieceId, instance.Insertion.X.ToString("0.###"), instance.Insertion.Y.ToString("0.###"))));

        private static string BomSignature(BillOfMaterials bom)
            => bom == null
                ? string.Empty
                : string.Join("|", bom.Lines.Select(line => string.Join(
                    ",", line.Category, line.ProfileId, line.Length.ToString("0.###"), line.Quantity.ToString())));
    }
}
