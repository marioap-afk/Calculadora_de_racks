using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (A3-MOD, contrato del dueño) — LA SECUENCIA MODULAR DE UN RACK COMPUESTO ES UNA, Y ES DEL RACK.
    ///
    /// <para>
    /// <c>M1…Mn</c>, el hueco y <c>B:Mn…B:M1</c> son la MISMA secuencia longitudinal. Su dueño canonico vive en el
    /// lado A —esa es la raiz del diseno— y el selector de lado es solo contexto de interfaz: no decide que modulos
    /// existen ni que personalizaciones sobreviven.
    /// </para>
    ///
    /// <para>
    /// <b>Medido antes del cambio.</b> El ensamblador preguntaba «¿hay que reconstruir la secuencia?» comparando la
    /// del baseline —la del RACK: 17 modulos, base 17— contra el layout de profundidad del lado A —8 posiciones,
    /// base 8—. Nunca coincidian, asi que TODO recalculo de un compuesto reconstruia una secuencia solo-A y
    /// reconciliaba contra ella: un recalculo sin ninguna edicion reportaba «GAP eliminado» y <c>LostAnything</c>;
    /// una edicion aceptada de <c>B:M8</c> (55") volvia a 54" en el recalculo siguiente, con <c>B:M8</c> en la lista
    /// de eliminados; y un override por linea sobre <c>B:M6</c> desaparecia entero, porque el filtro de
    /// <c>HeaderLineOverrides</c> solo conocia los ids del lado A.
    /// </para>
    ///
    /// <para>
    /// La correccion no deshabilita nada ni crea una segunda autoridad: la pregunta de reconstruccion se hace contra
    /// la FORMA del lado A —su sistema local, que el compuesto ya publica—, y cuando si hay que reconstruir, la cola
    /// compuesta del baseline se reanexa antes de reconciliar para que la comparacion sea contra la secuencia del
    /// rack y no contra media. El resolver reparte despues esa secuencia por lado, exactamente como al reabrir un
    /// rack guardado.
    /// </para>
    /// </summary>
    public class PushBackCompositeModuleSequenceTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackEditorInputs Inputs()
        {
            var inputs = PushBackEditorInputs.NewDesign();
            inputs.Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");
            return inputs;
        }

        /// <summary>Un compuesto A + hueco + B con las ranuras de B presentes, como lo deja el editor.</summary>
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

        /// <summary>
        /// El recalculo REAL del editor compuesto: se arma el diseno del rack, se resuelve por el mismo
        /// <c>BuildFrom</c> que usa la ventana y se adopta el resultado como baseline.
        /// </summary>
        private static PushBackSystem Recompute(PushBackCompositeEditorState state)
        {
            var assembler = new PushBackEditorDesignAssembler(Catalog);
            var design = new PushBackCompositeEditorAssembler(Catalog).BuildDesign(state, Inputs());
            var computation = assembler.BuildFrom(design, PushBackSide.A);
            Assert.True(computation.IsValid, computation.Error);
            assembler.AcceptComputation(state.SideA, computation);
            return computation.System;
        }

        private static PushBackDesign Design(PushBackCompositeEditorState state)
            => new PushBackCompositeEditorAssembler(Catalog).BuildDesign(state, Inputs());

        private static DynamicRackModule Module(PushBackSystem system, string moduleId)
            => system.Structure.Modules.FirstOrDefault(module =>
                string.Equals(module.ModuleId, moduleId, StringComparison.Ordinal));

        private static IReadOnlyList<string> Ids(PushBackSystem system)
            => system.Structure.Modules.Select(module => module.ModuleId).ToList();

        private static void Edit(PushBackCompositeEditorState state, Action<RackCad.Application.Systems.Shared.RackModuleEditSession> edit)
        {
            edit(state.SideA.ModuleSession);
            state.SideA.CommitModuleEdits();
        }

        // ---------------------------------------------------------------- el modulo de B sobrevive

        [Fact]
        public void CompositeModules_BSideEditSurvivesRecomputeAndReconcile()
        {
            var state = State();
            Recompute(state);

            // El panel modular del rack muestra la secuencia entera: es la del rack, no la de un lado.
            Assert.Contains("B:M8", state.SideA.ModuleSession.Modules.Select(module => module.ModuleId));
            Edit(state, session => Assert.True(session.SetLength("B:M8", 55.0)));

            var afterCommit = Recompute(state);
            Assert.Equal(55.0, Module(afterCommit, "B:M8").Length, 6);
            Assert.Contains("B:M8", state.SideA.LastModuleReconciliation.Preserved);
            Assert.False(state.SideA.LastModuleReconciliation.LostAnything);

            // Y el recalculo SIGUIENTE —el que no lleva ninguna edicion— tampoco se la lleva por delante.
            var later = Recompute(state);
            Assert.Equal(55.0, Module(later, "B:M8").Length, 6);
            Assert.False(state.SideA.LastModuleReconciliation.LostAnything);
        }

        [Fact]
        public void CompositeModules_BSideHeaderLineOverrideSurvivesRecompute()
        {
            var state = State();
            Recompute(state);

            var session = state.SideA.ModuleSession;
            var configuration = session.HeaderConfigurationCopy("B:M6", 1);
            Assert.NotNull(configuration);
            configuration.Height = 137.0;
            Assert.True(session.ApplyHeaderConfigurationToLine(configuration, 1, new[] { "B:M6" }).Applied);
            state.SideA.CommitModuleEdits();

            foreach (var _ in new[] { 0, 1 })
            {
                var system = Recompute(state);

                // La clave de I-40 es (linea, ModuleId) y sigue valiendo para la mitad B.
                var line = system.Structure.HeaderLineOverrides.Single(override_ =>
                    override_.PostIndex == 1 && string.Equals(override_.ModuleId, "B:M6", StringComparison.Ordinal));
                Assert.Equal(137.0, line.Header.Height, 6);
            }
        }

        [Fact]
        public void CompositeModules_GapIsNotReportedLostOnUnchangedRecompute()
        {
            var state = State();

            // Dos recalculos sin ninguna edicion: el hueco sigue siendo una pieza del rack, no algo que se pierde.
            Recompute(state);
            Recompute(state);
            Assert.False(state.SideA.LastModuleReconciliation.LostAnything);
            Assert.DoesNotContain("GAP", state.SideA.LastModuleReconciliation.Removed);

            var system = Recompute(state);
            Assert.False(state.SideA.LastModuleReconciliation.LostAnything);
            Assert.Contains("GAP", Ids(system));
        }

        [Fact]
        public void CompositeModules_AM2AndBM2KeepIndependentCustomizations()
        {
            var state = State();
            Recompute(state);
            Edit(state, session =>
            {
                Assert.True(session.SetLength("M2", 30.0));
                Assert.True(session.SetLength("B:M2", 40.0));
            });

            var system = Recompute(state);

            // M2 y B:M2 son identidades DISTINTAS: normalizar el prefijo las colapsaria.
            Assert.Equal(30.0, Module(system, "M2").Length, 6);
            Assert.Equal(40.0, Module(system, "B:M2").Length, 6);
        }

        [Fact]
        public void CompositeModules_ActiveSideDoesNotChangeModuleOwnership()
        {
            var state = State();
            Recompute(state);
            state.SetActiveSide(PushBackSide.B);
            Edit(state, session => Assert.True(session.SetLength("B:M8", 55.0)));

            Assert.Equal(55.0, Module(Recompute(state), "B:M8").Length, 6);

            state.SetActiveSide(PushBackSide.A);
            Assert.Equal(55.0, Module(Recompute(state), "B:M8").Length, 6);

            state.SetActiveSide(PushBackSide.B);
            var system = Recompute(state);
            Assert.Equal(55.0, Module(system, "B:M8").Length, 6);

            // Y editar el lado A no toca lo de B.
            state.SetActiveSide(PushBackSide.A);
            Edit(state, session => Assert.True(session.SetLength("M8", 51.0)));
            var both = Recompute(state);
            Assert.Equal(51.0, Module(both, "M8").Length, 6);
            Assert.Equal(55.0, Module(both, "B:M8").Length, 6);
        }

        // ---------------------------------------------------------------- lo que desaparece, desaparece

        [Fact]
        public void CompositeModules_ReconcileRemovesOnlyPhysicalModulesThatActuallyDisappear()
        {
            var state = State();
            Recompute(state);
            Edit(state, session =>
            {
                Assert.True(session.SetLength("M8", 51.0));
                Assert.True(session.SetLength("B:M6", 33.0));
            });
            Recompute(state);

            // Un cambio de topologia REAL del lado A: el rack encoge y M8 deja de existir.
            foreach (var front in state.SideA.Structure.Fronts) front.PalletsDeep = 2;
            var system = Recompute(state);
            var report = state.SideA.LastModuleReconciliation;

            Assert.DoesNotContain("M8", Ids(system));
            Assert.Contains("M8", report.Removed);   // se pierde, y se dice
            Assert.True(report.LostAnything);

            // Y SOLO esa: el hueco y la mitad B no cambiaron, asi que no pueden aparecer como perdidos.
            Assert.Equal(new[] { "M8" }, report.Removed.OrderBy(id => id, StringComparer.Ordinal).ToArray());
            Assert.DoesNotContain("GAP", report.Removed);
            Assert.Contains("GAP", report.Preserved);

            // Lo que sigue existiendo fisicamente se conserva: el hueco y la mitad B, que no cambio.
            Assert.Contains("GAP", Ids(system));
            Assert.Equal(33.0, Module(system, "B:M6").Length, 6);
            Assert.Contains("B:M6", report.Preserved);
        }

        // ---------------------------------------------------------------- ida y vuelta por el documento

        [Fact]
        public void CompositeModules_BSideCustomizationRoundTripsThroughPersistence()
        {
            var state = State();
            Recompute(state);
            var session = state.SideA.ModuleSession;
            Assert.True(session.SetLength("M2", 30.0));
            Assert.True(session.SetLength("B:M2", 40.0));
            var configuration = session.HeaderConfigurationCopy("B:M6", 1);
            configuration.Height = 137.0;
            Assert.True(session.ApplyHeaderConfigurationToLine(configuration, 1, new[] { "B:M6" }).Applied);
            state.SideA.CommitModuleEdits();
            Recompute(state);

            // Guardar, reabrir, recalcular y volver a guardar: dos vueltas completas.
            var design = Design(state);
            for (var round = 0; round < 2; round++)
            {
                var reopened = PushBackDesignDocument.FromDomain(design).ToDomain();
                var stored = reopened.Structure.Modules;

                Assert.Equal(30.0, stored.Single(module => module.ModuleId == "M2").Length, 6);
                Assert.Equal(40.0, stored.Single(module => module.ModuleId == "B:M2").Length, 6);
                Assert.Contains(stored, module => module.ModuleId == "GAP");

                // Y lo que se dibuja tras reabrir es lo mismo que se guardo.
                var system = new PushBackResolver(Catalog).Resolve(reopened);
                Assert.Equal(30.0, Module(system, "M2").Length, 6);
                Assert.Equal(40.0, Module(system, "B:M2").Length, 6);
                Assert.Equal(
                    137.0,
                    system.Structure.HeaderLineOverrides
                        .Single(line => line.PostIndex == 1 && line.ModuleId == "B:M6").Header.Height,
                    6);

                design = reopened;
            }
        }

        // ---------------------------------------------------------------- una sola linea base

        [Fact]
        public void CompositeModules_OneRackBaselineOwnsTheCompositeSequence()
        {
            var state = State();
            Recompute(state);
            Edit(state, session => Assert.True(session.SetLength("B:M8", 55.0)));
            Recompute(state);

            // El baseline del dueño canonico lleva la secuencia del RACK.
            var owner = state.SideA.WorkingBaseline.Structure.Modules.Select(module => module.ModuleId).ToList();
            Assert.Contains("M1", owner);
            Assert.Contains("GAP", owner);
            Assert.Contains("B:M8", owner);

            // El lado B no gobierna: no tiene una secuencia compuesta paralela que pueda divergir.
            var sideB = state.SideB.WorkingBaseline?.Structure?.Modules
                .Select(module => module.ModuleId)
                .ToList();
            Assert.True(
                sideB == null || !sideB.Contains("GAP"),
                "el lado B no puede llevar una segunda secuencia compuesta");
            Assert.Equal(55.0, state.SideA.WorkingBaseline.Structure.Modules
                .Single(module => module.ModuleId == "B:M8").Length, 6);
        }

        [Fact]
        public void CompositeModules_RebuildDoesNotMutateTheBaselineTail()
        {
            // La cola compuesta se reanexa COPIADA: reconciliar sobre la secuencia reconstruida no puede escribir
            // en el baseline, que solo avanza cuando una computacion valida se adopta.
            var state = State();
            Recompute(state);
            Edit(state, session => Assert.True(session.SetLength("B:M6", 33.0)));
            Recompute(state);

            // El baseline vigente ANTES del recalculo que reconstruye.
            var previous = state.SideA.WorkingBaseline.Structure.Modules
                .Single(module => module.ModuleId == "B:M6");

            foreach (var front in state.SideA.Structure.Fronts) front.PalletsDeep = 3;
            var rebuilt = Recompute(state);

            Assert.NotSame(previous, Module(rebuilt, "B:M6"));
            Assert.Equal(33.0, previous.Length, 6);          // el objeto anterior no se toco
            Assert.Equal(33.0, Module(rebuilt, "B:M6").Length, 6);
        }
    }
}
