using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.UI.Systems.PushBack;
using Xunit;
using static RackCad.UI.Tests.PushBackFullEditorCharacterizationSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4B PB-T0 — CARACTERIZACION de la parte COMPUESTA de la ventana clasica de Push Back (CT-PB0-06 a CT-PB0-09): la
    /// cola aparcada, la presencia del lado B, y anadir, quitar o dejar en blanco frentes con y sin lado B. Se fijan el
    /// estado compuesto de la ventana y lo que su <c>LastComputation</c> escribiria, sin ejecutar nada que escriba.
    ///
    /// <para>Cada gesto es el del usuario sobre el control real (casilla «Lado B», selector de lado, casillas «En blanco»
    /// y botones de frente); ninguna ventana se muestra y toda ventana se cierra en un <c>finally</c>. Fija lo que HAY.</para>
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackFullEditorCharacterizationCompositeTests
    {
        private const string Grid = "PushBackMatrixGrid";

        private static void Blank(RackPushBackSystemWindow w, int front, bool blank) => EditorWindowTestSupport.SetFrontBlank(w, Grid, front, blank);

        private static void Side(RackPushBackSystemWindow w, int index) => Combo(w, "SideSelectorBox").SelectedIndex = index;

        private static void DeclareB(RackPushBackSystemWindow w, bool present) => Check(w, "SideBPresentCheck").IsChecked = present;

        private static string Ids(IEnumerable<string> ids) => "[" + string.Join(",", ids) + "]";

        private static string Numbers(IEnumerable<int> numbers) => Ids(numbers.Select(n => n.ToString(CultureInfo.InvariantCulture)));

        private static string Dormant(RackPushBackSystemWindow w) => Ids(w.CompositeState.DormantCompositeTail.Select(m => m.ModuleId));

        /// <summary>Lo que la ventana escribiria: si es compuesto y los ids de la secuencia de modulos.</summary>
        private static string Out(RackPushBackSystemWindow w)
        {
            Assert.True(w.CurrentInputsAreValid && w.LastComputation != null && w.LastComputation.IsValid, StatusText(w));
            var design = w.LastComputation.Design;
            return "composite=" + design.IsComposite + " modules=" + Ids(design.Structure.Modules.Select(m => m.ModuleId));
        }

        /// <summary>CT-PB0-07: lo compuesto de la salida y de la casilla.</summary>
        private static string Composite(RackPushBackSystemWindow w)
        {
            Assert.True(w.CurrentInputsAreValid && w.LastComputation != null && w.LastComputation.IsValid, StatusText(w));
            var design = w.LastComputation.Design;
            var composite = design.CompositeOrDefault();
            return "composite=" + design.IsComposite + " sideB=" + (design.SideB != null)
                + " absentA=" + Numbers(composite.AbsentSlotsA) + " absentB=" + Numbers(composite.AbsentSlotsB)
                + " topology=" + composite.DefaultTopology + " check=" + Check(w, "SideBPresentCheck").IsChecked + " dormant=" + Dormant(w);
        }

        /// <summary>CT-PB0-08 y CT-PB0-09: los frentes de cada lado (activos como 1/0), las ranuras ausentes y los cortes laterales.</summary>
        private static string Fronts(RackPushBackSystemWindow w)
        {
            Assert.True(w.CurrentInputsAreValid && w.LastComputation != null && w.LastComputation.IsValid, StatusText(w));
            var state = w.CompositeState;
            return "A=" + state.SideA.Structure.Count + ":" + string.Concat(state.SideA.Structure.Fronts.Select(f => f.IsActive ? "1" : "0"))
                + " B=" + state.SideB.Structure.Count + ":" + string.Concat(state.SideB.Structure.Fronts.Select(f => f.IsActive ? "1" : "0"))
                + " absentA=" + Numbers(state.AbsentSlotsOfA()) + " absentB=" + Numbers(state.AbsentSlotsOfB())
                + " cortes=" + Numbers(w.LastComputation.LateralCortes.Select(c => c.PostIndex));
        }

        [Fact]
        public void CT_PB0_06_TheDormantTailIsParkedAtLoadConsumedWhenSideBHasAFrontAndParkedAgainWhenSideBIsRetired()
        {
            var log = new List<string>();
            Sta(() =>
            {
                var w = Abrir(Fixture("pb-21-cola"));
                try
                {
                    log.Add("carga dormant=" + Dormant(w) + " sideB=" + w.CompositeState.SideBPresent + " " + Out(w));
                    DeclareB(w, true);
                    log.Add("declarar B dormant=" + Dormant(w) + " sideB=" + w.CompositeState.SideBPresent + " " + Out(w));
                    Side(w, 1);
                    Blank(w, 0, false);
                    log.Add("ranura 0 de B activa dormant=" + Dormant(w) + " sideB=" + w.CompositeState.SideBPresent + " " + Out(w));
                    DeclareB(w, false);
                    log.Add("retirar B dormant=" + Dormant(w) + " sideB=" + w.CompositeState.SideBPresent + " " + Out(w));
                }
                finally { Cerrar(w); }
            });

            Assert.Equal(
                new[]
                {
                    "carga dormant=[GAP,B:M8,B:M7,B:M6,B:M5,B:M4,B:M3,B:M2,B:M1] sideB=False composite=False modules=[M1,M2,M3,M4,M5,M6,M7,M8]",
                    "declarar B dormant=[GAP,B:M8,B:M7,B:M6,B:M5,B:M4,B:M3,B:M2,B:M1] sideB=True composite=False modules=[M1,M2,M3,M4,M5,M6,M7,M8]",
                    "ranura 0 de B activa dormant=[] sideB=True composite=True modules=[M1,M2,M3,M4,M5,M6,M7,M8,GAP,B:M8,B:M7,B:M6,B:M5,B:M4,B:M3,B:M2,B:M1]",
                    "retirar B dormant=[GAP,B:M8,B:M7,B:M6,B:M5,B:M4,B:M3,B:M2,B:M1] sideB=False composite=False modules=[M1,M2,M3,M4,M5,M6,M7,M8]",
                },
                log);
        }

        [Fact]
        public void CT_PB0_07_RetiringAndDeclaringSideBMovesTheCompositeFlagsTheAbsentSlotsAndTheDefaultTopology()
        {
            var log = new List<string>();
            Sta(() =>
            {
                var w = Abrir(Fixture("pb-20-ab"));
                try
                {
                    log.Add("carga " + Composite(w));
                    DeclareB(w, false);
                    log.Add("retirar B " + Composite(w));
                    DeclareB(w, true);
                    log.Add("declarar B " + Composite(w));
                    Side(w, 1);
                    for (var slot = 0; slot < w.CompositeState.SideB.Structure.Count; slot++) Blank(w, slot, true);
                    log.Add("todas las ranuras de B en blanco " + Composite(w));
                }
                finally { Cerrar(w); }
            });

            Assert.Equal(
                new[]
                {
                    "carga composite=True sideB=True absentA=[] absentB=[] topology=Encontradas check=True dormant=[]",
                    "retirar B composite=False sideB=False absentA=[] absentB=[0,1] topology=SoloA check=False dormant=[GAP,B:M8,B:M7,B:M6,B:M5,B:M4,B:M3,B:M2,B:M1]",
                    "declarar B composite=True sideB=True absentA=[] absentB=[] topology=Encontradas check=True dormant=[]",
                    "todas las ranuras de B en blanco composite=False sideB=False absentA=[] absentB=[0,1] topology=Encontradas check=True dormant=[GAP,B:M8,B:M7,B:M6,B:M5,B:M4,B:M3,B:M2,B:M1]",
                },
                log);
        }

        [Fact]
        public void CT_PB0_08_AddingRemovingAndBlankingFrontsWithoutSideBMovesTheFrontsTheAbsentSlotsAndTheLateralCuts()
        {
            var log = new List<string>();
            Sta(() =>
            {
                var w = Abrir(Fixture("pb-01-base"));
                try
                {
                    log.Add("carga " + Fronts(w));
                    Click(w, "AddFrontButton");
                    log.Add("anadir frente " + Fronts(w));
                    Click(w, "RemoveFrontButton");
                    log.Add("quitar frente " + Fronts(w));
                    Blank(w, 1, true);
                    Blank(w, 2, true);
                    log.Add("frentes 1 y 2 en blanco " + Fronts(w));
                    Blank(w, 2, false);
                    log.Add("frente 2 activo otra vez " + Fronts(w));
                }
                finally { Cerrar(w); }
            });

            Assert.Equal(
                new[]
                {
                    "carga A=3:111 B=1:0 absentA=[] absentB=[0] cortes=[0,1,2,3]",
                    "anadir frente A=4:1111 B=1:0 absentA=[] absentB=[0] cortes=[0,1,2,3,4]",
                    "quitar frente A=3:111 B=1:0 absentA=[] absentB=[0] cortes=[0,1,2,3]",
                    "frentes 1 y 2 en blanco A=3:100 B=1:0 absentA=[1,2] absentB=[0] cortes=[0,1,3]",
                    "frente 2 activo otra vez A=3:101 B=1:0 absentA=[1] absentB=[0] cortes=[0,1,2,3]",
                },
                log);
        }

        [Fact]
        public void CT_PB0_09_AddingRemovingAndBlankingFrontsWithSideBMovesTheFrontsTheAbsentSlotsTheCutsAndTheCompositeFlag()
        {
            var log = new List<string>();
            Sta(() =>
            {
                var w = Abrir(Fixture("pb-01-base"));
                try
                {
                    DeclareB(w, true);
                    log.Add("declarar B " + Fronts(w) + " composite=" + w.LastComputation.Design.IsComposite);
                    Click(w, "AddFrontButton");
                    log.Add("anadir frente con A activo " + Fronts(w) + " composite=" + w.LastComputation.Design.IsComposite);
                    Side(w, 1);
                    Blank(w, 1, false);
                    log.Add("lado B, ranura 1 de B activa " + Fronts(w) + " composite=" + w.LastComputation.Design.IsComposite);
                    Side(w, 0);
                    Blank(w, 1, true);
                    Blank(w, 2, true);
                    log.Add("ranuras 1 y 2 de A en blanco " + Fronts(w) + " composite=" + w.LastComputation.Design.IsComposite);
                    Side(w, 1);
                    Blank(w, 1, true);
                    log.Add("ranura 1 de B en blanco " + Fronts(w) + " composite=" + w.LastComputation.Design.IsComposite);
                    DeclareB(w, false);
                    log.Add("retirar B " + Fronts(w) + " composite=" + w.LastComputation.Design.IsComposite);
                }
                finally { Cerrar(w); }
            });

            Assert.Equal(
                new[]
                {
                    "declarar B A=3:111 B=3:000 absentA=[] absentB=[0,1,2] cortes=[0,1,2,3] composite=False",
                    "anadir frente con A activo A=4:1111 B=4:0000 absentA=[] absentB=[0,1,2,3] cortes=[0,1,2,3,4] composite=False",
                    "lado B, ranura 1 de B activa A=4:1111 B=4:0100 absentA=[] absentB=[0,2,3] cortes=[0,1,2,3,4] composite=True",
                    "ranuras 1 y 2 de A en blanco A=4:1001 B=4:0100 absentA=[1,2] absentB=[0,2,3] cortes=[0,1,2,3,4] composite=True",
                    "ranura 1 de B en blanco A=4:1001 B=4:0000 absentA=[1,2] absentB=[0,1,2,3] cortes=[0,1,3,4] composite=False",
                    "retirar B A=4:1001 B=4:0000 absentA=[1,2] absentB=[0,1,2,3] cortes=[0,1,3,4] composite=False",
                },
                log);
        }
    }
}
