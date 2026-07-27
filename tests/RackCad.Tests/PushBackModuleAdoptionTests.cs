using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-35 — ADOPCION de la sesion y la reconciliacion de modulos por Push Back, en Application.
    ///
    /// Cubre las cinco decisiones del Owner de punta a punta: eje por modulo de rack, correspondencia exacta
    /// ModuleId + Kind, perdida explicita y reportable, ausencia de descarte ordinario, y confirmar/cancelar en la
    /// sesion. Ademas: una sola resolucion para las cuatro vistas y el BOM, PB-013 intacto, I-33 intacto y la
    /// persistencia completa (round-trip, legacy, biblioteca, Xrecord, campos desconocidos, crecimiento, reduccion
    /// y rejilla dentada).
    /// </summary>
    public class PushBackModuleAdoptionTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackEditorDesignAssembler Assembler() => new PushBackEditorDesignAssembler(Catalog);

        private static PushBackEditorInputs Inputs(double palletDepth = 48.0, int palletsDeep = 4)
            => new PushBackEditorInputs
            {
                Pallet = new PalletSpecification(42.0, palletDepth, 60.0, 1000.0, "kg"),
                PalletsDeep = palletsDeep
            };

        /// <summary>A live editor on a rack that already produced a baseline, so its modules can be customized.</summary>
        private static PushBackEditorState Live(PushBackEditorDesignAssembler assembler, out PushBackEditorInputs inputs, int fronts = 1)
        {
            var state = new PushBackEditorState();
            if (fronts > 1) state.SetFrontCount(fronts);
            inputs = Inputs();
            var seed = assembler.Build(state, inputs);
            Assert.True(seed.IsValid, seed.Error);
            assembler.AcceptComputation(state, seed);
            return state;
        }

        private static RackFrameConfiguration CustomHeader(double panelClear)
        {
            var catalog = Catalog;
            var configuration = new DynamicRackSystemBuilder(catalog).BuildHeaderConfiguration(
                RackFrameTemplateCatalog.Default, catalog.Defaults?.Post, 120.0, 48.0);
            configuration.PanelClear = panelClear;
            return configuration;
        }

        // ===== Confirmar / cancelar (decision 5: viven en la sesion, no en el configurador) =====================

        [Fact]
        public void StagingAModuleEdit_ChangesNothing_UntilItIsConfirmed()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headerId = state.ModuleSession.Modules.First(module => module.IsHeader).ModuleId;
            var before = state.WorkingBaseline.Structure.Modules.First(module => module.ModuleId == headerId).Length;

            state.ModuleSession.SetLength(headerId, before + 9.0);   // staged only

            var untouched = assembler.Build(state, inputs);
            Assert.True(untouched.IsValid, untouched.Error);
            Assert.Equal(
                before,
                untouched.Design.Structure.Modules.First(module => module.ModuleId == headerId).Length,
                6);
        }

        [Fact]
        public void ConfirmingAModuleEdit_AppliesIt_ToTheAssembledDesign()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headerId = state.ModuleSession.Modules.First(module => module.IsHeader).ModuleId;
            var before = state.WorkingBaseline.Structure.Modules.First(module => module.ModuleId == headerId).Length;

            state.ModuleSession.SetLength(headerId, before + 9.0);
            state.CommitModuleEdits();

            var applied = assembler.Build(state, inputs);
            Assert.True(applied.IsValid, applied.Error);
            var module = applied.Design.Structure.Modules.First(m => m.ModuleId == headerId);
            Assert.Equal(before + 9.0, module.Length, 6);
            Assert.True(module.IsManualOverride);
        }

        [Fact]
        public void CancellingAModuleEdit_LeavesTheDesignExactlyAsItWas()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headerId = state.ModuleSession.Modules.First(module => module.IsHeader).ModuleId;
            var before = assembler.BuildDesign(state, inputs);

            state.ModuleSession.SetLength(headerId, 77.0);
            state.ModuleSession.SetHeaderConfiguration(headerId, CustomHeader(40.0));
            state.CancelModuleEdits();

            var after = assembler.BuildDesign(state, inputs);
            Assert.Equal(Signature(before), Signature(after));
        }

        // ===== Cabecera personalizada: sobrevive, se adapta, y solo la restauracion la quita ====================

        [Fact]
        public void AConfirmedCustomCabecera_SurvivesAStructuralChange_AndIsAdapted()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headerId = state.ModuleSession.Modules.First(module => module.IsHeader).ModuleId;

            state.ModuleSession.SetLength(headerId, 55.0);
            state.ModuleSession.SetHeaderConfiguration(headerId, CustomHeader(40.0));
            state.CommitModuleEdits();
            var applied = assembler.Build(state, inputs);
            Assert.True(applied.IsValid, applied.Error);
            assembler.AcceptComputation(state, applied);
            state.ClearModuleCommit();

            // Structural change: a deeper pallet rebuilds the module sequence.
            inputs.Pallet = new PalletSpecification(42.0, 52.0, 60.0, 1000.0, "kg");
            var rebuilt = assembler.Build(state, inputs);
            Assert.True(rebuilt.IsValid, rebuilt.Error);

            var survivor = rebuilt.System.Structure.Modules.First(module => module.ModuleId == headerId);
            Assert.False(survivor.UseCalculatedHeaderConfiguration);
            Assert.Equal(40.0, survivor.AssociatedFrameConfiguration.PanelClear, 4);
            Assert.Equal(55.0, survivor.Length, 6);
            Assert.Equal(55.0, survivor.AssociatedFrameConfiguration.Depth, 6);   // adapted
            Assert.Contains(headerId, state.LastModuleReconciliation.Preserved);
        }

        [Fact]
        public void AnIndividualRestore_ReturnsTheModuleToItsCalculatedValues_AndIsReportedAsRestored()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headerId = state.ModuleSession.Modules.First(module => module.IsHeader).ModuleId;
            var calculated = state.WorkingBaseline.Structure.Modules.First(module => module.ModuleId == headerId).Length;

            state.ModuleSession.SetLength(headerId, 55.0);
            state.ModuleSession.SetHeaderConfiguration(headerId, CustomHeader(40.0));
            state.CommitModuleEdits();
            var customized = assembler.Build(state, inputs);
            assembler.AcceptComputation(state, customized);
            state.ClearModuleCommit();
            Assert.Equal(55.0, customized.System.Structure.Modules.First(m => m.ModuleId == headerId).Length, 6);

            state.ModuleSession.RestoreModule(headerId);
            state.CommitModuleEdits();
            var restored = assembler.Build(state, inputs);
            Assert.True(restored.IsValid, restored.Error);

            var module = restored.System.Structure.Modules.First(m => m.ModuleId == headerId);
            Assert.Equal(calculated, module.Length, 6);            // the CALCULATED length is back
            Assert.True(module.UseCalculatedHeaderConfiguration);
            Assert.False(module.IsManualOverride);
            Assert.Contains(headerId, state.LastModuleReconciliation.Restored);
            Assert.False(state.LastModuleReconciliation.LostAnything);
        }

        [Fact]
        public void AStandardRestore_DiscardsEveryCustomization_AtOnce()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var ids = state.ModuleSession.Modules.Where(module => module.IsLengthBearing).Take(2).Select(m => m.ModuleId).ToList();
            foreach (var id in ids) state.ModuleSession.SetLength(id, 57.0);
            state.CommitModuleEdits();
            assembler.AcceptComputation(state, assembler.Build(state, inputs));
            state.ClearModuleCommit();

            state.ModuleSession.RequestStandardRestore();
            state.CommitModuleEdits();
            var reset = assembler.Build(state, inputs);

            Assert.True(reset.IsValid, reset.Error);
            Assert.DoesNotContain(reset.System.Structure.Modules, module => module.IsManualOverride);
            Assert.Empty(state.LastModuleReconciliation.Preserved);
        }

        [Fact]
        public void AModuleLostToAStructuralReduction_IsReported_NotSilentlyDropped()
        {
            var assembler = Assembler();
            var state = new PushBackEditorState();
            state.Structure.Fronts[0].PalletsDeep = 10;
            var inputs = Inputs(palletsDeep: 10);
            assembler.AcceptComputation(state, assembler.Build(state, inputs));

            var lastId = state.ModuleSession.Modules.Last(module => module.IsHeader).ModuleId;
            state.ModuleSession.SetLength(lastId, 51.0);
            state.CommitModuleEdits();
            assembler.AcceptComputation(state, assembler.Build(state, inputs));
            state.ClearModuleCommit();

            state.Structure.Fronts[0].PalletsDeep = 4;   // the rack shrinks: that module no longer exists
            var shrunk = assembler.Build(state, inputs);

            Assert.True(shrunk.IsValid, shrunk.Error);
            Assert.DoesNotContain(shrunk.System.Structure.Modules, module => module.ModuleId == lastId);
            Assert.Contains(lastId, state.LastModuleReconciliation.Removed);
            Assert.True(state.LastModuleReconciliation.LostAnything);
            Assert.Contains(lastId, state.LastModuleReconciliation.Describe());
        }

        [Fact]
        public void AGrownRack_GetsCalculatedModules_ForTheNewPositions()
        {
            var assembler = Assembler();
            var state = new PushBackEditorState();
            state.Structure.Fronts[0].PalletsDeep = 4;
            var inputs = Inputs(palletsDeep: 4);
            assembler.AcceptComputation(state, assembler.Build(state, inputs));
            var before = state.ModuleSession.Modules.Count;
            Assert.True(before > 0);

            state.Structure.Fronts[0].PalletsDeep = 8;
            var grown = assembler.Build(state, inputs);

            Assert.True(grown.IsValid, grown.Error);
            Assert.True(grown.System.Structure.Modules.Count > before);
            Assert.All(grown.System.Structure.Modules, module => Assert.False(module.IsManualOverride));
        }

        // ===== Separadores: solo longitud (decision del Owner y correccion del checklist) =======================

        [Fact]
        public void ASeparatorsManualLength_IsHonouredAndSurvives_ARebuild()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var separatorId = state.ModuleSession.Modules
                .First(module => module.Kind == DynamicRackModuleKind.Separator).ModuleId;

            state.ModuleSession.SetLength(separatorId, 39.0);
            state.CommitModuleEdits();
            var applied = assembler.Build(state, inputs);
            assembler.AcceptComputation(state, applied);
            state.ClearModuleCommit();
            Assert.Equal(39.0, applied.System.Structure.Modules.First(m => m.ModuleId == separatorId).Length, 6);

            inputs.Pallet = new PalletSpecification(42.0, 52.0, 60.0, 1000.0, "kg");
            var rebuilt = assembler.Build(state, inputs);

            Assert.Equal(39.0, rebuilt.System.Structure.Modules.First(m => m.ModuleId == separatorId).Length, 6);
            Assert.Contains(separatorId, state.LastModuleReconciliation.Preserved);
        }

        [Fact]
        public void ASeparatorHasNothingButLength_TheSessionRefusesAHeaderConfiguration()
        {
            var assembler = Assembler();
            var state = Live(assembler, out _);
            var separatorId = state.ModuleSession.Modules
                .First(module => module.Kind == DynamicRackModuleKind.Separator).ModuleId;

            Assert.False(state.ModuleSession.SetHeaderConfiguration(separatorId, CustomHeader(40.0)));
            Assert.False(state.ModuleSession.ResetHeaderToCalculated(separatorId));
            Assert.True(state.ModuleSession.SetLength(separatorId, 39.0));
        }

        // ===== Una sola resolucion para preview, cuatro vistas y BOM ============================================

        [Fact]
        public void OneBuild_ResolvesOnce_AndTheFourViewsAndTheBomComeFromThatSameSystem()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headerId = state.ModuleSession.Modules.First(module => module.IsHeader).ModuleId;
            state.ModuleSession.SetLength(headerId, 57.0);
            state.CommitModuleEdits();

            var computation = assembler.Build(state, inputs);

            Assert.True(computation.IsValid, computation.Error);
            Assert.NotNull(computation.LateralPlan);
            Assert.NotNull(computation.FrontalEntradaSalida);
            Assert.NotNull(computation.FrontalPosterior);
            Assert.NotNull(computation.PlantaPlan);
            Assert.NotNull(computation.Bom);
            Assert.NotEmpty(computation.LateralCortes);

            // The customized length is the one the resolved system carries, so every consumer sees the same rack.
            Assert.Equal(57.0, computation.System.Structure.Modules.First(m => m.ModuleId == headerId).Length, 6);
            Assert.Equal(
                computation.System.Structure.TotalLength,
                computation.Design.Structure.Modules.Sum(module => module.Length),
                6);
        }

        [Fact]
        public void ALongerModule_MovesTheRack_InTheFourViews()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var before = assembler.Build(state, inputs);
            var headerId = state.ModuleSession.Modules.First(module => module.IsHeader).ModuleId;

            state.ModuleSession.SetLength(headerId, 70.0);
            state.CommitModuleEdits();
            var after = assembler.Build(state, inputs);

            Assert.True(after.IsValid, after.Error);
            Assert.NotEqual(before.System.Structure.TotalLength, after.System.Structure.TotalLength);
            Assert.NotEqual(PlanSignature(before.LateralPlan), PlanSignature(after.LateralPlan));
            Assert.NotEqual(PlanSignature(before.PlantaPlan), PlanSignature(after.PlantaPlan));
        }

        // ===== PB-013 y I-33 intactos ==========================================================================

        [Fact]
        public void PB013_TheGeneralPalletHeight_StaysInert_EvenWithACustomCabecera()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headerId = state.ModuleSession.Modules.First(module => module.IsHeader).ModuleId;
            state.ModuleSession.SetHeaderConfiguration(headerId, CustomHeader(40.0));
            state.CommitModuleEdits();
            var withCustom = assembler.Build(state, inputs);
            assembler.AcceptComputation(state, withCustom);
            state.ClearModuleCommit();

            // The rack-wide pallet HEIGHT is a mirror of the cell, never an input of its own: the assembler reads it
            // from the inputs the window keeps loaded, and nothing about a custom cabecera turns it into a driver.
            var height = withCustom.System.Structure.Fronts[0].LoadBeamLevels.Count;
            var again = assembler.Build(state, inputs);
            Assert.Equal(height, again.System.Structure.Fronts[0].LoadBeamLevels.Count);
            Assert.Equal(
                withCustom.System.Structure.Pallet.Height,
                again.System.Structure.Pallet.Height,
                6);
        }

        [Fact]
        public void I33_ABlankFront_IsNeverReactivated_AndASuppressedBoundaryIsNeverRecreated_ByAModuleEdit()
        {
            var assembler = Assembler();
            var state = new PushBackEditorState();
            state.SetFrontCount(4);
            state.SetActive(1, false);
            state.SetActive(2, false);            // two adjacent blanks: the boundary between them disappears
            var inputs = Inputs();
            var seed = assembler.Build(state, inputs);
            Assert.True(seed.IsValid, seed.Error);
            assembler.AcceptComputation(state, seed);

            var activationBefore = DynamicFrontActivation.FrontActivation(seed.System.Structure).ToList();
            var boundariesBefore = DynamicFrontActivation.PresentBoundaries(seed.System.Structure).ToList();
            Assert.Contains(false, activationBefore);
            Assert.DoesNotContain(2, boundariesBefore);   // the shared boundary of the two blanks does not exist

            var headerId = state.ModuleSession.Modules.First(module => module.IsHeader).ModuleId;
            state.ModuleSession.SetLength(headerId, 57.0);
            state.ModuleSession.SetHeaderConfiguration(headerId, CustomHeader(40.0));
            state.CommitModuleEdits();
            var edited = assembler.Build(state, inputs);

            Assert.True(edited.IsValid, edited.Error);
            Assert.Equal(activationBefore, DynamicFrontActivation.FrontActivation(edited.System.Structure));
            Assert.Equal(boundariesBefore, DynamicFrontActivation.PresentBoundaries(edited.System.Structure));
        }

        [Fact]
        public void AJaggedRack_KeepsItsShape_AcrossAModuleEdit()
        {
            var assembler = Assembler();
            var state = new PushBackEditorState();
            state.SetFrontCount(3);
            state.AdjustLevels(0, 2);
            state.AdjustLevels(2, 1);             // jagged: 5 / 3 / 4 levels
            var inputs = Inputs();
            var seed = assembler.Build(state, inputs);
            Assert.True(seed.IsValid, seed.Error);
            assembler.AcceptComputation(state, seed);
            var shape = seed.System.Structure.Fronts.Select(front => front.LoadLevels).ToList();
            Assert.True(shape.Distinct().Count() > 1, "the fixture must actually be jagged");

            var headerId = state.ModuleSession.Modules.First(module => module.IsHeader).ModuleId;
            state.ModuleSession.SetLength(headerId, 57.0);
            state.CommitModuleEdits();
            var edited = assembler.Build(state, inputs);

            Assert.True(edited.IsValid, edited.Error);
            Assert.Equal(shape, edited.System.Structure.Fronts.Select(front => front.LoadLevels));
        }

        // ===== Persistencia: round-trip, legacy, biblioteca, Xrecord, campos desconocidos =======================

        [Fact]
        public void ACustomizedModule_SurvivesTheProjectRoundTrip()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headerId = state.ModuleSession.Modules.First(module => module.IsHeader).ModuleId;
            state.ModuleSession.SetLength(headerId, 55.0);
            state.ModuleSession.SetHeaderConfiguration(headerId, CustomHeader(40.0));
            state.CommitModuleEdits();
            var design = assembler.BuildDesign(state, inputs);

            var store = new RackProjectStore();
            var reloaded = store.Deserialize(store.Serialize(RackProject.ForPushBack(design))).PushBackDesign;

            var module = reloaded.Structure.Modules.First(m => m.ModuleId == headerId);
            Assert.Equal(55.0, module.Length, 6);
            Assert.False(module.UseCalculatedHeaderConfiguration);
            Assert.Equal(40.0, module.HeaderConfiguration.PanelClear, 4);
        }

        [Fact]
        public void ACustomizedModule_SurvivesTheXrecordEnvelope_WithTheSameGuid()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headerId = state.ModuleSession.Modules.First(module => module.IsHeader).ModuleId;
            state.ModuleSession.SetLength(headerId, 55.0);
            state.CommitModuleEdits();
            var design = assembler.BuildDesign(state, inputs);

            var store = new RackProjectStore();
            var embedStore = new RackEmbedStore();
            var guid = System.Guid.NewGuid().ToString();
            var payload = embedStore.Serialize(RackEmbedComposer.Compose(
                null,
                RackEmbedDocument.KindPushBack,
                guid,
                "PB-35",
                RackEmbedDocument.ViewLateral,
                0,
                store.Serialize(RackProject.ForPushBack(design))));

            var embed = embedStore.Deserialize(payload);
            var reloaded = store.Deserialize(embed.Design).PushBackDesign;

            Assert.Equal(guid, embed.Id);
            Assert.Equal(RackEmbedDocument.KindPushBack, embed.Kind);
            Assert.Equal(55.0, reloaded.Structure.Modules.First(m => m.ModuleId == headerId).Length, 6);
        }

        [Fact]
        public void ACustomizedModule_PreservesUnknownJsonFields_I11()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headerId = state.ModuleSession.Modules.First(module => module.IsHeader).ModuleId;
            state.ModuleSession.SetLength(headerId, 55.0);
            state.CommitModuleEdits();
            var design = assembler.BuildDesign(state, inputs);

            var store = new RackProjectStore();
            var node = JsonNode.Parse(store.Serialize(RackProject.ForPushBack(design)));
            node["PushBack"]["CampoDesconocidoDeOtraVersion"] = "conservame";

            var loaded = store.Deserialize(node.ToJsonString());
            var rewritten = JsonNode.Parse(
                store.Serialize(RackProject.ForPushBack(loaded.PushBackDesign).WithSourceMetadataFrom(loaded)));

            Assert.Equal("conservame", rewritten["PushBack"]["CampoDesconocidoDeOtraVersion"].GetValue<string>());
            Assert.Equal(
                55.0,
                store.Deserialize(rewritten.ToJsonString()).PushBackDesign.Structure.Modules
                    .First(m => m.ModuleId == headerId).Length,
                6);
        }

        [Fact]
        public void ALegacyDocument_WithoutModuleProvenance_LoadsAndStillEditsByModuleId()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var design = assembler.BuildDesign(state, inputs);

            // An older build wrote no UseCalculatedHeaderConfiguration on the modules at all.
            var store = new RackProjectStore();
            var node = JsonNode.Parse(store.Serialize(RackProject.ForPushBack(design)));
            foreach (var module in node["PushBack"]["Structure"]["Modules"].AsArray())
            {
                module.AsObject().Remove("UseCalculatedHeaderConfiguration");
            }

            var loaded = store.Deserialize(node.ToJsonString()).PushBackDesign;
            var reloadedState = new PushBackEditorState();
            var reloadedInputs = reloadedState.LoadFromDesign(loaded, assembler.Resolver);

            // The legacy fallback keeps every persisted cabecera as custom; the session addresses them by id all the same.
            var headerId = reloadedState.ModuleSession.Modules.First(module => module.IsHeader).ModuleId;
            Assert.True(reloadedState.ModuleSession.SetLength(headerId, 58.0));
            reloadedState.CommitModuleEdits();

            var rebuilt = assembler.Build(reloadedState, reloadedInputs);
            Assert.True(rebuilt.IsValid, rebuilt.Error);
            Assert.Equal(58.0, rebuilt.System.Structure.Modules.First(m => m.ModuleId == headerId).Length, 6);
        }

        [Fact]
        public void ACustomizedModule_SurvivesTheDesignLibrary()
        {
            var assembler = Assembler();
            var state = Live(assembler, out var inputs);
            var headerId = state.ModuleSession.Modules.First(module => module.IsHeader).ModuleId;
            state.ModuleSession.SetLength(headerId, 55.0);
            state.ModuleSession.SetHeaderConfiguration(headerId, CustomHeader(40.0));
            state.CommitModuleEdits();
            var design = assembler.BuildDesign(state, inputs);

            var store = new RackProjectStore();
            var json = store.Serialize(RackProject.ForPushBack(design));
            var entry = store.Deserialize(json);
            Assert.Equal(RackSystemKind.PushBack, entry.Kind);

            // Reopening it in a fresh editor keeps the customization addressable and applied.
            var reopened = new PushBackEditorState();
            var reopenedInputs = reopened.LoadFromDesign(entry.PushBackDesign, assembler.Resolver);
            var built = assembler.Build(reopened, reopenedInputs);

            Assert.True(built.IsValid, built.Error);
            var module = built.System.Structure.Modules.First(m => m.ModuleId == headerId);
            Assert.Equal(55.0, module.Length, 6);
            Assert.False(module.UseCalculatedHeaderConfiguration);
            Assert.Equal(40.0, module.AssociatedFrameConfiguration.PanelClear, 4);
        }

        // ===== Fixtures ========================================================================================

        private static string Signature(PushBackDesign design)
            => string.Join("|", design.Structure.Modules.Select(module => string.Join(
                ",",
                module.ModuleId,
                module.Kind.ToString(),
                module.Length.ToString("0.####"),
                module.IsManualOverride.ToString(),
                module.UseCalculatedHeaderConfiguration.ToString(),
                module.HeaderConfiguration?.PanelClear.ToString("0.####") ?? "-")));

        private static string PlanSignature(HeaderRunPlan plan)
            => plan == null
                ? string.Empty
                : string.Join("|", plan.Flatten().Instances.Select(instance => string.Join(
                    ",",
                    instance.PieceId,
                    instance.Insertion.X.ToString("0.###"),
                    instance.Insertion.Y.ToString("0.###"))));
    }
}
