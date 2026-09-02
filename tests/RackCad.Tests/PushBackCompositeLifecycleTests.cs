using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (A4-MOD-LIFECYCLE, contrato del dueño) — LA SECUENCIA QUE SE ENTREGA ES LA DEL RACK QUE SE RESUELVE
    /// AHORA, Y DORMIR NO ES BORRAR.
    ///
    /// <para>
    /// Son tres cosas distintas: la CAPACIDAD compuesta (el editor conoce el lado B), el diseño EFECTIVAMENTE
    /// compuesto (ahora mismo tiene dos lados fisicos) y la SECUENCIA persistida (<c>M* + GAP + B:*</c>). La unica
    /// pregunta que decide por que camino se resuelve un diseño es <c>design.IsComposite</c>, la misma que usa el
    /// resolver: ni el selector de lado, ni el numero de modulos, ni haber tenido lado B alguna vez.
    /// </para>
    ///
    /// <para>
    /// <b>Medido antes del cambio.</b> Al dejar de ser efectivamente compuesto —se retira el lado B, o se queda sin
    /// ninguna ranura efectiva, que NO es lo mismo— la secuencia compuesta seguia viajando al resolver de un solo
    /// sentido; alli «tantos modulos como posiciones» actuaba de segunda autoridad y reconstruia la receta estandar:
    /// <c>A:M2</c> volvia de 30" a 48" en ese mismo recalculo, el informe declaraba conservados modulos que el
    /// diseño resuelto ya no tenia, y reactivar el lado B lo devolvia estandar (<c>B:M2</c> 40" → 48", override de
    /// linea perdido).
    /// </para>
    /// </summary>
    public class PushBackCompositeLifecycleTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackEditorInputs Inputs()
        {
            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            return inputs;
        }

        private static PushBackCompositeEditorState State(int slots = 2, double gap = 54.0)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(slots);
            state.SetSideBPresent(true);
            state.SideB.LoadNew();
            state.SetSlotCount(slots);
            for (var slot = 0; slot < slots; slot++)
            {
                state.SetSlotPresent(PushBackSide.B, slot, true);
            }

            state.SetGap(gap);
            return state;
        }

        /// <summary>El recalculo REAL del editor compuesto.</summary>
        private static PushBackSystem Recompute(PushBackCompositeEditorState state)
        {
            var assembler = new PushBackEditorDesignAssembler(Catalog);
            var design = new PushBackCompositeEditorAssembler(Catalog).BuildDesign(state, Inputs());
            var computation = assembler.BuildFrom(design, PushBackSide.A);
            Assert.True(computation.IsValid, computation.Error);
            assembler.AcceptComputation(state.SideA, computation);
            return computation.System;
        }

        /// <summary>Personaliza los DOS lados y una linea de la mitad B, y lo confirma.</summary>
        private static void Customize(PushBackCompositeEditorState state)
        {
            var session = state.SideA.ModuleSession;
            Assert.True(session.SetLength("M2", 30.0));
            Assert.True(session.SetLength("B:M2", 40.0));
            var configuration = session.HeaderConfigurationCopy("B:M6", 1);
            Assert.NotNull(configuration);
            configuration.Height = 137.0;
            Assert.True(session.ApplyHeaderConfigurationToLine(configuration, 1, new[] { "B:M6" }).Applied);
            state.SideA.CommitModuleEdits();
        }

        private static double LengthOf(PushBackSystem system, string moduleId)
            => system.Structure.Modules
                .Single(module => string.Equals(module.ModuleId, moduleId, StringComparison.Ordinal))
                .Length;

        private static IReadOnlyList<string> Ids(PushBackSystem system)
            => system.Structure.Modules.Select(module => module.ModuleId).ToList();

        private static bool HasTail(PushBackSystem system)
            => Ids(system).Any(PushBackCompositeStructure.IsCompositeTailId);

        private static void AssertCompositeCustomizationsAreBack(PushBackSystem system)
        {
            Assert.Equal(30.0, LengthOf(system, "M2"), 6);
            Assert.Equal(40.0, LengthOf(system, "B:M2"), 6);
            Assert.Contains("GAP", Ids(system));
            Assert.Equal(
                137.0,
                system.Structure.HeaderLineOverrides
                    .Single(line => line.PostIndex == 1 && line.ModuleId == "B:M6").Header.Height,
                6);
        }

        // ---------------------------------------------------------------- R-1

        [Fact]
        public void CompositeModules_DisablingSideBPreservesAIntentAndDormantBIntent()
        {
            var state = State();
            Recompute(state);
            Customize(state);
            Assert.True(HasTail(Recompute(state)));

            state.SetSideBPresent(false);
            var single = Recompute(state);

            // El diseño que se resuelve es el del lado A: sin hueco y sin mitad B...
            Assert.False(HasTail(single));
            // ...y la personalizacion del lado A no se pierde por el camino.
            Assert.Equal(30.0, LengthOf(single, "M2"), 6);
            Recompute(state);
            Assert.Equal(30.0, LengthOf(Recompute(state), "M2"), 6);

            // La intencion de B estaba DORMIDA, no borrada: vuelve entera.
            state.SetSideBPresent(true);
            for (var slot = 0; slot < 2; slot++)
            {
                state.SetSlotPresent(PushBackSide.B, slot, true);
            }

            AssertCompositeCustomizationsAreBack(Recompute(state));
        }

        [Fact]
        public void CompositeModules_NoEffectiveBSlotsUsesSingleSideStructureWithoutDestroyingDormantTail()
        {
            // El lado B sigue DECLARADO, pero sin ninguna ranura efectiva el rack es de un solo sentido: el
            // discriminador no puede ser el interruptor del lado.
            var state = State();
            Recompute(state);
            Customize(state);
            Recompute(state);

            for (var slot = 0; slot < 2; slot++)
            {
                state.SetSlotPresent(PushBackSide.B, slot, false);
            }

            var single = Recompute(state);
            Assert.True(state.SideBPresent);
            Assert.False(HasTail(single));
            Assert.Equal(30.0, LengthOf(single, "M2"), 6);
            Recompute(state);

            for (var slot = 0; slot < 2; slot++)
            {
                state.SetSlotPresent(PushBackSide.B, slot, true);
            }

            AssertCompositeCustomizationsAreBack(Recompute(state));
        }

        [Fact]
        public void CompositeModules_ReconciliationReportOnlyTalksAboutTheResolvedSequence()
        {
            var state = State();
            Recompute(state);
            Customize(state);
            Recompute(state);

            state.SetSideBPresent(false);
            Recompute(state);
            var report = state.SideA.LastModuleReconciliation;

            // Ni «conservado» lo que el diseño resuelto no tiene, ni «perdido» lo que solo esta dormido.
            Assert.Contains("M2", report.Preserved);
            Assert.DoesNotContain("GAP", report.Preserved);
            Assert.DoesNotContain("B:M2", report.Preserved);
            Assert.Empty(report.Removed);
            Assert.False(report.LostAnything);

            // Y al volver, tampoco se declara perdido lo que esta regresando.
            state.SetSideBPresent(true);
            for (var slot = 0; slot < 2; slot++)
            {
                state.SetSlotPresent(PushBackSide.B, slot, true);
            }

            Recompute(state);
            var back = state.SideA.LastModuleReconciliation;
            Assert.Contains("B:M2", back.Preserved);
            Assert.False(back.LostAnything);
        }

        // ---------------------------------------------------------------- N-2

        [Fact]
        public void CompositeModules_NonNestedSlotsSurviveSnapshotAndRepeatedRecompute()
        {
            // Ranuras escalonadas legitimas: la 0 solo del lado A y la ultima solo del lado B. Sus rangos de
            // profundidad NO anidan, y el compuesto lo permite por construccion.
            var state = State(slots: 3, gap: 0.0);
            Recompute(state);
            state.SetSlotPresent(PushBackSide.B, 0, false);
            state.SideA.Structure.Fronts[2].IsActive = false;
            state.SideA.Structure.Fronts[0].PalletsDeep = 6;
            state.SideB.Structure.Fronts[2].PalletsDeep = 2;

            var first = Recompute(state);
            var composite = first.Structure;
            Assert.True(PushBackCompositeStructure.IsCompositeSequence(composite.Modules));

            // La COPIA es donde se perdia: el snapshot no llevaba la identidad compuesta y el siguiente resolve
            // reventaba con «cada frente debe contener la estructura completa del frente con menos fondos».
            var resolver = new DynamicRackSystemResolver(Catalog);
            var snapshot = resolver.Snapshot(
                composite,
                Math.Max(1, composite.LoadBeamLevels.Count),
                composite.Fronts.First().FirstLevelHeight,
                composite.InOutBeamDepth,
                composite.Modules.FirstOrDefault(m => m.IsHeader && m.AssociatedFrameConfiguration?.LeftPost != null)?
                    .AssociatedFrameConfiguration.LeftPost.PostCatalogId);
            snapshot.AllowsNonNestedDepthRanges =
                PushBackCompositeStructure.IsCompositeSequence(composite.Modules);
            Assert.True(snapshot.AllowsNonNestedDepthRanges);
            Assert.Null(Record.Exception(() => resolver.Resolve(snapshot)));

            // Y por el camino del editor: dos recalculos mas, misma topologia y sin excepciones.
            var ids = Ids(first);
            Assert.Equal(ids, Ids(Recompute(state)));
            Assert.Equal(ids, Ids(Recompute(state)));
        }

        // ---------------------------------------------------------------- restaurar

        [Fact]
        public void CompositeModules_StandardRestoreResetsBothATailAndBTailCustomizations()
        {
            var state = State();
            Recompute(state);
            Customize(state);
            var customized = Recompute(state);
            Assert.Equal(30.0, LengthOf(customized, "M2"), 6);
            Assert.Equal(40.0, LengthOf(customized, "B:M2"), 6);

            state.SideA.ModuleSession.RequestStandardRestore();
            state.SideA.CommitModuleEdits();
            var restored = Recompute(state);

            // Un reset es un reset: tambien para la mitad B, que no puede sobrevivir por ir en la cola.
            Assert.NotEqual(30.0, LengthOf(restored, "M2"));
            Assert.NotEqual(40.0, LengthOf(restored, "B:M2"));
            Assert.Empty(restored.Structure.HeaderLineOverrides);

            // Y el hueco sigue siendo fisicamente necesario: no se restaura eliminandolo.
            Assert.Contains("GAP", Ids(restored));
        }

        [Fact]
        public void CompositeModules_IndividualRestoreOfBModuleActuallyRestoresThatModule()
        {
            var state = State();
            Recompute(state);
            var session = state.SideA.ModuleSession;
            Assert.True(session.SetLength("B:M8", 55.0));
            Assert.True(session.SetLength("M2", 30.0));
            state.SideA.CommitModuleEdits();
            var customized = Recompute(state);
            var standard = LengthOf(Recompute(State()), "B:M8");
            Assert.Equal(55.0, LengthOf(customized, "B:M8"), 6);

            Assert.True(state.SideA.ModuleSession.RestoreModule("B:M8"));
            state.SideA.CommitModuleEdits();
            var restored = Recompute(state);

            Assert.Equal(standard, LengthOf(restored, "B:M8"), 6);   // vuelve a su calculada
            Assert.Equal(30.0, LengthOf(restored, "M2"), 6);         // el lado A no se toca
            Assert.Equal(48.0, LengthOf(restored, "B:M2"), 6);       // ni el resto de la mitad B
        }

        [Fact]
        public void CompositeModules_RestoreUsesModuleIdentityNotPosition()
        {
            // M2 y B:M2 son identidades distintas: restaurar una no puede restaurar la otra.
            var state = State();
            Recompute(state);
            var session = state.SideA.ModuleSession;
            Assert.True(session.SetLength("M2", 30.0));
            Assert.True(session.SetLength("B:M2", 40.0));
            state.SideA.CommitModuleEdits();
            Recompute(state);

            Assert.True(state.SideA.ModuleSession.RestoreModule("B:M2"));
            state.SideA.CommitModuleEdits();
            var restored = Recompute(state);

            Assert.Equal(30.0, LengthOf(restored, "M2"), 6);
            Assert.NotEqual(40.0, LengthOf(restored, "B:M2"));
        }

        // ---------------------------------------------------------------- el flujo normal no se rompe

        [Fact]
        public void CompositeModules_NormalFlowStillPreservesEverything()
        {
            var state = State();
            Recompute(state);
            Customize(state);

            Recompute(state);
            AssertCompositeCustomizationsAreBack(Recompute(state));
            AssertCompositeCustomizationsAreBack(Recompute(state));
        }
    }
}
