using System;
using System.Collections.Generic;
using System.Windows;
using RackCad.UI.Shell;
using RackCad.UI.Systems.PushBack;
using Xunit;
using static RackCad.UI.Tests.PushBackDeclaredCorrectionSupport;
using static RackCad.UI.Tests.PushBackFullEditorCharacterizationSupport;
using static RackCad.UI.Tests.PushBackStateNetSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4B PB-T1a — la red de estado de la parte COMPUESTA de la ventana CLASICA de Push Back: lo que el panel compuesto (separador
    /// central, estructura efectiva por lado, topologia, sentido, alcance y numero de frentes) escribe en la salida persistida
    /// (CT-PBT1A-05, D-5), lo que hacen «+ frente», «- frente» y «En blanco» con y sin lado B (CT-PBT1A-09, M-04 de PB-T0) y el defecto
    /// M-07 de PB-TC2 (CT-PBT1A-10), que se reproduce tal cual. Se fija lo que hay y no se corrige nada.
    ///
    /// <para>Cada metodo arma una lista de lineas y termina en UN <c>Assert.Equal</c>. Toda ventana se crea con
    /// <see cref="PushBackFullEditorCharacterizationSupport.Ventana"/>, su carga va dentro del <c>try</c> y se cierra en el
    /// <c>finally</c>; ninguna se muestra, no se usa el teclado ni el foco real, y cada metodo corre dentro de
    /// <see cref="EditorDiscardPrompt.Substitute(System.Func{string,bool})"/> con un contador de descartes que la ultima linea de cada
    /// ventana lleva, leido despues de cerrarla.</para>
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackStateNetCompositeTests
    {
        /// <summary>Cierra la ventana y agrega su ultima linea con el contador de descartes leido despues.</summary>
        private static void Fin(RackPushBackSystemWindow w, List<string> log, string linea, Func<int> descartes)
        {
            Cerrar(w);
            log.Add(linea + " descartes=" + descartes());
        }

        /// <summary>El panel compuesto de la salida: separador central, estructura por lado, topologia, sentido y topologias por celda.</summary>
        private static string Compuesto(string salida)
            => salida.StartsWith("NO-VÁLIDA", StringComparison.Ordinal)
                ? salida
                : "CentralSeparator=" + Leer(salida, "Composite.CentralSeparator")
                  + " StructureOverrideA=" + Leer(salida, "Composite.StructureOverrideA")
                  + " StructureOverrideB=" + Leer(salida, "Composite.StructureOverrideB")
                  + " DefaultTopology=" + Leer(salida, "Composite.DefaultTopology")
                  + " DefaultDirection=" + Leer(salida, "Composite.DefaultDirection")
                  + " Topologies=" + Leer(salida, "Composite.Topologies");

        /// <summary>Los frentes de cada lado y las ranuras ausentes de la salida.</summary>
        private static string Frentes(string salida)
            => salida.StartsWith("NO-VÁLIDA", StringComparison.Ordinal)
                ? salida
                : "Structure.Fronts=" + Cuenta(salida, "Structure.Fronts") + " SideB.Fronts=" + Cuenta(salida, "SideB.Fronts")
                  + " AbsentSlotsA=" + Leer(salida, "Composite.AbsentSlotsA") + " AbsentSlotsB=" + Leer(salida, "Composite.AbsentSlotsB");

        [Fact]
        public void CT_PBT1A_05_TheCentralSeparatorTheStructureOverrideTheTopologyTheDirectionTheScopeAndTheFrontCountReachTheOutput()
        {
            var log = new List<string>();
            var descartes = 0;
            using (EditorDiscardPrompt.Substitute(_ => { descartes++; return true; }))
            {
                Sta(() =>
                {
                    // El separador central.
                    var separador = Ventana();
                    try
                    {
                        Cargar(separador, Fixture("pb-20-ab"));
                        log.Add("carga " + Compuesto(Salida(separador)));
                        Check(separador, "CentralSeparatorCheck").IsChecked = true;
                        Fin(separador, log, "CentralSeparatorCheck=True " + Compuesto(Salida(separador)), () => descartes);
                    }
                    finally { Cerrar(separador); }

                    // La estructura efectiva por lado: aplicar en A, aplicar en B y restaurar la de B.
                    var estructura = Ventana();
                    try
                    {
                        Cargar(estructura, Fixture("pb-20-ab"));
                        SetNumberR(estructura, "StructureOverrideBox", 6);
                        Click(estructura, "ApplyStructureButton");
                        log.Add("A StructureOverrideBox=6 Aplicar " + Compuesto(Salida(estructura)));
                        Combo(estructura, "SideSelectorBox").SelectedIndex = 1;
                        SetNumberR(estructura, "StructureOverrideBox", 7);
                        Click(estructura, "ApplyStructureButton");
                        log.Add("B StructureOverrideBox=7 Aplicar " + Compuesto(Salida(estructura)));
                        Click(estructura, "RestoreStructureButton");
                        Fin(estructura, log, "B Restaurar estructura " + Compuesto(Salida(estructura)), () => descartes);
                    }
                    finally { Cerrar(estructura); }

                    // La topologia, el sentido y el alcance.
                    var topologia = Ventana();
                    try
                    {
                        Cargar(topologia, Fixture("pb-20-ab"));
                        foreach (var paso in new[] { (0, 0, 0), (3, 1, 4), (1, 0, 3) })
                        {
                            Combo(topologia, "CellTopologyBox").SelectedIndex = paso.Item1;
                            Combo(topologia, "RunDirectionBox").SelectedIndex = paso.Item2;
                            Combo(topologia, "TopologyScopeBox").SelectedIndex = paso.Item3;
                            Click(topologia, "ApplyTopologyButton");
                            log.Add("topologia=" + paso.Item1 + " sentido=" + paso.Item2 + " alcance=" + paso.Item3 + " " + Compuesto(Salida(topologia)));
                        }

                        Fin(topologia, log, "topologia fin " + Compuesto(Salida(topologia)), () => descartes);
                    }
                    finally { Cerrar(topologia); }

                    // El numero de frentes.
                    var frentes = Ventana();
                    try
                    {
                        Cargar(frentes, Fixture("pb-20-ab"));
                        log.Add("carga " + Frentes(Salida(frentes)));
                        SetNumberR(frentes, "FrontCountBox", 3);
                        log.Add("FrontCountBox=3 " + Frentes(Salida(frentes)));
                        SetNumberR(frentes, "FrontCountBox", 1);
                        Fin(frentes, log, "FrontCountBox=1 " + Frentes(Salida(frentes)), () => descartes);
                    }
                    finally { Cerrar(frentes); }
                });
            }

            Assert.Equal(
            new[]
            {
                "carga CentralSeparator=<null> StructureOverrideA=<null> StructureOverrideB=<null> DefaultTopology=\"Encontradas\" DefaultDirection=\"AToB\" Topologies=<null>",
                "CentralSeparatorCheck=True CentralSeparator=true StructureOverrideA=<null> StructureOverrideB=<null> DefaultTopology=\"Encontradas\" DefaultDirection=\"AToB\" Topologies=<null> descartes=0",
                "A StructureOverrideBox=6 Aplicar CentralSeparator=<null> StructureOverrideA=6 StructureOverrideB=<null> DefaultTopology=\"Encontradas\" DefaultDirection=\"AToB\" Topologies=<null>",
                "B StructureOverrideBox=7 Aplicar CentralSeparator=<null> StructureOverrideA=6 StructureOverrideB=7 DefaultTopology=\"Encontradas\" DefaultDirection=\"AToB\" Topologies=<null>",
                "B Restaurar estructura CentralSeparator=<null> StructureOverrideA=6 StructureOverrideB=<null> DefaultTopology=\"Encontradas\" DefaultDirection=\"AToB\" Topologies=<null> descartes=0",
                "topologia=0 sentido=0 alcance=0 CentralSeparator=<null> StructureOverrideA=<null> StructureOverrideB=<null> DefaultTopology=\"Encontradas\" DefaultDirection=\"AToB\" Topologies=[{\"Frente\":0,\"Level\":0,\"Topology\":\"SoloA\",\"Direction\":\"AToB\",\"CorridaDepth\":null}]",
                "topologia=3 sentido=1 alcance=4 CentralSeparator=<null> StructureOverrideA=<null> StructureOverrideB=<null> DefaultTopology=\"Encontradas\" DefaultDirection=\"AToB\" Topologies=[{\"Frente\":0,\"Level\":0,\"Topology\":\"Corrida\",\"Direction\":\"BToA\",\"CorridaDepth\":null},{\"Frente\":0,\"Level\":1,\"Topology\":\"Corrida\",\"Direction\":\"BToA\",\"CorridaDepth\":null},{\"Frente\":0,\"Level\":2,\"Topology\":\"Corrida\",\"Direction\":\"BToA\",\"CorridaDepth\":null},{\"Frente\":1,\"Level\":0,\"Topology\":\"Corrida\",\"Direction\":\"BToA\",\"CorridaDepth\":null},{\"Frente\":1,\"Level\":1,\"Topology\":\"Corrida\",\"Direction\":\"BToA\",\"CorridaDepth\":null},{\"Frente\":1,\"Level\":2,\"Topology\":\"Corrida\",\"Direction\":\"BToA\",\"CorridaDepth\":null}]",
                "topologia=1 sentido=0 alcance=3 CentralSeparator=<null> StructureOverrideA=<null> StructureOverrideB=<null> DefaultTopology=\"Encontradas\" DefaultDirection=\"AToB\" Topologies=[{\"Frente\":0,\"Level\":0,\"Topology\":\"SoloB\",\"Direction\":\"AToB\",\"CorridaDepth\":null},{\"Frente\":0,\"Level\":1,\"Topology\":\"SoloB\",\"Direction\":\"AToB\",\"CorridaDepth\":null},{\"Frente\":0,\"Level\":2,\"Topology\":\"SoloB\",\"Direction\":\"AToB\",\"CorridaDepth\":null},{\"Frente\":1,\"Level\":0,\"Topology\":\"Corrida\",\"Direction\":\"BToA\",\"CorridaDepth\":null},{\"Frente\":1,\"Level\":1,\"Topology\":\"Corrida\",\"Direction\":\"BToA\",\"CorridaDepth\":null},{\"Frente\":1,\"Level\":2,\"Topology\":\"Corrida\",\"Direction\":\"BToA\",\"CorridaDepth\":null}]",
                "topologia fin CentralSeparator=<null> StructureOverrideA=<null> StructureOverrideB=<null> DefaultTopology=\"Encontradas\" DefaultDirection=\"AToB\" Topologies=[{\"Frente\":0,\"Level\":0,\"Topology\":\"SoloB\",\"Direction\":\"AToB\",\"CorridaDepth\":null},{\"Frente\":0,\"Level\":1,\"Topology\":\"SoloB\",\"Direction\":\"AToB\",\"CorridaDepth\":null},{\"Frente\":0,\"Level\":2,\"Topology\":\"SoloB\",\"Direction\":\"AToB\",\"CorridaDepth\":null},{\"Frente\":1,\"Level\":0,\"Topology\":\"Corrida\",\"Direction\":\"BToA\",\"CorridaDepth\":null},{\"Frente\":1,\"Level\":1,\"Topology\":\"Corrida\",\"Direction\":\"BToA\",\"CorridaDepth\":null},{\"Frente\":1,\"Level\":2,\"Topology\":\"Corrida\",\"Direction\":\"BToA\",\"CorridaDepth\":null}] descartes=0",
                "carga Structure.Fronts=2 SideB.Fronts=2 AbsentSlotsA=<null> AbsentSlotsB=<null>",
                "FrontCountBox=3 Structure.Fronts=3 SideB.Fronts=3 AbsentSlotsA=<null> AbsentSlotsB=[2]",
                "FrontCountBox=1 Structure.Fronts=1 SideB.Fronts=1 AbsentSlotsA=<null> AbsentSlotsB=<null> descartes=0",
            },
            log);
        }

        [Fact]
        public void CT_PBT1A_09_AddingRemovingAndBlankingAFrontMoveTheFrontCountsAndTheAbsentSlotsWithAndWithoutSideB()
        {
            var log = new List<string>();
            var descartes = 0;
            using (EditorDiscardPrompt.Substitute(_ => { descartes++; return true; }))
            {
                Sta(() =>
                {
                    // Sin lado B.
                    var sinB = Ventana();
                    try
                    {
                        Cargar(sinB, Fixture("pb-01-base"));
                        log.Add("sin B carga " + Frentes(Salida(sinB)));
                        Click(sinB, "AddFrontButton");
                        log.Add("sin B + frente " + Frentes(Salida(sinB)));
                        Click(sinB, "RemoveFrontButton");
                        log.Add("sin B - frente " + Frentes(Salida(sinB)));
                        EditorWindowTestSupport.SetFrontBlank(sinB, Grid, 1, true);
                        var enBlanco = Salida(sinB);
                        log.Add("sin B En blanco frente 2 " + Frentes(enBlanco) + " IsActive=[" + Leer(enBlanco, "Structure.Fronts[0].IsActive") + "," + Leer(enBlanco, "Structure.Fronts[1].IsActive") + "," + Leer(enBlanco, "Structure.Fronts[2].IsActive") + "]");
                        EditorWindowTestSupport.SetFrontBlank(sinB, Grid, 1, false);
                        Fin(sinB, log, "sin B activo otra vez " + Frentes(Salida(sinB)), () => descartes);
                    }
                    finally { Cerrar(sinB); }

                    // Con lado B.
                    var conB = Ventana();
                    try
                    {
                        Cargar(conB, Fixture("pb-20-ab"));
                        log.Add("con B carga " + Frentes(Salida(conB)));
                        Click(conB, "AddFrontButton");
                        log.Add("con B + frente " + Frentes(Salida(conB)));
                        Click(conB, "RemoveFrontButton");
                        log.Add("con B - frente " + Frentes(Salida(conB)));
                        EditorWindowTestSupport.SetFrontBlank(conB, Grid, 0, true);
                        log.Add("con B En blanco frente 1 (lado A) " + Frentes(Salida(conB)));
                        EditorWindowTestSupport.SetFrontBlank(conB, Grid, 0, false);
                        Combo(conB, "SideSelectorBox").SelectedIndex = 1;
                        EditorWindowTestSupport.SetFrontBlank(conB, Grid, 1, true);
                        log.Add("con B En blanco frente 2 (lado B) " + Frentes(Salida(conB)));
                        EditorWindowTestSupport.SetFrontBlank(conB, Grid, 1, false);
                        Combo(conB, "SideSelectorBox").SelectedIndex = 2;
                        EditorWindowTestSupport.SetFrontBlank(conB, Grid, 1, true);
                        Fin(conB, log, "con B En blanco frente 2 (Ambos) " + Frentes(Salida(conB)), () => descartes);
                    }
                    finally { Cerrar(conB); }
                });
            }

            Assert.Equal(
            new[]
            {
                "sin B carga Structure.Fronts=3 SideB.Fronts=<null> AbsentSlotsA=<null> AbsentSlotsB=[0]",
                "sin B + frente Structure.Fronts=4 SideB.Fronts=<null> AbsentSlotsA=<null> AbsentSlotsB=[0]",
                "sin B - frente Structure.Fronts=3 SideB.Fronts=<null> AbsentSlotsA=<null> AbsentSlotsB=[0]",
                "sin B En blanco frente 2 Structure.Fronts=3 SideB.Fronts=<null> AbsentSlotsA=[1] AbsentSlotsB=[0] IsActive=[<null>,false,<null>]",
                "sin B activo otra vez Structure.Fronts=3 SideB.Fronts=<null> AbsentSlotsA=<null> AbsentSlotsB=[0] descartes=0",
                "con B carga Structure.Fronts=2 SideB.Fronts=2 AbsentSlotsA=<null> AbsentSlotsB=<null>",
                "con B + frente Structure.Fronts=3 SideB.Fronts=3 AbsentSlotsA=<null> AbsentSlotsB=[2]",
                "con B - frente Structure.Fronts=2 SideB.Fronts=2 AbsentSlotsA=<null> AbsentSlotsB=<null>",
                "con B En blanco frente 1 (lado A) Structure.Fronts=2 SideB.Fronts=2 AbsentSlotsA=[0] AbsentSlotsB=<null>",
                "con B En blanco frente 2 (lado B) Structure.Fronts=2 SideB.Fronts=2 AbsentSlotsA=<null> AbsentSlotsB=[1]",
                "con B En blanco frente 2 (Ambos) Structure.Fronts=2 SideB.Fronts=2 AbsentSlotsA=[1] AbsentSlotsB=[1] descartes=0",
            },
            log);
        }

        [Fact]
        public void CT_PBT1A_10_IncumplimientoCaracterizadoM07AStaleMixedIndicatorMakesAnEmptiedManualLengthKeepItsOverride()
        {
            var log = new List<string>();
            var descartes = 0;
            using (EditorDiscardPrompt.Substitute(_ => { descartes++; return true; }))
            {
                Sta(() =>
                {
                    const string LargoA = "Structure.Fronts[0].Levels[0].BeamLengthOverride";
                    const string LargoB = "SideB.Fronts[0].Levels[0].BeamLengthOverride";

                    // D-mixto de PB-TC2: A y B difieren en el largo manual (100.00044 y 120) y en el alto (61.23456 y 60).
                    var mixto = EditInner(
                        Fixture("pb-20-ab"),
                        inner =>
                        {
                            Poner(inner, "Structure.Fronts[0].Levels[0].PalletHeight", 61.23456);
                            Poner(inner, LargoA, 100.00044);
                            Poner(inner, LargoB, 120);
                        });

                    var w = Ventana();
                    try
                    {
                        Cargar(w, mixto);
                        var carga = Salida(w);
                        log.Add("carga A.largo=" + Leer(carga, LargoA) + " B.largo=" + Leer(carga, LargoB));

                        // El camino de H2-11: «Ambos» pinta el estado mixto, y retirar el lado B lo deja obsoleto.
                        Combo(w, "SideSelectorBox").SelectedIndex = 2;
                        log.Add("Ambos CellBeamLengthOverrideBox=[" + Field(w, "CellBeamLengthOverrideBox").Text + "] CellPalletHeightBox=[" + Field(w, "CellPalletHeightBox").Text + "]");
                        Check(w, "SideBPresentCheck").IsChecked = false;
                        w.SelectMatrixCell(0, 0, false);
                        log.Add("sin lado B CellBeamLengthOverrideBox=[" + Field(w, "CellBeamLengthOverrideBox").Text + "] CellPalletHeightBox=[" + Field(w, "CellPalletHeightBox").Text + "]");

                        // Vaciar «Largo manual»: INCUMPLIMIENTO CARACTERIZADO, la salida conserva el override.
                        var largo = Field(w, "CellBeamLengthOverrideBox");
                        EditorWindowTestSupport.SetText(w, "CellBeamLengthOverrideBox", string.Empty);
                        largo.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, largo));
                        var vaciado = Salida(w);
                        log.Add("INCUMPLIMIENTO CARACTERIZADO M-07 CellBeamLengthOverrideBox vaciado texto=[" + largo.Text + "] A.largo=" + Leer(vaciado, LargoA));

                        // Una caja obligatoria vacia no marca error.
                        var alto = Field(w, "CellPalletHeightBox");
                        EditorWindowTestSupport.SetText(w, "CellPalletHeightBox", string.Empty);
                        alto.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, alto));
                        var final = Salida(w);
                        Fin(w, log, "INCUMPLIMIENTO CARACTERIZADO M-07 CellPalletHeightBox vaciado HasError=" + alto.HasError + " IsOptional=" + alto.IsOptional
                            + " A.alto=" + Leer(final, "Structure.Fronts[0].Levels[0].PalletHeight"), () => descartes);
                    }
                    finally { Cerrar(w); }
                });
            }

            Assert.Equal(
            new[]
            {
                "carga A.largo=100.00044 B.largo=120",
                "Ambos CellBeamLengthOverrideBox=[] CellPalletHeightBox=[]",
                "sin lado B CellBeamLengthOverrideBox=[100] CellPalletHeightBox=[61.235]",
                "INCUMPLIMIENTO CARACTERIZADO M-07 CellBeamLengthOverrideBox vaciado texto=[] A.largo=100.00044",
                "INCUMPLIMIENTO CARACTERIZADO M-07 CellPalletHeightBox vaciado HasError=False IsOptional=True A.alto=61.23456 descartes=0",
            },
            log);
        }
    }
}
