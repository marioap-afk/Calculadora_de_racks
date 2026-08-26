using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (corrección aislada 2B) — la DECLARACIÓN FÍSICA de una ranura del lado B sobrevive a «En blanco».
    ///
    /// <para>
    /// El lado A ya podía: su ausencia se declara aparte y su frente sigue en el diseño. El lado B la expresaba con
    /// una entrada NULA en su propia lista, así que al poner la ranura en blanco perdía también su ancho, su BFR y
    /// su override de larguero — y con ellos se movían todas las líneas posteriores.
    /// </para>
    /// <para>
    /// Ahora los dos lados declaran igual: el frente viaja completo y la ausencia se dice aparte. «En blanco» apaga
    /// el ALMACENAMIENTO; la declaración física queda DORMANTE y reaparece al quitarlo.
    /// </para>
    /// </summary>
    public class PushBackBlankSideBDeclarationTests
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

        /// <summary>Un rack donde SOLO el lado <paramref name="wide"/> declara calles de más en la ranura pedida.</summary>
        private static PushBackCompositeEditorState WideOn(PushBackSide wide, int slot, int slots = 3, int extra = 1)
        {
            var state = State(slots);
            state.Of(wide).Structure.AdjustPositions(slot, extra);
            return state;
        }

        private static PushBackSystem Resolve(PushBackCompositeEditorState state)
        {
            var computation = new PushBackCompositeEditorAssembler(Catalog).Build(state, Inputs(), Catalog);
            Assert.NotNull(computation.System);
            return computation.System;
        }

        private static PushBackDesign Design(PushBackCompositeEditorState state)
            => new PushBackCompositeEditorAssembler(Catalog).BuildDesign(state, Inputs());

        private static PushBackDesign RoundTrip(PushBackDesign design)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(design));
            return System.Text.Json.JsonSerializer.Deserialize<PushBackDesignDocument>(json).ToDomain();
        }

        // ---- lecturas ------------------------------------------------------------------------------------------

        private static IReadOnlyList<double> LinePositions(PushBackSystem system)
            => DynamicFrontGeometry.Compute(system.Structure, Catalog).PostPositions
                .Select(position => Math.Round(position, 3))
                .ToList();

        private static IReadOnlyList<(double Start, double End)> SlotSpans(PushBackSystem system)
            => system.Structure.Fronts
                .Select(front => (Math.Round(front.StartX, 3), Math.Round(front.EndX, 3)))
                .ToList();

        private static IReadOnlyList<(int Slot, int Level, PushBackSide Low)> Runs(PushBackSystem system)
            => PushBackRuns.Resolve(system).Runs
                .Select(run => (Slot: run.Slot, Level: run.Level, Low: run.LowSide))
                .OrderBy(run => run.Slot).ThenBy(run => run.Level).ThenBy(run => run.Low)
                .ToList();

        private static IReadOnlyList<(int Slot, int Level, PushBackSide Low)> RunsOfSlot(PushBackSystem system, int slot)
            => Runs(system).Where(run => run.Slot == slot).ToList();

        /// <summary>
        /// Los rangos de PROFUNDIDAD de las ranuras indicadas. La ranura que se pone en blanco en un lado sí encoge
        /// su rango —ese medio rack ya no almacena—, así que sólo se comparan las OTRAS: son las que no se pueden
        /// mover.
        /// </summary>
        private static IReadOnlyList<(double Start, double End)> SpansOf(PushBackSystem system, params int[] slots)
            => slots.Select(slot => SlotSpans(system)[slot]).ToList();

        /// <summary>El ancho declarado de la bahía, tal como lo ve la retícula: la distancia a la línea siguiente.</summary>
        private static double BayWidth(PushBackSystem system, int slot)
        {
            var lines = LinePositions(system);
            return Math.Round(lines[slot + 1] - lines[slot], 3);
        }

        // ---- A: el ancho propio de B ----------------------------------------------------------------------------

        /// <summary>
        /// El caso que quedó abierto en la corrección 2: la ranura sólo obtiene su ancho de B, y ponerla EN BLANCO
        /// en B lo perdía. Medido entonces: las líneas pasaban de 0/97.49/150.99/204.48 a 0/53.49/106.99/160.48.
        /// </summary>
        [Fact]
        public void BlankB_PreservesItsOwnStoredWidth()
        {
            var baseline = Resolve(WideOn(PushBackSide.B, 0));

            var blanked = WideOn(PushBackSide.B, 0);
            Assert.True(blanked.SetSlotPresent(PushBackSide.B, 0, false));
            var system = Resolve(blanked);

            Assert.Equal(BayWidth(baseline, 0), BayWidth(system, 0));
            Assert.Equal(LinePositions(baseline), LinePositions(system));
            Assert.Equal(SpansOf(baseline, 1, 2), SpansOf(system, 1, 2));

            // Y lo que sí desaparece es el ALMACENAMIENTO de B en esa ranura, sólo ahí.
            Assert.All(RunsOfSlot(system, 0), run => Assert.Equal(PushBackSide.A, run.Low));
            Assert.Equal(RunsOfSlot(baseline, 1), RunsOfSlot(system, 1));
            Assert.Equal(RunsOfSlot(baseline, 2), RunsOfSlot(system, 2));
        }

        // ---- B: el override de larguero --------------------------------------------------------------------------

        /// <summary>
        /// Un override de longitud de larguero declarado sólo en B es intención FÍSICA: queda dormante con la ranura
        /// en blanco, no se borra.
        /// </summary>
        [Fact]
        public void BlankB_PreservesBeamLengthOverride()
        {
            const double Override = 111.0;

            var state = State(3);
            Assert.True(state.SetSlotPresent(PushBackSide.B, 1, false));
            var design = Design(state);

            Assert.NotNull(design.SideB);
            Assert.NotNull(design.SideB.Fronts[1]);                    // el frente en blanco VIAJA, no es nulo
            Assert.Contains(1, design.Composite.AbsentSlotsB);         // y su ausencia se declara aparte
            design.SideB.Fronts[1].BeamLengthOverride = Override;

            var system = new PushBackResolver(Catalog).Resolve(design);
            Assert.Equal(Override, system.Structure.Fronts[1].BeamLengthOverride);
            Assert.Empty(RunsOfSlot(system, 1).Where(run => run.Low == PushBackSide.B));
        }

        // ---- C: nada se mueve detrás ------------------------------------------------------------------------------

        [Fact]
        public void BlankB_DoesNotShiftFollowingSlots()
        {
            var baseline = Resolve(WideOn(PushBackSide.B, 1, slots: 4));

            var blanked = WideOn(PushBackSide.B, 1, slots: 4);
            Assert.True(blanked.SetSlotPresent(PushBackSide.B, 1, false));
            var system = Resolve(blanked);

            Assert.Equal(LinePositions(baseline), LinePositions(system));
            Assert.Equal(SpansOf(baseline, 0, 2, 3), SpansOf(system, 0, 2, 3));
            Assert.Equal(
                DynamicFrontActivation.PresentBoundaries(baseline.Structure),
                DynamicFrontActivation.PresentBoundaries(system.Structure));

            foreach (var slot in new[] { 0, 2, 3 })
            {
                Assert.Equal(RunsOfSlot(baseline, slot), RunsOfSlot(system, slot));
            }
        }

        // ---- D: reactivar devuelve lo que había ------------------------------------------------------------------

        /// <summary>
        /// Quitar «En blanco» devuelve la configuración que el lado tenía, no un valor por defecto: es la prueba de
        /// que el blanco APAGA el almacenamiento en vez de destruir el frente.
        /// </summary>
        [Fact]
        public void BlankB_ReactivationRestoresStoredIntent()
        {
            var state = WideOn(PushBackSide.B, 0, extra: 2);
            var baseline = Resolve(state);
            var width = BayWidth(baseline, 0);
            var storage = RunsOfSlot(baseline, 0);

            Assert.True(state.SetSlotPresent(PushBackSide.B, 0, false));
            var blanked = Resolve(state);
            Assert.Equal(width, BayWidth(blanked, 0));                              // el ancho no se fue
            Assert.All(RunsOfSlot(blanked, 0), run => Assert.Equal(PushBackSide.A, run.Low));

            Assert.True(state.SetSlotPresent(PushBackSide.B, 0, true));
            var restored = Resolve(state);
            Assert.Equal(width, BayWidth(restored, 0));
            Assert.Equal(storage, RunsOfSlot(restored, 0));                          // y el almacenamiento vuelve
            Assert.Equal(LinePositions(baseline), LinePositions(restored));
        }

        // ---- E: guardar y volver a abrir ---------------------------------------------------------------------------

        /// <summary>
        /// El blanco de B sobrevive al archivo, con su ancho, y NO resucita: el documento nuevo lleva el frente
        /// completo y la ranura declarada.
        /// </summary>
        [Fact]
        public void BlankB_RoundTripsWithoutResurrection()
        {
            var state = WideOn(PushBackSide.B, 1, slots: 4);
            Assert.True(state.SetSlotPresent(PushBackSide.B, 1, false));

            var design = Design(state);
            var before = new PushBackResolver(Catalog).Resolve(design);
            var after = new PushBackResolver(Catalog).Resolve(RoundTrip(design));

            Assert.Equal(LinePositions(before), LinePositions(after));
            Assert.Equal(SlotSpans(before), SlotSpans(after));   // el archivo no cambia NADA, ni el rango del blanco
            Assert.Equal(Runs(before), Runs(after));
            Assert.All(RunsOfSlot(after, 1), run => Assert.Equal(PushBackSide.A, run.Low));
        }

        [Fact]
        public void NonContiguousBlankB_RoundTrips()
        {
            var state = State(5);
            foreach (var slot in new[] { 1, 3 })
            {
                state.Of(PushBackSide.B).Structure.AdjustPositions(slot, 1);
                Assert.True(state.SetSlotPresent(PushBackSide.B, slot, false));
            }

            var design = Design(state);
            Assert.Equal(new[] { 1, 3 }, design.Composite.AbsentSlotsB.OrderBy(slot => slot).ToArray());

            var before = new PushBackResolver(Catalog).Resolve(design);
            var after = new PushBackResolver(Catalog).Resolve(RoundTrip(design));

            Assert.Equal(LinePositions(before), LinePositions(after));
            Assert.Equal(Runs(before), Runs(after));
            foreach (var slot in new[] { 1, 3 })
            {
                Assert.All(RunsOfSlot(after, slot), run => Assert.Equal(PushBackSide.A, run.Low));
            }

            foreach (var slot in new[] { 0, 2, 4 })
            {
                Assert.Contains(RunsOfSlot(after, slot), run => run.Low == PushBackSide.B);
            }
        }

        // ---- F: los dos lados significan lo mismo ------------------------------------------------------------------

        /// <summary>
        /// Declarar el ancho sólo en A y poner A en blanco, o declararlo sólo en B y poner B en blanco, tienen que
        /// dar la MISMA retícula. Era justo la asimetría que quedaba: A conservaba su declaración y B no.
        /// </summary>
        [Fact]
        public void BlankAAndB_AreSemanticallySymmetric()
        {
            var fromA = WideOn(PushBackSide.A, 0);
            Assert.True(fromA.SetSlotPresent(PushBackSide.A, 0, false));

            var fromB = WideOn(PushBackSide.B, 0);
            Assert.True(fromB.SetSlotPresent(PushBackSide.B, 0, false));

            var a = Resolve(fromA);
            var b = Resolve(fromB);

            Assert.Equal(LinePositions(a), LinePositions(b));
            Assert.Equal(SpansOf(a, 1, 2), SpansOf(b, 1, 2));
            Assert.Equal(
                DynamicFrontActivation.PresentBoundaries(a.Structure),
                DynamicFrontActivation.PresentBoundaries(b.Structure));

            // La ranura en blanco encoge su PROFUNDIDAD hacia el lado que sí almacena, y lo hace en espejo: con A
            // en blanco se queda con la mitad de B, y con B en blanco con la mitad de A. Misma regla, dos lados.
            var half = Math.Round(a.Structure.TotalLength / 2.0, 3);
            Assert.Equal(half, SlotSpans(a)[0].Start);
            Assert.Equal(a.Structure.TotalLength, SlotSpans(a)[0].End, 3);
            Assert.Equal(0.0, SlotSpans(b)[0].Start);
            Assert.Equal(half, SlotSpans(b)[0].End);

            // Y el almacenamiento superviviente es el del OTRO lado en cada caso, en la misma cantidad.
            Assert.All(RunsOfSlot(a, 0), run => Assert.Equal(PushBackSide.B, run.Low));
            Assert.All(RunsOfSlot(b, 0), run => Assert.Equal(PushBackSide.A, run.Low));
            Assert.Equal(RunsOfSlot(a, 0).Count, RunsOfSlot(b, 0).Count);
        }

        // ---- G: los documentos anteriores -------------------------------------------------------------------------

        /// <summary>
        /// Un documento I-42 anterior escribe la ausencia de B como entrada NULA y no trae la declaración nueva.
        /// Se sigue leyendo igual: la ranura no almacena, y no se le inventa una configuración física que nunca
        /// guardó.
        /// </summary>
        [Fact]
        public void LegacyI42NullableB_RemainsCompatible()
        {
            var state = State(3);
            Assert.True(state.SetSlotPresent(PushBackSide.B, 1, false));
            var design = Design(state);

            // Se degrada el diseño a la forma ANTERIOR: entrada nula y sin la lista nueva.
            design.SideB.Fronts[1] = null;
            design.SideB.FrontConfigs[1] = null;
            design.Composite.AbsentSlotsB.Clear();

            var document = PushBackDesignDocument.FromDomain(design);
            Assert.Null(document.Composite.AbsentSlotsB);   // nada nuevo llega al archivo

            var legacy = RoundTrip(design);
            Assert.Null(legacy.SideB.Fronts[1]);
            Assert.Empty(legacy.Composite.AbsentSlotsB);

            var system = new PushBackResolver(Catalog).Resolve(legacy);
            Assert.Equal(3, system.Structure.Fronts.Count);
            Assert.All(RunsOfSlot(system, 1), run => Assert.Equal(PushBackSide.A, run.Low));
            Assert.Contains(RunsOfSlot(system, 0), run => run.Low == PushBackSide.B);
            Assert.Contains(RunsOfSlot(system, 2), run => run.Low == PushBackSide.B);
        }

        /// <summary>Y un documento anterior cuya ranura SÍ tenía almacenamiento sigue teniéndolo.</summary>
        [Fact]
        public void LegacyI42PresentB_RemainsPresent()
        {
            var design = Design(State(3));
            Assert.Empty(design.Composite.AbsentSlotsB);

            var system = new PushBackResolver(Catalog).Resolve(RoundTrip(design));
            foreach (var slot in new[] { 0, 1, 2 })
            {
                Assert.Contains(RunsOfSlot(system, slot), run => run.Low == PushBackSide.B);
                Assert.Contains(RunsOfSlot(system, slot), run => run.Low == PushBackSide.A);
            }
        }
    }
}
