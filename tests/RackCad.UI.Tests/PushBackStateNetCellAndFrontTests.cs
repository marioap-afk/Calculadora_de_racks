using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
    /// I-64 F4B PB-T1a — la red de estado de las cajas del FRENTE y de la CELDA de la ventana CLASICA de Push Back: lo que cada caja del
    /// panel escribe en la salida persistida (CT-PBT1A-03 y CT-PBT1A-04, D-5), lo que una caja que el usuario no edito conserva (CT-PBT1A-07,
    /// M-03 de PB-TC y M-04 de PB-T0) y la regla de «Ambos lados» en el alcance y en los datos del frente (CT-PBT1A-08, M-03 de PB-TC2).
    /// Se fija lo que hay y no se corrige nada.
    ///
    /// <para>Cada metodo arma una lista de lineas y termina en UN <c>Assert.Equal</c>. Toda ventana se crea con
    /// <see cref="PushBackFullEditorCharacterizationSupport.Ventana"/>, su carga va dentro del <c>try</c> y se cierra en el
    /// <c>finally</c>; ninguna se muestra, no se usa el teclado ni el foco real, y cada metodo corre dentro de
    /// <see cref="EditorDiscardPrompt.Substitute(System.Func{string,bool})"/> con un contador de descartes que la ultima linea de cada
    /// ventana lleva, leido despues de cerrarla.</para>
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackStateNetCellAndFrontTests
    {
        /// <summary>Cierra la ventana y agrega su ultima linea con el contador de descartes leido despues.</summary>
        private static void Fin(RackPushBackSystemWindow w, List<string> log, string linea, Func<int> descartes)
        {
            Cerrar(w);
            log.Add(linea + " descartes=" + descartes());
        }

        /// <summary>Los cuatro numeros del frente que fijan las cajas del panel: <c>posiciones/niveles/fondos/inicio</c>.</summary>
        private static string Frente(string salida, string ruta)
            => salida.StartsWith("NO-VÁLIDA", StringComparison.Ordinal)
                ? salida
                : Leer(salida, ruta + ".PalletCount") + "/" + Leer(salida, ruta + ".LoadLevels") + "/" + Leer(salida, ruta + ".PalletsDeep")
               + "/" + Leer(salida, ruta + ".DepthStartPosition");

        private static void Escribir(RackPushBackSystemWindow w, string caja, double valor) => SetNumberR(w, caja, valor);

        [Fact]
        public void CT_PBT1A_03_ThePositionsLevelsDepthAndDepthStartBoxesOfAFrontReachTheOutputAloneAndInBothSides()
        {
            var log = new List<string>();
            var descartes = 0;
            using (EditorDiscardPrompt.Substitute(_ => { descartes++; return true; }))
            {
                Sta(() =>
                {
                    // Un rack de un sentido (pb-03-fondos, con fondos 3, 5 y 4): cada caja del frente, sola, en una ventana nueva.
                    foreach (var paso in new[] { ("FondosBox", 1, 4.0), ("DepthStartBox", 0, 2.0), ("PositionsBox", 1, 2.0), ("LevelsBox", 1, 2.0) })
                    {
                        var solo = Ventana();
                        try
                        {
                            Cargar(solo, Fixture("pb-03-fondos"));
                            solo.SelectMatrixCell(paso.Item2, 0, false);
                            var ruta = "Structure.Fronts[" + paso.Item2 + "]";
                            var antes = Frente(Salida(solo), ruta);
                            Escribir(solo, paso.Item1, paso.Item3);
                            Fin(solo, log, "sencillo " + paso.Item1 + "=" + paso.Item3.ToString(CultureInfo.InvariantCulture) + " F" + paso.Item2
                                + " antes=" + antes + " despues=" + Frente(Salida(solo), ruta), () => descartes);
                        }
                        finally { Cerrar(solo); }
                    }

                    // Un compuesto con «Ambos lados»: el frente 1 de los dos lados.
                    var ambos = Ventana();
                    try
                    {
                        Cargar(ambos, Fixture("pb-20-ab"));
                        ambos.SelectMatrixCell(1, 0, false);
                        Combo(ambos, "SideSelectorBox").SelectedIndex = 2;
                        log.Add("ambos carga A1=" + Frente(Salida(ambos), "Structure.Fronts[1]") + " B1=" + Frente(Salida(ambos), "SideB.Fronts[1]"));
                        foreach (var paso in new[] { ("FondosBox", 7.0), ("DepthStartBox", 2.0), ("PositionsBox", 2.0), ("LevelsBox", 2.0) })
                        {
                            Escribir(ambos, paso.Item1, paso.Item2);
                            var salida = Salida(ambos);
                            log.Add("ambos " + paso.Item1 + "=" + paso.Item2.ToString(CultureInfo.InvariantCulture)
                                + (salida.StartsWith("NO-VÁLIDA", StringComparison.Ordinal)
                                    ? " " + salida
                                    : " A1=" + Frente(salida, "Structure.Fronts[1]") + " B1=" + Frente(salida, "SideB.Fronts[1]")));
                        }

                        Fin(ambos, log, "ambos fin A0=" + Frente(Salida(ambos), "Structure.Fronts[0]") + " B0=" + Frente(Salida(ambos), "SideB.Fronts[0]"), () => descartes);
                    }
                    finally { Cerrar(ambos); }
                });
            }

            Assert.Equal(
            new[]
            {
                "sencillo FondosBox=4 F1 antes=1/3/5/1 despues=1/3/4/1 descartes=0",
                "sencillo DepthStartBox=2 F0 antes=1/3/3/1 despues=1/3/3/2 descartes=0",
                "sencillo PositionsBox=2 F1 antes=1/3/5/1 despues=2/3/5/1 descartes=0",
                "sencillo LevelsBox=2 F1 antes=1/3/5/1 despues=1/2/5/1 descartes=0",
                "ambos carga A1=1/3/8/1 B1=1/3/8/1",
                "ambos FondosBox=7 A1=1/3/7/1 B1=1/3/7/1",
                "ambos DepthStartBox=2 A1=1/3/7/2 B1=1/3/7/2",
                "ambos PositionsBox=2 A1=2/3/7/2 B1=2/3/7/2",
                "ambos LevelsBox=2 A1=2/2/7/2 B1=2/2/7/2",
                "ambos fin A0=1/3/8/1 B0=1/3/8/1 descartes=0",
            },
            log);
        }

        [Fact]
        public void CT_PBT1A_04_TheIntermediatePeralteTheOwnDepthAndTheDrawPalletBoxesReachTheCellOfTheOutput()
        {
            var log = new List<string>();
            var descartes = 0;
            using (EditorDiscardPrompt.Substitute(_ => { descartes++; return true; }))
            {
                Sta(() =>
                {
                    const string Celda = "Structure.Fronts[0].Levels[0]";

                    // El peralte del larguero intermedio: otra opcion del desplegable.
                    var peralte = Ventana();
                    try
                    {
                        Cargar(peralte, Fixture("pb-01-base"));
                        var caja = Combo(peralte, "CellIntermediatePeralteBox");
                        var opciones = caja.Items.Cast<double>().ToList();
                        var antes = Leer(Salida(peralte), Celda + ".IntermediateBeamDepth");
                        var actual = double.Parse(antes, CultureInfo.InvariantCulture);
                        var otra = opciones.First(o => o != actual);
                        caja.SelectedItem = otra;
                        Fin(peralte, log, "CellIntermediatePeralteBox opciones=[" + string.Join(",", opciones.Select(o => o.ToString("R", CultureInfo.InvariantCulture)))
                            + "] antes=" + antes + " elegido=" + otra.ToString("R", CultureInfo.InvariantCulture)
                            + " despues=" + Leer(Salida(peralte), Celda + ".IntermediateBeamDepth"), () => descartes);
                    }
                    finally { Cerrar(peralte); }

                    // El fondo propio de la celda.
                    var fondo = Ventana();
                    try
                    {
                        Cargar(fondo, Fixture("pb-01-base"));
                        var antes = Leer(Salida(fondo), "Fronts[0].PalletsDeepOverrides");
                        SetNumberR(fondo, "CellFondoOverrideBox", 6);
                        Fin(fondo, log, "CellFondoOverrideBox=6 antes=" + antes + " despues=" + Leer(Salida(fondo), "Fronts[0].PalletsDeepOverrides"), () => descartes);
                    }
                    finally { Cerrar(fondo); }

                    // La tarima de la celda.
                    var tarima = Ventana();
                    try
                    {
                        Cargar(tarima, Fixture("pb-01-base"));
                        var antes = Leer(Salida(tarima), "Fronts[0].DrawPallets");
                        Check(tarima, "CellDrawPalletCheck").IsChecked = true;
                        Fin(tarima, log, "CellDrawPalletCheck=True antes=" + antes + " despues=" + Leer(Salida(tarima), "Fronts[0].DrawPallets"), () => descartes);
                    }
                    finally { Cerrar(tarima); }
                });
            }

            Assert.Equal(
            new[]
            {
                "CellIntermediatePeralteBox opciones=[3,3.5,4,4.5,5,5.5,6] antes=3.5 elegido=3 despues=3 descartes=0",
                "CellFondoOverrideBox=6 antes=<null> despues=[6,null,null] descartes=0",
                "CellDrawPalletCheck=True antes=<null> despues=[true,null,null] descartes=0",
            },
            log);
        }

        /// <summary>Lo que fija CT-PBT1A-07 de una salida: la celda exacta, las dos portadoras del rack y la tolerancia.</summary>
        private static string Exacto(string salida)
            => salida.StartsWith("NO-VÁLIDA", StringComparison.Ordinal)
                ? salida
                : "c00=" + Leer(salida, "Structure.Fronts[0].Levels[0].PalletHeight")
                  + " c10=" + Leer(salida, "Structure.Fronts[1].Levels[0].PalletHeight")
                  + " PalletTolerance=" + Leer(salida, "Structure.PalletTolerance")
                  + " AnnotationScale=" + Leer(salida, "Structure.AnnotationScale");

        /// <summary>Los avanzados del rack de la salida.</summary>
        private static string Avanzados(string salida)
            => "ManualHeaderHeightOverride=" + Leer(salida, "Structure.ManualHeaderHeightOverride")
               + " DerivedPostReinforced=" + Leer(salida, "Structure.DerivedPostReinforced")
               + " SeparatorCountOverride=" + Leer(salida, "Structure.SeparatorCountOverride")
               + " SeparatorSpacingOverride=" + Leer(salida, "Structure.SeparatorSpacingOverride");

        [Fact]
        public void CT_PBT1A_07_AnUneditedExactValueReachesTheOutputExactAndTheRackCarriersAndTheAdvancedScopesAreKept()
        {
            var log = new List<string>();
            var descartes = 0;
            using (EditorDiscardPrompt.Substitute(_ => { descartes++; return true; }))
            {
                Sta(() =>
                {
                    // M-03 de PB-TC y M-04: la celda exacta (61,23456) y los portadores del rack (3,00044 y 1,23456).
                    var exacto = Ventana();
                    try
                    {
                        Cargar(exacto, EditInner(Fixture("pb-08-redondeo"), inner => Poner(inner, "Structure.PalletTolerance", 3.00044)));
                        log.Add("carga " + Exacto(Salida(exacto)));
                        Click(exacto, "ApplyAllButton");
                        log.Add("alcance Todas " + Exacto(Salida(exacto)));
                        Click(exacto, "ApplyAllFrontDataButton");
                        log.Add("Aplicar a todos los frentes " + Exacto(Salida(exacto)));
                        Click(exacto, "ApplyFrontDataButton");
                        log.Add("datos del frente " + Exacto(Salida(exacto)));
                        exacto.SelectMatrixCell(1, 0, false);
                        log.Add("celda (1,0) " + Exacto(Salida(exacto)));
                        exacto.SelectMatrixCell(0, 0, false);
                        log.Add("celda (0,0) " + Exacto(Salida(exacto)));
                        Click(exacto, "RestoreButton");
                        Fin(exacto, log, "Restaurar " + Exacto(Salida(exacto)), () => descartes);
                    }
                    finally { Cerrar(exacto); }

                    // El caso negativo de D-PB-11: «Restaurar modulo» individual con «Confirmar» no reinicia los avanzados.
                    var avanzados = Ventana();
                    try
                    {
                        Cargar(avanzados, Fixture("pb-06-seguridad"));
                        log.Add("avanzados carga " + Avanzados(Salida(avanzados)));
                        SelectModule(avanzados, "M2");
                        SetNumberR(avanzados, "ModuleLengthBox", ValueOf(avanzados, "ModuleLengthBox").Value + 8.0);
                        Click(avanzados, "ConfirmModuleButton");
                        log.Add("avanzados M2 +8 confirmado " + Avanzados(Salida(avanzados)) + " " + ModuleSequence(avanzados));
                        SelectModule(avanzados, "M2");
                        Click(avanzados, "RestoreModuleButton");
                        Click(avanzados, "ConfirmModuleButton");
                        Fin(avanzados, log, "avanzados M2 restaurado y confirmado " + Avanzados(Salida(avanzados)) + " " + ModuleSequence(avanzados), () => descartes);
                    }
                    finally { Cerrar(avanzados); }
                });
            }

            Assert.Equal(
            new[]
            {
                "carga c00=61.23456 c10=60 PalletTolerance=3.00044 AnnotationScale=1.23456",
                "alcance Todas c00=61.23456 c10=61.23456 PalletTolerance=3.00044 AnnotationScale=1.23456",
                "Aplicar a todos los frentes c00=61.23456 c10=61.23456 PalletTolerance=3.00044 AnnotationScale=1.23456",
                "datos del frente c00=61.23456 c10=61.23456 PalletTolerance=3.00044 AnnotationScale=1.23456",
                "celda (1,0) c00=61.23456 c10=61.23456 PalletTolerance=3.00044 AnnotationScale=1.23456",
                "celda (0,0) c00=61.23456 c10=61.23456 PalletTolerance=3.00044 AnnotationScale=1.23456",
                "Restaurar c00=61.23456 c10=61.23456 PalletTolerance=3.00044 AnnotationScale=1.23456 descartes=0",
                "avanzados carga ManualHeaderHeightOverride=150 DerivedPostReinforced=false SeparatorCountOverride=3 SeparatorSpacingOverride=18",
                "avanzados M2 +8 confirmado ManualHeaderHeightOverride=150 DerivedPostReinforced=false SeparatorCountOverride=3 SeparatorSpacingOverride=18 M1/HeaderStart/54/calc/-;M2/Separator/56/-/manual;M3/HeaderIntermediate/48/calc/-;M4/Separator/48/calc/-;M5/Separator/48/calc/-;M6/HeaderIntermediate/48/calc/-;M7/Separator/48/calc/-;M8/HeaderEnd/54/calc/-",
                "avanzados M2 restaurado y confirmado ManualHeaderHeightOverride=150 DerivedPostReinforced=false SeparatorCountOverride=3 SeparatorSpacingOverride=18 M1/HeaderStart/54/calc/-;M2/Separator/48/calc/-;M3/HeaderIntermediate/48/calc/-;M4/Separator/48/calc/-;M5/Separator/48/calc/-;M6/HeaderIntermediate/48/calc/-;M7/Separator/48/calc/-;M8/HeaderEnd/54/calc/- descartes=0",
            },
            log);
        }

        /// <summary>El alto de la celda y la altura del primer nivel del frente, en cada lado.</summary>
        private static string Lados(string salida)
            => "A.alto=" + Leer(salida, "Structure.Fronts[0].Levels[0].PalletHeight") + " B.alto=" + Leer(salida, "SideB.Fronts[0].Levels[0].PalletHeight")
               + " A.primer=" + Leer(salida, "Structure.Fronts[0].FirstLevelHeight") + " B.primer=" + Leer(salida, "SideB.Fronts[0].FirstLevelHeight");

        [Fact]
        public void CT_PBT1A_08_AMixedBoxWrittenReachesBothSidesByTheScopeAndByTheFrontDataAndAnUnwrittenOneKeepsEachSide()
        {
            var log = new List<string>();
            var descartes = 0;
            using (EditorDiscardPrompt.Substitute(_ => { descartes++; return true; }))
            {
                Sta(() =>
                {
                    // A y B difieren en el alto de la celda (61.23456 y 60) y en la altura del primer nivel del frente (4 y 5).
                    var mixto = EditInner(
                        Fixture("pb-20-ab"),
                        inner =>
                        {
                            Poner(inner, "Structure.Fronts[0].Levels[0].PalletHeight", 61.23456);
                            Poner(inner, "SideB.Fronts[0].FirstLevelHeight", 5);
                        });

                    // La caja mixta sin escribir: cada lado conserva el suyo, por el alcance «Todas» y por los datos del frente.
                    var sinEscribir = Ventana();
                    try
                    {
                        Cargar(sinEscribir, mixto);
                        log.Add("carga " + Lados(Salida(sinEscribir)));
                        Combo(sinEscribir, "SideSelectorBox").SelectedIndex = 2;
                        log.Add("Ambos CellPalletHeightBox=[" + Field(sinEscribir, "CellPalletHeightBox").Text + "] FirstLevelHeightBox=[" + Field(sinEscribir, "FirstLevelHeightBox").Text + "]");
                        Click(sinEscribir, "ApplyAllButton");
                        log.Add("alcance Todas sin escribir " + Lados(Salida(sinEscribir)));
                        Click(sinEscribir, "ApplyFrontDataButton");
                        Fin(sinEscribir, log, "datos del frente sin escribir " + Lados(Salida(sinEscribir)), () => descartes);
                    }
                    finally { Cerrar(sinEscribir); }

                    // La caja mixta escrita llega a los dos lados por el alcance «Todas».
                    var alcance = Ventana();
                    try
                    {
                        Cargar(alcance, mixto);
                        Combo(alcance, "SideSelectorBox").SelectedIndex = 2;
                        EditorWindowTestSupport.SetText(alcance, "CellPalletHeightBox", "61.235");
                        Click(alcance, "ApplyAllButton");
                        Fin(alcance, log, "alcance Todas CellPalletHeightBox=61.235 " + Lados(Salida(alcance)), () => descartes);
                    }
                    finally { Cerrar(alcance); }

                    // La caja mixta escrita llega a los dos lados por los datos del frente.
                    var datos = Ventana();
                    try
                    {
                        Cargar(datos, mixto);
                        Combo(datos, "SideSelectorBox").SelectedIndex = 2;
                        EditorWindowTestSupport.SetText(datos, "FirstLevelHeightBox", "6");
                        Click(datos, "ApplyFrontDataButton");
                        Fin(datos, log, "datos del frente FirstLevelHeightBox=6 " + Lados(Salida(datos)), () => descartes);
                    }
                    finally { Cerrar(datos); }
                });
            }

            Assert.Equal(
            new[]
            {
                "carga A.alto=61.23456 B.alto=60 A.primer=4 B.primer=5",
                "Ambos CellPalletHeightBox=[] FirstLevelHeightBox=[]",
                "alcance Todas sin escribir A.alto=61.23456 B.alto=60 A.primer=4 B.primer=5",
                "datos del frente sin escribir A.alto=61.23456 B.alto=60 A.primer=4 B.primer=5 descartes=0",
                "alcance Todas CellPalletHeightBox=61.235 A.alto=61.235 B.alto=61.235 A.primer=4 B.primer=5 descartes=0",
                "datos del frente FirstLevelHeightBox=6 A.alto=61.23456 B.alto=60 A.primer=6 B.primer=6 descartes=0",
            },
            log);
        }
    }
}
