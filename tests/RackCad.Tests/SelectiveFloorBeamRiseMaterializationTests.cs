using System.Linq;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-43, gate 8.6D (ARQ-43-03): INV-12 — todo frente tiene elevación DIRECTA tras crearse, cargarse o
    /// redimensionarse.
    /// <para>
    /// <c>null</c> solo puede sobrevivir de forma transitoria mientras se lee un documento legacy, antes de
    /// materializarlo. El valor global histórico queda como compatibilidad de LECTURA: deja de ser autoridad de
    /// escritura, y por eso ninguna fila puede quedarse esperando a que alguien lo coalesque.
    /// </para>
    /// </summary>
    public class SelectiveFloorBeamRiseMaterializationTests
    {
        private static SelectiveEditorState StateWith(params int[] frentesPorFondo)
        {
            var state = new SelectiveEditorState();
            foreach (var frentes in frentesPorFondo)
            {
                state.InitMatrix(frentes, 2);
                state.FondoMatrices.Add(state.SnapshotWorking(48.0, 0.0));
            }

            state.SelectedFondo = 0;
            state.LoadFondo(0);
            return state;
        }

        private static double?[] LiveRow(SelectiveEditorState state) => state.FloorBeamRiseOverrides.ToArray();

        private static double?[] SlotRow(SelectiveEditorState state, int fondo)
            => state.FondoMatrices[fondo].FloorBeamRiseOverrides.ToArray();

        // ---- El slot del fondo SELECCIONADO también se materializa ----

        [Fact]
        public void Materialize_FillsTheSlotOfTheSelectedFondo_NotOnlyTheLiveRow()
        {
            // El caso real de la carga: SelectedFondo = 0, la fila viva es todavía OTRA matriz y el slot 0 llega con
            // nulos. Saltarse k == SelectedFondo dejaba ese slot sin materializar, y el siguiente RestoreWorkingFrom
            // volvía a meter los nulos en la fila viva.
            var state = StateWith(2, 2);
            state.SelectedFondo = 0;
            for (var i = 0; i < state.FondoMatrices[0].FloorBeamRiseOverrides.Count; i++)
            {
                state.FondoMatrices[0].FloorBeamRiseOverrides[i] = null; // el slot llega como documento legacy
            }

            for (var i = 0; i < state.FloorBeamRiseOverrides.Count; i++) state.FloorBeamRiseOverrides[i] = null;

            state.MaterializeFloorBeamRises(7.0);

            Assert.All(SlotRow(state, 0), rise => Assert.Equal(7.0, rise)); // el slot del fondo SELECCIONADO
            Assert.All(LiveRow(state), rise => Assert.Equal(7.0, rise));
            // El fondo 1 ya tenia valor directo (sembrado al crearse): materializar no lo pisa.
            Assert.All(SlotRow(state, 1), rise => Assert.Equal(SelectiveRackDefaults.DefaultFloorBeamRise, rise));
        }

        [Fact]
        public void Materialize_LeavesNoNullInAnyFondo()
        {
            var state = StateWith(3, 1, 2);
            foreach (var matrix in state.FondoMatrices)
            {
                for (var i = 0; i < matrix.FloorBeamRiseOverrides.Count; i++) matrix.FloorBeamRiseOverrides[i] = null;
            }

            for (var i = 0; i < state.FloorBeamRiseOverrides.Count; i++) state.FloorBeamRiseOverrides[i] = null;

            state.MaterializeFloorBeamRises(7.0);

            Assert.All(Enumerable.Range(0, state.FondoMatrices.Count).SelectMany(k => SlotRow(state, k)),
                rise => Assert.Equal(7.0, rise));
            Assert.DoesNotContain(LiveRow(state), rise => !rise.HasValue);
        }

        // ---- Idempotencia y preservación de un valor directo ----

        [Fact]
        public void Materialize_IsIdempotent()
        {
            var state = StateWith(2, 2);
            state.MaterializeFloorBeamRises(7.0);
            var afterFirst = Enumerable.Range(0, state.FondoMatrices.Count).Select(k => SlotRow(state, k)).ToArray();
            var liveAfterFirst = LiveRow(state);

            state.MaterializeFloorBeamRises(7.0);

            for (var k = 0; k < state.FondoMatrices.Count; k++) Assert.Equal(afterFirst[k], SlotRow(state, k));
            Assert.Equal(liveAfterFirst, LiveRow(state));
        }

        [Fact]
        public void Materialize_NeverOverwritesAValueThatIsAlreadyDirect()
        {
            var state = StateWith(2, 2);
            state.FondoMatrices[1].FloorBeamRiseOverrides[0] = 0.0;  // un cero explícito ES un valor
            state.FondoMatrices[1].FloorBeamRiseOverrides[1] = 15.0;

            state.MaterializeFloorBeamRises(7.0);

            Assert.Equal(new double?[] { 0.0, 15.0 }, SlotRow(state, 1));
        }

        [Fact]
        public void Materialize_WithANegativeLegacy_UsesTheDefault()
        {
            var state = StateWith(2);
            for (var i = 0; i < state.FloorBeamRiseOverrides.Count; i++) state.FloorBeamRiseOverrides[i] = null;

            state.MaterializeFloorBeamRises(-1.0); // no es una elevación: cae al default

            Assert.All(LiveRow(state), rise => Assert.Equal(SelectiveRackDefaults.DefaultFloorBeamRise, rise));
        }

        // ---- Siembra: un frente NUEVO nace con valor directo ----

        [Fact]
        public void AFreshMatrix_SeedsEveryFrenteWithTheDefault()
        {
            var state = new SelectiveEditorState();
            state.InitMatrix(3, 2);

            Assert.All(LiveRow(state), rise => Assert.Equal(SelectiveRackDefaults.DefaultFloorBeamRise, rise));
        }

        [Fact]
        public void GrowingTheMatrix_StillClonesTheLastFrentesValue()
        {
            // La siembra es solo para un frente SIN origen. Un frente que clona al anterior conserva su semántica:
            // copia lo que el último tenga, no el default.
            var state = new SelectiveEditorState();
            state.InitMatrix(2, 2);
            state.FloorBeamRiseOverrides[1] = 21.0;

            state.ResizeBays(3);

            Assert.Equal(21.0, state.FloorBeamRiseOverrides[2]);
        }

        [Fact]
        public void AClonedFondo_KeepsTheSourceValues_AndSeedsOnlyTheFrentesItAdds()
        {
            var state = new SelectiveEditorState();
            state.InitMatrix(2, 2);
            state.FloorBeamRiseOverrides[0] = 9.0;
            state.FloorBeamRiseOverrides[1] = 11.0;
            var source = state.SnapshotWorking(48.0, 0.0);

            var clone = state.CloneAligned(source, 3, source);

            Assert.Equal(9.0, clone.FloorBeamRiseOverrides[0]);
            Assert.Equal(11.0, clone.FloorBeamRiseOverrides[1]);
            Assert.Equal(SelectiveRackDefaults.DefaultFloorBeamRise, clone.FloorBeamRiseOverrides[2]);
        }

        [Fact]
        public void BuildDesign_EmitsADirectValueOnEveryFrente_WithoutAnyEditing()
        {
            var state = StateWith(2, 2);
            var design = state.BuildDesign(new SelectiveDesignInputs
            {
                PostId = TestCatalogIds.Profiles.Posts.Standard,
                PostPeralte = 3.0,
                PalletTolerance = 4.0,
                VerticalClearance = 6.0,
                FloorBeamRise = SelectiveRackDefaults.DefaultFloorBeamRise,
                Fondo = 48.0,
                DepthCount = 2,
                WorkingDepth = 48.0,
                WorkingCabeceraOverride = 0.0,
                Separators = new System.Collections.Generic.List<double>()
            });

            Assert.All(design.Bays, bay => Assert.NotNull(bay.FloorBeamRiseOverride));
            Assert.All(design.ExtraFondoBays.SelectMany(f => f), bay => Assert.NotNull(bay.FloorBeamRiseOverride));
        }
    }
}
