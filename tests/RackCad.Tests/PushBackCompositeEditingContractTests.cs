using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (ronda 7) — LA CAPA DE EDICIÓN: ActiveSide es CONTEXTO, no autoridad, y la defensa se decide por lado.
    ///
    /// <para>
    /// El modelo físico ya está cerrado. Esta ronda fija que la edición no lo contamine: cambiar de lado no muta
    /// nada, editar A no toca B, los Restore están acotados a su objetivo, Preview/Cancel son transaccionales y el
    /// documento reproduce exactamente lo que se guardó.
    /// </para>
    /// <para>
    /// La ronda 7 añadió aquí una intención de defensa POR LADO. El dueño la rechazó: la granularidad correcta es el
    /// POSTE FÍSICO, y ya existía —<c>SafetyPostDefense.PostIndex</c>, editable desde «Elementos de seguridad»—. Ese
    /// contrato se retiró y las pruebas de la defensa viven ahora en
    /// <see cref="PushBackDefensePerPostTests"/>, sobre el mecanismo que el producto ya tenía.
    /// </para>
    /// </summary>
    public class PushBackCompositeEditingContractTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackEditorInputs Inputs()
        {
            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            foreach (var selection in new PushBackSafetyAuthority(Catalog).Defaults())
            {
                inputs.SafetySelections.Add(selection);
            }

            return inputs;
        }

        private static PushBackCompositeEditorState State(
            int slots = 3, IReadOnlyCollection<int> blanksA = null, IReadOnlyCollection<int> blanksB = null,
            PushBackCellTopology topology = PushBackCellTopology.Encontradas)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(slots);
            state.SetSideBPresent(true);
            for (var slot = 0; slot < slots; slot++)
            {
                state.SetSlotPresent(PushBackSide.B, slot, true);
            }

            state.SetDefaults(topology, PushBackRunDirection.AToB);
            foreach (var slot in blanksA ?? Array.Empty<int>()) { Assert.True(state.SetSlotPresent(PushBackSide.A, slot, false)); }
            foreach (var slot in blanksB ?? Array.Empty<int>()) { Assert.True(state.SetSlotPresent(PushBackSide.B, slot, false)); }
            return state;
        }

        private static PushBackSystem Build(PushBackCompositeEditorState state, PushBackEditorInputs inputs = null)
        {
            var computation = new PushBackCompositeEditorAssembler(Catalog).Build(state, inputs ?? Inputs(), Catalog);
            Assert.NotNull(computation.System);
            return computation.System;
        }

        private static PushBackDesign Design(PushBackCompositeEditorState state, PushBackEditorInputs inputs = null)
        {
            var computation = new PushBackCompositeEditorAssembler(Catalog).Build(state, inputs ?? Inputs(), Catalog);
            Assert.NotNull(computation.Design);
            return computation.Design;
        }

        private static PushBackDesign RoundTrip(PushBackDesign design)
            => JsonSerializer.Deserialize<PushBackDesignDocument>(
                JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(design))).ToDomain();

        // ---- lecturas semanticas ---------------------------------------------------------------------------------

        /// <summary>Una firma semántica del estado del editor: todo lo que una edición puede tocar.</summary>
        private static string StateSignature(PushBackCompositeEditorState state)
        {
            var parts = new List<string>
            {
                FormattableString.Invariant($"gap={state.Gap:0.####}")
                    + $"|sep={state.CentralSeparator}|ovA={state.StructureOverrideA}|ovB={state.StructureOverrideB}"
                    + $"|topo={state.DefaultTopology}|dir={state.DefaultDirection}"
                    + $"|bPresent={state.SideBPresent}|slots={state.SlotCount}"
            };

            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                var editor = state.Of(side);
                for (var index = 0; index < editor.Structure.Count; index++)
                {
                    var front = editor.Structure.Fronts[index];
                    parts.Add($"{side}|f{index}|niveles={front.LoadLevels}|deep={front.PalletsDeep}"
                        + $"|presente={state.IsSlotPresent(side, index)}");
                    for (var level = 0; level < front.LoadLevels; level++)
                    {
                        var cell = editor.Cell(index, level);
                        parts.Add($"{side}|c{index}.{level}|deep={cell?.PalletsDeepOverride}|tarima={cell?.DrawPallet}");
                    }
                }
            }

            parts.Sort(StringComparer.Ordinal);
            return string.Join("\n", parts);
        }

        private static IReadOnlyList<(double X, double Y)> Defenses(PushBackSystem system)
        {
            var catalog = Catalog;
            return new PushBackSystemPlantaBuilder().BuildPlan(system, catalog).Flatten().Instances
                .Where(instance => instance.Role == HeaderBlockRole.Safety)
                .Where(instance =>
                {
                    var element = catalog.SafetyElements?.FirstOrDefault(entry => string.Equals(
                        entry?.Id, instance.PieceId, StringComparison.OrdinalIgnoreCase));
                    return element != null
                           && SelectiveSafetyDefaults.IsType(element.Type, SelectiveSafetyDefaults.DefensaType);
                })
                .Select(instance => (Math.Round(instance.Insertion.X, 3), Math.Round(instance.Insertion.Y, 3)))
                .Distinct().OrderBy(entry => entry.Item1).ThenBy(entry => entry.Item2).ToList();
        }

        private static int BomDefenses(PushBackSystem system)
        {
            var catalog = Catalog;
            return PushBackBomBuilder.Build(system, catalog).Components
                .Where(component => catalog.SafetyElements?.Any(entry =>
                    string.Equals(entry?.Id, component.ProfileId, StringComparison.OrdinalIgnoreCase)
                    && SelectiveSafetyDefaults.IsType(entry.Type, SelectiveSafetyDefaults.DefensaType)) ?? false)
                .Sum(component => component.Quantity);
        }

        // ======================================================================================================
        // ActiveSide: contexto, no autoridad
        // ======================================================================================================

        [Fact]
        public void SwitchingActiveSide_DoesNotMutateModel()
        {
            var state = State();
            var before = StateSignature(state);
            var drawnBefore = Build(state).Structure.TotalLength;

            state.SetActiveSide(PushBackSide.B);
            state.SetActiveSide(PushBackSide.A);
            state.SetActiveSide(PushBackSide.B);
            state.SetActiveSide(PushBackSide.A);

            Assert.Equal(before, StateSignature(state));
            Assert.Equal(drawnBefore, Build(state).Structure.TotalLength, 6);
        }

        [Fact]
        public void EditingA_DoesNotChangeB()
        {
            var state = State();
            var bBefore = StateSignature(state).Split('\n').Where(line => line.StartsWith("B|", StringComparison.Ordinal)).ToList();

            state.SetActiveSide(PushBackSide.A);
            state.Of(PushBackSide.A).AdjustLevels(0, 1);
            state.Of(PushBackSide.A).ToggleCell(0, 0, false);
            state.Of(PushBackSide.A).ApplyPalletsDeep(7, DynamicRackCellScope.Cell);

            var bAfter = StateSignature(state).Split('\n').Where(line => line.StartsWith("B|", StringComparison.Ordinal)).ToList();
            Assert.Equal(bBefore, bAfter);
        }

        [Fact]
        public void EditingB_DoesNotChangeA()
        {
            var state = State();
            var aBefore = StateSignature(state).Split('\n').Where(line => line.StartsWith("A|", StringComparison.Ordinal)).ToList();

            state.SetActiveSide(PushBackSide.B);
            state.Of(PushBackSide.B).AdjustLevels(1, 2);
            state.Of(PushBackSide.B).ToggleCell(1, 0, false);
            state.Of(PushBackSide.B).ApplyPalletsDeep(3, DynamicRackCellScope.Cell);

            var aAfter = StateSignature(state).Split('\n').Where(line => line.StartsWith("A|", StringComparison.Ordinal)).ToList();
            Assert.Equal(aBefore, aAfter);
        }

        /// <summary>Una propiedad COMPARTIDA por contrato —el hueco— cambia para los dos; una independiente, no.</summary>
        [Fact]
        public void SharedProperty_ChangesBothOnlyWhenContractSaysShared()
        {
            var state = State();
            var lengthBefore = Build(state).Structure.TotalLength;

            state.SetGap(48.0);
            Assert.NotEqual(lengthBefore, Build(state).Structure.TotalLength);

            // …y los niveles NO son compartidos: subir A no sube B.
            var levelsB = state.Of(PushBackSide.B).Structure.Fronts[0].LoadLevels;
            state.Of(PushBackSide.A).AdjustLevels(0, 1);
            Assert.Equal(levelsB, state.Of(PushBackSide.B).Structure.Fronts[0].LoadLevels);
        }

        [Fact]
        public void ModuleIdsSurviveSideSwitch()
        {
            var state = State();
            var before = Build(state).Structure.Modules.Select(module => module.ModuleId).ToList();

            state.SetActiveSide(PushBackSide.B);
            state.SetActiveSide(PushBackSide.A);

            Assert.Equal(before, Build(state).Structure.Modules.Select(module => module.ModuleId).ToList());
            Assert.All(before, id => Assert.False(string.IsNullOrWhiteSpace(id)));
        }

        [Fact]
        public void HeaderOverrideSurvivesSideSwitch()
        {
            var state = State();
            var inputs = Inputs();
            inputs.ManualHeaderHeightOverride = 300.0;

            var before = Build(state, inputs).Structure.Fronts.Select(front => front.Height).ToList();
            state.SetActiveSide(PushBackSide.B);
            state.SetActiveSide(PushBackSide.A);

            Assert.Equal(before, Build(state, inputs).Structure.Fronts.Select(front => front.Height).ToList());
            Assert.All(before, height => Assert.Equal(300.0, height, 6));
        }

        // ======================================================================================================
        // Restore acotado al objetivo
        // ======================================================================================================

        [Fact]
        public void RestoreCell_IsSideScoped()
        {
            var state = State();
            state.Of(PushBackSide.A).ToggleCell(0, 0, false);
            state.Of(PushBackSide.A).ApplyPalletsDeep(7, DynamicRackCellScope.Cell);
            state.Of(PushBackSide.B).ToggleCell(0, 0, false);
            state.Of(PushBackSide.B).ApplyPalletsDeep(3, DynamicRackCellScope.Cell);

            Assert.Equal(7, state.Of(PushBackSide.A).Cell(0, 0).PalletsDeepOverride);
            Assert.Equal(3, state.Of(PushBackSide.B).Cell(0, 0).PalletsDeepOverride);

            // Restaurar la celda de A es EXACTAMENTE quitar su override; el de B no se toca.
            state.Of(PushBackSide.A).ApplyPalletsDeep(null, DynamicRackCellScope.Cell);
            Assert.Null(state.Of(PushBackSide.A).Cell(0, 0).PalletsDeepOverride);
            Assert.Equal(3, state.Of(PushBackSide.B).Cell(0, 0).PalletsDeepOverride);
        }

        [Fact]
        public void RestoreLevel_IsSideScoped()
        {
            var state = State();
            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                state.Of(side).ToggleCell(0, 0, false);
                state.Of(side).ApplyPalletsDeep(6, DynamicRackCellScope.Level);
            }

            state.Of(PushBackSide.A).ApplyPalletsDeep(null, DynamicRackCellScope.Level);

            Assert.All(Enumerable.Range(0, state.Of(PushBackSide.A).Structure.Count),
                index => Assert.Null(state.Of(PushBackSide.A).Cell(index, 0)?.PalletsDeepOverride));
            Assert.Contains(Enumerable.Range(0, state.Of(PushBackSide.B).Structure.Count),
                index => state.Of(PushBackSide.B).Cell(index, 0)?.PalletsDeepOverride == 6);
        }

        /// <summary>Restaurar la estructura de un lado devuelve su propuesta ACTUAL, no una antigua.</summary>
        [Fact]
        public void RestoreUsesCurrentDerivedProposal()
        {
            var state = State();
            var proposal = Build(state).Composite.SideA.ProposedStructure;

            state.SetStructureOverride(PushBackSide.A, proposal + 3);
            Assert.Equal(proposal + 3, Build(state).Composite.SideA.EffectiveStructure);

            state.RestoreStructure(PushBackSide.A);
            var after = Build(state).Composite;
            Assert.Null(state.StructureOverrideA);
            Assert.Equal(after.SideA.ProposedStructure, after.SideA.EffectiveStructure);
            // …y el lado B no cambia por el Restore de A.
            Assert.Null(state.StructureOverrideB);
        }

        // ======================================================================================================
        // Blancos: no bloquean el otro lado
        // ======================================================================================================

        [Fact]
        public void BlankA_DoesNotDisableBControls()
        {
            var state = State(blanksA: new[] { 0 });

            Assert.False(state.IsSlotPresent(PushBackSide.A, 0));
            Assert.True(state.IsSlotPresent(PushBackSide.B, 0));
            // El lado B se sigue pudiendo editar en esa misma ranura.
            state.SetActiveSide(PushBackSide.B);
            state.Of(PushBackSide.B).AdjustLevels(0, 1);
            Assert.True(state.Of(PushBackSide.B).Structure.Fronts[0].LoadLevels > 0);
        }

        [Fact]
        public void BlankB_DoesNotDisableAControls()
        {
            var state = State(blanksB: new[] { 0 });

            Assert.True(state.IsSlotPresent(PushBackSide.A, 0));
            Assert.False(state.IsSlotPresent(PushBackSide.B, 0));
            state.SetActiveSide(PushBackSide.A);
            state.Of(PushBackSide.A).AdjustLevels(0, 1);
            Assert.True(state.Of(PushBackSide.A).Structure.Fronts[0].LoadLevels > 0);
        }

        /// <summary>Un override manual imposible en A no impide seguir resolviendo el rack por B.</summary>
        [Fact]
        public void LocalizedBlocking_DoesNotBlockIndependentSide()
        {
            var state = State();
            var blockingA = state.IntentDiagnostics().Count;
            state.SetStructureOverride(PushBackSide.A, 1);   // por debajo de la demanda: diagnostico de A

            var diagnostics = state.IntentDiagnostics();
            Assert.True(diagnostics.Count >= blockingA);
            // El diagnostico existe, pero la estructura de B conserva su override propio, intacto.
            Assert.Null(state.StructureOverrideB);
            state.SetStructureOverride(PushBackSide.B, 12);
            Assert.Equal(12, state.StructureOverrideB);
            Assert.Equal(1, state.StructureOverrideA);
        }

        // ======================================================================================================
        // Snapshot / Preview transaccional
        // ======================================================================================================

        [Fact]
        public void PreviewCancel_IsTransactional()
        {
            var state = State();
            var baseline = state.Snapshot();
            var before = StateSignature(state);

            state.Of(PushBackSide.A).AdjustLevels(0, 2);
            state.SetGap(36.0);
            Assert.NotEqual(before, StateSignature(state));

            state.Restore(baseline);   // Cancel
            Assert.Equal(before, StateSignature(state));
        }

        [Fact]
        public void PreviewAcrossSideSwitch_DoesNotLeakState()
        {
            var state = State();
            var baseline = state.Snapshot();

            state.SetActiveSide(PushBackSide.A);
            state.Of(PushBackSide.A).AdjustLevels(0, 1);
            Build(state);                                   // Preview de A
            state.SetActiveSide(PushBackSide.B);
            state.Of(PushBackSide.B).AdjustLevels(0, 2);
            Build(state);                                   // Preview tras cambiar de lado

            state.Restore(baseline);                        // Cancel
            Assert.Equal(StateSignature(State()), StateSignature(state));
        }

        [Fact]
        public void AcceptCommitsBothSides()
        {
            var state = State();
            state.SetActiveSide(PushBackSide.A);
            state.Of(PushBackSide.A).AdjustLevels(0, 1);
            state.SetActiveSide(PushBackSide.B);
            state.Of(PushBackSide.B).AdjustLevels(1, 2);
            state.SetActiveSide(PushBackSide.A);

            var design = Design(state);          // Accept
            var reloaded = RoundTrip(design);

            // Los dos lados llegan al documento con SUS ediciones: ninguna sobreescribe a la otra.
            Assert.Equal(state.Of(PushBackSide.A).Structure.Fronts[0].LoadLevels, reloaded.Structure.Fronts[0].LoadLevels);
            Assert.NotNull(reloaded.SideB);
            Assert.Equal(state.Of(PushBackSide.B).Structure.Fronts[1].LoadLevels, reloaded.SideB.Fronts[1].LoadLevels);
            Assert.Equal(
                new PushBackResolver(Catalog).Resolve(design).Structure.TotalLength,
                new PushBackResolver(Catalog).Resolve(reloaded).Structure.TotalLength, 6);
        }

        /// <summary>El estado compuesto de edición sobrevive al guardado y a RACKEDITAR.</summary>
        [Fact]
        public void RackEditar_RoundTripsCompositeUiState()
        {
            var state = State(blanksA: new[] { 0 });
            state.SetGap(24.0);
            state.SetCentralSeparator(true);
            state.SetStructureOverride(PushBackSide.B, 14);
            state.Of(PushBackSide.A).AdjustLevels(1, 1);
            state.Of(PushBackSide.B).ToggleCell(1, 0, false);
            state.Of(PushBackSide.B).ApplyPalletsDeep(3, DynamicRackCellScope.Cell);
            var design = Design(state);
            var reloaded = RoundTrip(design);

            Assert.Equal(24.0, reloaded.Composite.Gap, 6);
            Assert.True(reloaded.Composite.CentralSeparator);
            Assert.Equal(14, reloaded.Composite.StructureOverrideB);

            // Y el sistema reconstruido dibuja lo mismo que el original.
            var before = Build(state);
            var after = new PushBackResolver(Catalog).Resolve(reloaded);
            Assert.Equal(Defenses(before), Defenses(after));   // la seguridad reconstruida es la misma
            Assert.Equal(before.Structure.TotalLength, after.Structure.TotalLength, 6);
            Assert.Equal(
                before.Structure.Modules.Select(module => module.ModuleId).ToList(),
                after.Structure.Modules.Select(module => module.ModuleId).ToList());
        }

        /// <summary>El lado B ausente no resucita por defecto, ni por intencion de defensa, ni por recargar.</summary>
        [Fact]
        public void AbsentB_DoesNotResurrect()
        {
            var state = State(blanksB: new[] { 0, 1 });
            var reloaded = RoundTrip(Design(state));

            Assert.Contains(0, reloaded.Composite.AbsentSlotsB);
            Assert.Contains(1, reloaded.Composite.AbsentSlotsB);
            Assert.DoesNotContain(2, reloaded.Composite.AbsentSlotsB);

            // Y el sistema reconstruido no resucita esas ranuras.
            var system = new PushBackResolver(Catalog).Resolve(reloaded);
            var runs = PushBackRuns.Resolve(system).Runs;
            Assert.DoesNotContain(runs, run => run.Slot == 0 && run.LowSide == PushBackSide.B);
            Assert.DoesNotContain(runs, run => run.Slot == 1 && run.LowSide == PushBackSide.B);
            Assert.Contains(runs, run => run.Slot == 2 && run.LowSide == PushBackSide.B);
        }

        /// <summary>Y 6A / 6F siguen intactos: el BOM de intermedios y las botas no cambian por esta ronda.</summary>
        [Fact]
        public void PhysicalContractsFromEarlierRounds_AreUntouched()
        {
            var system = Build(State());
            var catalog = Catalog;
            var bootId = catalog.SafetyElements
                .First(entry => SelectiveSafetyDefaults.IsType(entry.Type, SelectiveSafetyDefaults.BotaType)).Id;

            var drawnBoots = new PushBackSystemPlantaBuilder().BuildPlan(system, catalog).Flatten().Instances
                .Count(instance => instance.Role == HeaderBlockRole.Safety
                    && string.Equals(instance.PieceId, bootId, StringComparison.OrdinalIgnoreCase));
            var bomBoots = PushBackBomBuilder.Build(system, catalog).Components
                .Where(component => string.Equals(component.ProfileId, bootId, StringComparison.OrdinalIgnoreCase))
                .Sum(component => component.Quantity);

            Assert.Equal(drawnBoots, bomBoots);
            Assert.True(drawnBoots > 0);
        }
    }
}
