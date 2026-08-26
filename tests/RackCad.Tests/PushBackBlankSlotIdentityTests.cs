using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (corrección aislada 2) — «EN BLANCO» conserva la RANURA FÍSICA.
    ///
    /// <para>
    /// Decisión del dueño: ese frente físico EXISTE en la retícula, pero ese lado no almacena ahí. No significa
    /// borrar la ranura, compactar índices, mover los frentes posteriores ni retirar la estructura.
    /// </para>
    /// <para>
    /// Son TRES preguntas distintas y no un solo booleano: la RANURA física existe; el ALMACENAMIENTO de un lado
    /// puede no existir en ella; y una LÍNEA de cabecera existe si sostiene algo a izquierda o a derecha. Estas
    /// pruebas fijan las tres por separado, con aserciones POR RANURA contra el mismo rack sin el blanco.
    /// </para>
    /// </summary>
    public class PushBackBlankSlotIdentityTests
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

        private static PushBackCompositeEditorState State(int slots, int levels = 2)
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.LoadNew();
            state.SetSlotCount(slots);
            state.SetSideBPresent(true);
            for (var slot = 0; slot < slots; slot++)
            {
                state.SetSlotPresent(PushBackSide.B, slot, true);
            }

            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                var matrix = state.Of(side).Structure;
                for (var index = 0; index < matrix.Count; index++)
                {
                    state.Of(side).AdjustLevels(index, levels - matrix.Fronts[index].LoadLevels);
                }
            }

            return state;
        }

        /// <summary>El mismo rack con las ranuras pedidas EN BLANCO en los lados pedidos.</summary>
        private static PushBackSystem Rack(int slots, params (int Slot, PushBackSide Side)[] blanks)
        {
            var state = State(slots);
            foreach (var blank in blanks)
            {
                Assert.True(
                    state.SetSlotPresent(blank.Side, blank.Slot, false),
                    $"el editor rechazó poner en blanco la ranura {blank.Slot} del lado {blank.Side}");
            }

            var computation = new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog);
            Assert.NotNull(computation.System);
            return computation.System;
        }

        // ---- lecturas ------------------------------------------------------------------------------------------

        /// <summary>La posición transversal de cada LÍNEA de la retícula, exista o no.</summary>
        private static IReadOnlyList<double> LinePositions(PushBackSystem system)
            => DynamicFrontGeometry.Compute(system.Structure, Catalog).PostPositions
                .Select(position => Math.Round(position, 3))
                .ToList();

        /// <summary>Las líneas que EXISTEN, por su índice: la regla de continuidad estructural.</summary>
        private static IReadOnlyList<int> PresentLines(PushBackSystem system)
            => DynamicFrontActivation.PresentBoundaries(system.Structure);

        /// <summary>La identidad física de cada ranura: su índice, su claro y si tiene almacenamiento.</summary>
        private static IReadOnlyList<(int Index, double Start, double End, bool Active)> Slots(PushBackSystem system)
            => system.Structure.Fronts
                .Select(front => (front.Index, Math.Round(front.StartX, 3), Math.Round(front.EndX, 3), front.IsActive))
                .ToList();

        private static IReadOnlyList<(int Slot, int Level, PushBackSide Low, PushBackSide High)> Runs(PushBackSystem system)
            => PushBackRuns.Resolve(system).Runs
                .Select(run => (Slot: run.Slot, Level: run.Level, Low: run.LowSide, High: run.HighSide))
                .OrderBy(run => run.Slot).ThenBy(run => run.Level).ThenBy(run => run.Low)
                .ToList();

        private static IReadOnlyList<(int Slot, int Level, PushBackSide Low, PushBackSide High)> RunsOfSlot(
            PushBackSystem system, int slot)
            => Runs(system).Where(run => run.Slot == slot).ToList();

        private static IReadOnlyList<string> ModuleIds(PushBackSystem system)
            => system.Structure.Modules.Select(module => module.ModuleId).ToList();

        /// <summary>La ranura que un lado ve como suya: la que el puente ranura→índice local resuelve.</summary>
        private static DynamicRackFront LocalFront(PushBackSystem system, PushBackSide side, int slot)
            => system.Composite.Of(side).LocalFront(slot);

        // ---- 1: la primera en blanco no se lleva la última ------------------------------------------------------

        /// <summary>
        /// EL DEFECTO QUE VIO EL DUEÑO. Con tres ranuras y la primera en blanco en los dos lados, la tercera perdía
        /// TODO su almacenamiento: la sub-estructura de cada lado se compactaba a N-1 frentes mientras la retícula
        /// compuesta conservaba N, y como el puente ranura→índice local es la identidad, la ranura 1 leía la
        /// configuración de la 2 y la 2 se quedaba sin ninguna.
        /// </summary>
        [Fact]
        public void BlankFirstSlot_DoesNotRemoveLastSlot()
        {
            var baseline = Rack(3);
            var blanked = Rack(3, (0, PushBackSide.A), (0, PushBackSide.B));

            Assert.Equal(3, blanked.Structure.Fronts.Count);
            Assert.Equal(LinePositions(baseline), LinePositions(blanked));

            // La ranura en blanco no almacena, y las OTRAS DOS conservan exactamente lo suyo.
            Assert.Empty(RunsOfSlot(blanked, 0));
            Assert.Equal(RunsOfSlot(baseline, 1), RunsOfSlot(blanked, 1));
            Assert.Equal(RunsOfSlot(baseline, 2), RunsOfSlot(blanked, 2));
            Assert.NotEmpty(RunsOfSlot(blanked, 2));

            // Y cada lado sigue leyendo SU frente en cada ranura, no el de la siguiente: la ranura en blanco tiene
            // COLUMNA local pero no almacenamiento, y las dos siguientes tienen las dos cosas.
            foreach (var side in new[] { PushBackSide.A, PushBackSide.B })
            {
                Assert.NotNull(LocalFront(blanked, side, 0));            // la columna existe
                Assert.Null(blanked.Composite.Of(side).Front(0));        // el almacenamiento no
                Assert.NotNull(LocalFront(blanked, side, 1));
                Assert.NotNull(blanked.Composite.Of(side).Front(1));
                Assert.NotNull(LocalFront(blanked, side, 2));
                Assert.NotNull(blanked.Composite.Of(side).Front(2));
            }
        }

        // ---- 2: la del medio no corre a las siguientes ----------------------------------------------------------

        [Fact]
        public void BlankMiddleSlot_DoesNotShiftFollowingSlots()
        {
            var baseline = Rack(3);
            var blanked = Rack(3, (1, PushBackSide.A), (1, PushBackSide.B));

            Assert.Equal(LinePositions(baseline), LinePositions(blanked));
            Assert.Equal(Slots(baseline).Count, Slots(blanked).Count);

            // La ranura POSTERIOR al blanco conserva su índice, su claro y su almacenamiento.
            Assert.Equal(Slots(baseline)[2].Index, Slots(blanked)[2].Index);
            Assert.Equal(Slots(baseline)[2].Start, Slots(blanked)[2].Start);
            Assert.Equal(Slots(baseline)[2].End, Slots(blanked)[2].End);
            Assert.Equal(RunsOfSlot(baseline, 2), RunsOfSlot(blanked, 2));
            Assert.Equal(RunsOfSlot(baseline, 0), RunsOfSlot(blanked, 0));
            Assert.Empty(RunsOfSlot(blanked, 1));
        }

        // ---- 3: dos blancos seguidos, y la línea inútil de en medio ---------------------------------------------

        /// <summary>
        /// Entre DOS ranuras en blanco consecutivas no hay línea: no sostiene nada. Las que delimitan los grupos
        /// activos sí, y ninguna se mueve.
        /// </summary>
        [Fact]
        public void TwoConsecutiveBlankSlots_RemoveOnlyUselessIntermediateHeader()
        {
            var baseline = Rack(4);
            var blanked = Rack(4, (1, PushBackSide.A), (1, PushBackSide.B), (2, PushBackSide.A), (2, PushBackSide.B));

            Assert.Equal(new[] { 0, 1, 2, 3, 4 }, PresentLines(baseline));
            Assert.Equal(new[] { 0, 1, 3, 4 }, PresentLines(blanked));   // la 2 queda entre los dos blancos
            Assert.Equal(LinePositions(baseline), LinePositions(blanked));

            Assert.Equal(RunsOfSlot(baseline, 0), RunsOfSlot(blanked, 0));
            Assert.Equal(RunsOfSlot(baseline, 3), RunsOfSlot(blanked, 3));
            Assert.Empty(RunsOfSlot(blanked, 1));
            Assert.Empty(RunsOfSlot(blanked, 2));
        }

        // ---- 4 y 5: los dos lados son independientes ------------------------------------------------------------

        [Fact]
        public void BlankOnA_DoesNotRemoveBStorage()
        {
            var baseline = Rack(3);
            var blanked = Rack(3, (0, PushBackSide.A));

            Assert.Equal(LinePositions(baseline), LinePositions(blanked));
            Assert.Equal(PresentLines(baseline), PresentLines(blanked));   // la línea sigue haciendo falta por B

            var slotZero = RunsOfSlot(blanked, 0);
            Assert.NotEmpty(slotZero);
            Assert.All(slotZero, run => Assert.Equal(PushBackSide.B, run.Low));
            Assert.Equal(
                RunsOfSlot(baseline, 0).Where(run => run.Low == PushBackSide.B).ToList(),
                slotZero);

            Assert.Equal(RunsOfSlot(baseline, 1), RunsOfSlot(blanked, 1));
            Assert.Equal(RunsOfSlot(baseline, 2), RunsOfSlot(blanked, 2));
        }

        [Fact]
        public void BlankOnB_DoesNotRemoveAStorage()
        {
            var baseline = Rack(3);
            var blanked = Rack(3, (0, PushBackSide.B));

            Assert.Equal(LinePositions(baseline), LinePositions(blanked));
            Assert.Equal(PresentLines(baseline), PresentLines(blanked));

            var slotZero = RunsOfSlot(blanked, 0);
            Assert.NotEmpty(slotZero);
            Assert.All(slotZero, run => Assert.Equal(PushBackSide.A, run.Low));
            Assert.Equal(RunsOfSlot(baseline, 1), RunsOfSlot(blanked, 1));
            Assert.Equal(RunsOfSlot(baseline, 2), RunsOfSlot(blanked, 2));
        }

        // ---- 6: el hueco físico ---------------------------------------------------------------------------------

        /// <summary>
        /// En blanco en los DOS lados la ranura sigue siendo un hueco FÍSICO: conserva su índice, su claro y su
        /// posición, y puede separar dos grupos del rack. Lo único que pierde es el almacenamiento.
        /// </summary>
        [Fact]
        public void BlankOnBoth_PreservesPhysicalGap()
        {
            var baseline = Rack(3);
            var blanked = Rack(3, (1, PushBackSide.A), (1, PushBackSide.B));

            Assert.Equal(Slots(baseline).Select(slot => slot.Index), Slots(blanked).Select(slot => slot.Index));
            Assert.Equal(Slots(baseline).Select(slot => slot.Start), Slots(blanked).Select(slot => slot.Start));
            Assert.Equal(Slots(baseline).Select(slot => slot.End), Slots(blanked).Select(slot => slot.End));
            Assert.Equal(LinePositions(baseline), LinePositions(blanked));

            Assert.True(Slots(baseline)[1].Active);
            Assert.False(Slots(blanked)[1].Active);   // el hueco existe y se declara vacío
        }

        // ---- 7: un patrón no contiguo ---------------------------------------------------------------------------

        [Fact]
        public void NonContiguousBlankPattern_PreservesSlotIdentity()
        {
            var baseline = Rack(5);
            var blanked = Rack(5, (1, PushBackSide.A), (1, PushBackSide.B), (3, PushBackSide.A), (3, PushBackSide.B));

            Assert.Equal(5, blanked.Structure.Fronts.Count);
            Assert.Equal(LinePositions(baseline), LinePositions(blanked));
            // Ningún blanco es vecino de otro, así que TODAS las líneas siguen haciendo falta.
            Assert.Equal(PresentLines(baseline), PresentLines(blanked));

            foreach (var slot in new[] { 0, 2, 4 })
            {
                Assert.Equal(RunsOfSlot(baseline, slot), RunsOfSlot(blanked, slot));
                Assert.NotEmpty(RunsOfSlot(blanked, slot));
            }

            foreach (var slot in new[] { 1, 3 })
            {
                Assert.Empty(RunsOfSlot(blanked, slot));
            }
        }

        // ---- 8: celdas y camas siguen alineadas ------------------------------------------------------------------

        /// <summary>
        /// Ninguna cama nace en una ranura que ese lado dejó en blanco, y ninguna que sí tiene almacenamiento se
        /// pierde: la numeración de celdas y la de camas siguen diciendo lo mismo.
        /// </summary>
        [Fact]
        public void BlankSlot_CellsAndRunsRemainConsistent()
        {
            var blanked = Rack(4, (1, PushBackSide.A), (2, PushBackSide.A), (2, PushBackSide.B));

            foreach (var run in PushBackRuns.Resolve(blanked).Runs)
            {
                Assert.NotNull(blanked.Composite.Of(run.LowSide).Front(run.Slot));
                Assert.NotNull(blanked.Composite.Of(run.HighSide).Front(run.Slot));
                Assert.Equal(run.Slot, run.SourceFrontIndex);   // identidad, no compactación
            }

            // Ranura 1: sólo B almacena. Ranura 2: ninguno. Ranuras 0 y 3: los dos.
            Assert.All(RunsOfSlot(blanked, 1), run => Assert.Equal(PushBackSide.B, run.Low));
            Assert.Empty(RunsOfSlot(blanked, 2));
            Assert.Equal(4, RunsOfSlot(blanked, 0).Count);
            Assert.Equal(4, RunsOfSlot(blanked, 3).Count);

            // Y toda celda resuelta de una ranura en blanco está vacía de camas.
            foreach (var cell in blanked.Composite.Cells.Where(candidate => candidate.FrontIndex == 2))
            {
                Assert.Empty(cell.Beds.Where(bed => bed != null));
            }
        }

        // ---- 9: la línea sigue a la necesidad física -------------------------------------------------------------

        /// <summary>
        /// Una línea existe si sostiene almacenamiento a izquierda o a derecha, y la necesidad es la UNIÓN de los
        /// dos lados: ni «en blanco ⇒ sin línea» ni «ranura ⇒ siempre línea».
        /// </summary>
        [Fact]
        public void BlankSlot_HeaderLinesFollowPhysicalNeed()
        {
            // Activo | blanco | activo: todas las líneas hacen falta.
            Assert.Equal(new[] { 0, 1, 2, 3 }, PresentLines(Rack(3, (1, PushBackSide.A), (1, PushBackSide.B))));

            // Activo | blanco | blanco | activo: la de en medio no sostiene nada.
            Assert.Equal(
                new[] { 0, 1, 3, 4 },
                PresentLines(Rack(4, (1, PushBackSide.A), (1, PushBackSide.B), (2, PushBackSide.A), (2, PushBackSide.B))));

            // Blanco SÓLO en A: la línea sigue haciendo falta porque B almacena ahí.
            Assert.Equal(
                new[] { 0, 1, 2, 3, 4 },
                PresentLines(Rack(4, (1, PushBackSide.A), (2, PushBackSide.A))));

            // Y los bordes exteriores nunca desaparecen, aunque la ranura del borde esté en blanco.
            var edge = PresentLines(Rack(3, (0, PushBackSide.A), (0, PushBackSide.B)));
            Assert.Contains(0, edge);
            Assert.Contains(3, edge);
        }

        // ---- 10: I-40 no se reabre --------------------------------------------------------------------------------

        /// <summary>Poner una ranura en blanco no toca la secuencia de MÓDULOS, que es la identidad de I-40.</summary>
        [Fact]
        public void BlankSlot_ModuleIdsOfSurvivingLinesRemainStable()
        {
            var baseline = Rack(4);
            foreach (var blanked in new[]
            {
                Rack(4, (0, PushBackSide.A), (0, PushBackSide.B)),
                Rack(4, (1, PushBackSide.A), (1, PushBackSide.B), (2, PushBackSide.A), (2, PushBackSide.B)),
                Rack(4, (2, PushBackSide.B))
            })
            {
                Assert.Equal(ModuleIds(baseline), ModuleIds(blanked));
                Assert.Equal(baseline.Structure.TotalLength, blanked.Structure.TotalLength, 6);
            }
        }

        // ---- persistencia ----------------------------------------------------------------------------------------

        /// <summary>
        /// El patrón de blancos sobrevive a guardar y volver a abrir: mismas ranuras, mismos lados, mismas
        /// posiciones y las mismas camas.
        /// </summary>
        [Fact]
        public void BlankPattern_SurvivesSaveAndLoad()
        {
            var state = State(5);
            foreach (var slot in new[] { 1, 3 })
            {
                state.SetSlotPresent(PushBackSide.A, slot, false);
            }

            state.SetSlotPresent(PushBackSide.B, 3, false);

            var assembler = new PushBackCompositeEditorAssembler(Catalog);
            var design = assembler.BuildDesign(state, Inputs());
            var before = new PushBackResolver(Catalog).Resolve(design);

            var json = System.Text.Json.JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(design));
            var reloaded = System.Text.Json.JsonSerializer
                .Deserialize<PushBackDesignDocument>(json)
                .ToDomain();
            var after = new PushBackResolver(Catalog).Resolve(reloaded);

            Assert.Equal(before.Structure.Fronts.Count, after.Structure.Fronts.Count);
            Assert.Equal(LinePositions(before), LinePositions(after));
            Assert.Equal(PresentLines(before), PresentLines(after));
            Assert.Equal(Runs(before), Runs(after));

            // Y el patrón es el declarado, no otro: 1 sólo en A, 3 en los dos.
            Assert.All(RunsOfSlot(after, 1), run => Assert.Equal(PushBackSide.B, run.Low));
            Assert.Empty(RunsOfSlot(after, 3));
            foreach (var slot in new[] { 0, 2, 4 })
            {
                Assert.Equal(4, RunsOfSlot(after, slot).Count);
            }
        }
    }
}
