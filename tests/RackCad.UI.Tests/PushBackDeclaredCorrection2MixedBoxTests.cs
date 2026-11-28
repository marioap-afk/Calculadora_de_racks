using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Nodes;
using System.Windows;
using RackCad.UI.Shell;
using Xunit;
using static RackCad.UI.Tests.PushBackDeclaredCorrectionSupport;
using static RackCad.UI.Tests.PushBackFullEditorCharacterizationSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4B PB-TC2 — la decision del dueño OWNER-1 en la ventana CLASICA de Push Back: en «Ambos lados», con el lado B presente,
    /// una caja que el panel pinta como mixta (A y B difieren) y recibe un valor lo aplica a los dos lados; una caja que NO es
    /// mixta, o que lo parece sin lado B, conserva el valor exacto del lado (la regla de PB-TC). CT-PBTC2-04 lo fija con cuatro
    /// ventanas: la mixta, la no mixta, la que retira el lado B (el indicador obsoleto) y la que cambia de celda sin salir de la caja.
    ///
    /// <para>El metodo arma una lista de lineas y termina en UN <c>Assert.Equal</c>. Toda ventana se crea con
    /// <see cref="PushBackFullEditorCharacterizationSupport.Ventana"/> y su carga va dentro del <c>try</c>; ninguna se muestra, no
    /// se usa el teclado ni el foco real, y el metodo corre dentro de <see cref="EditorDiscardPrompt.Substitute(System.Func{string,bool})"/>
    /// con un contador de descartes que la ultima linea de cada ventana lleva, leido despues de cerrarla.</para>
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackDeclaredCorrection2MixedBoxTests
    {
        private const string AltoA = "Structure.Fronts[0].Levels[0].PalletHeight";
        private const string AltoB = "SideB.Fronts[0].Levels[0].PalletHeight";
        private const string LargoA = "Structure.Fronts[0].Levels[0].BeamLengthOverride";
        private const string LargoB = "SideB.Fronts[0].Levels[0].BeamLengthOverride";

        /// <summary>NumR(json, ruta): el numero en esa ruta (bajo <c>PushBack</c>) del JSON persistido, con «R», o <c>&lt;null&gt;</c>.</summary>
        private static string NumR(string json, string ruta)
        {
            var node = JsonNode.Parse(json)["PushBack"];
            foreach (var segment in ruta.Split('.'))
            {
                var bracket = segment.IndexOf('[');
                var name = bracket < 0 ? segment : segment.Substring(0, bracket);
                node = node?[name];
                while (bracket >= 0 && node != null)
                {
                    var close = segment.IndexOf(']', bracket);
                    node = node[int.Parse(segment.Substring(bracket + 1, close - bracket - 1), CultureInfo.InvariantCulture)];
                    bracket = segment.IndexOf('[', close);
                }
            }

            return node == null ? "<null>" : node.GetValue<double>().ToString("R", CultureInfo.InvariantCulture);
        }

        [Fact]
        public void CT_PBTC2_04_AMixedBoxThatReceivesAValueAppliesItToBothSidesAndANonMixedBoxKeepsTheExactValue()
        {
            var log = new List<string>();
            var descartes = 0;
            using (EditorDiscardPrompt.Substitute(_ => { descartes++; return true; }))
            {
                Sta(() =>
                {
                    // D-mixto: A y B difieren en el alto (61.23456 y 60) y en el largo manual (100.00044 y 120).
                    var mixto = EditInner(
                        Fixture("pb-20-ab"),
                        inner =>
                        {
                            Poner(inner, AltoA, 61.23456);
                            Poner(inner, LargoA, 100.00044);
                            Poner(inner, LargoB, 120);
                        });

                    // Ventana mixta: una caja mixta que recibe un valor lo aplica a los dos lados.
                    var mixta = Ventana();
                    try
                    {
                        Cargar(mixta, mixto);
                        var inicial = Salida(mixta);
                        log.Add("mixta carga A.alto=" + NumR(inicial, AltoA) + " B.alto=" + NumR(inicial, AltoB)
                            + " A.largo=" + NumR(inicial, LargoA) + " B.largo=" + NumR(inicial, LargoB));

                        Combo(mixta, "SideSelectorBox").SelectedIndex = 2;
                        log.Add("mixta Ambos CellPalletHeightBox texto=[" + Field(mixta, "CellPalletHeightBox").Text
                            + "] CellBeamLengthOverrideBox texto=[" + Field(mixta, "CellBeamLengthOverrideBox").Text + "]");

                        var alto = Field(mixta, "CellPalletHeightBox");
                        EditorWindowTestSupport.SetText(mixta, "CellPalletHeightBox", "61.235");
                        alto.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, alto));
                        var tras = Salida(mixta);
                        log.Add("mixta CellPalletHeightBox=61.235 A.alto=" + NumR(tras, AltoA) + " B.alto=" + NumR(tras, AltoB)
                            + " A.largo=" + NumR(tras, LargoA) + " B.largo=" + NumR(tras, LargoB));

                        var largo = Field(mixta, "CellBeamLengthOverrideBox");
                        EditorWindowTestSupport.SetText(mixta, "CellBeamLengthOverrideBox", "100");
                        largo.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, largo));
                        var final = Salida(mixta);
                        var linea = "mixta CellBeamLengthOverrideBox=100 A.largo=" + NumR(final, LargoA) + " B.largo=" + NumR(final, LargoB);
                        Cerrar(mixta);
                        log.Add(linea + " descartes=" + descartes);
                    }
                    finally { Cerrar(mixta); }

                    // Ventana no mixta: A y B tienen el mismo alto exacto; escribir lo que se ve conserva el exacto.
                    var igual = EditInner(
                        Fixture("pb-20-ab"),
                        inner =>
                        {
                            Poner(inner, AltoA, 61.23456);
                            Poner(inner, AltoB, 61.23456);
                        });
                    var noMixta = Ventana();
                    try
                    {
                        Cargar(noMixta, igual);
                        var inicial = Salida(noMixta);
                        log.Add("no mixta carga A.alto=" + NumR(inicial, AltoA) + " B.alto=" + NumR(inicial, AltoB));

                        Combo(noMixta, "SideSelectorBox").SelectedIndex = 2;
                        log.Add("no mixta Ambos CellPalletHeightBox texto=[" + Field(noMixta, "CellPalletHeightBox").Text + "]");

                        var alto = Field(noMixta, "CellPalletHeightBox");
                        EditorWindowTestSupport.SetText(noMixta, "CellPalletHeightBox", "61.2350");
                        alto.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, alto));
                        var tras = Salida(noMixta);
                        var linea = "no mixta CellPalletHeightBox texto=[" + alto.Text + "] A.alto=" + NumR(tras, AltoA) + " B.alto=" + NumR(tras, AltoB);
                        Cerrar(noMixta);
                        log.Add(linea + " descartes=" + descartes);
                    }
                    finally { Cerrar(noMixta); }

                    // Ventana sin lado B: el indicador mixto queda obsoleto y la caja sembrada conserva el exacto de A.
                    var sinB = Ventana();
                    try
                    {
                        Cargar(sinB, mixto);
                        var inicial = Salida(sinB);
                        log.Add("sin lado B carga A.alto=" + NumR(inicial, AltoA) + " B.alto=" + NumR(inicial, AltoB));

                        Combo(sinB, "SideSelectorBox").SelectedIndex = 2;
                        log.Add("sin lado B Ambos CellPalletHeightBox texto=[" + Field(sinB, "CellPalletHeightBox").Text + "]");

                        Check(sinB, "SideBPresentCheck").IsChecked = false;
                        sinB.SelectMatrixCell(0, 0, false);
                        var tras = Salida(sinB);
                        var linea = "sin lado B CellPalletHeightBox texto=[" + Field(sinB, "CellPalletHeightBox").Text + "] A.alto=" + NumR(tras, AltoA);
                        Cerrar(sinB);
                        log.Add(linea + " descartes=" + descartes);
                    }
                    finally { Cerrar(sinB); }

                    // Ventana cambio de celda: la caja mixta escrita se confirma, con la regla de «Ambos lados», al cambiar de celda.
                    var cambio = Ventana();
                    try
                    {
                        Cargar(cambio, mixto);
                        var inicial = Salida(cambio);
                        log.Add("cambio de celda carga A.alto=" + NumR(inicial, AltoA) + " B.alto=" + NumR(inicial, AltoB));

                        Combo(cambio, "SideSelectorBox").SelectedIndex = 2;
                        log.Add("cambio de celda Ambos CellPalletHeightBox texto=[" + Field(cambio, "CellPalletHeightBox").Text + "]");

                        EditorWindowTestSupport.SetText(cambio, "CellPalletHeightBox", "61.235");
                        cambio.SelectMatrixCell(0, 1, false);
                        var tras = Salida(cambio);
                        var linea = "cambio de celda CellPalletHeightBox=61.235 seleccion(" + cambio.State.Structure.SelectedFrontIndex
                            + "," + cambio.State.Structure.SelectedLevelIndex + ") A.alto=" + NumR(tras, AltoA) + " B.alto=" + NumR(tras, AltoB);
                        Cerrar(cambio);
                        log.Add(linea + " descartes=" + descartes);
                    }
                    finally { Cerrar(cambio); }
                });
            }

            Assert.Equal(
                new[]
                {
                    "mixta carga A.alto=61.23456 B.alto=60 A.largo=100.00044 B.largo=120",
                    "mixta Ambos CellPalletHeightBox texto=[] CellBeamLengthOverrideBox texto=[]",
                    "mixta CellPalletHeightBox=61.235 A.alto=61.235 B.alto=61.235 A.largo=100.00044 B.largo=120",
                    "mixta CellBeamLengthOverrideBox=100 A.largo=100 B.largo=100 descartes=0",
                    "no mixta carga A.alto=61.23456 B.alto=61.23456",
                    "no mixta Ambos CellPalletHeightBox texto=[61.235]",
                    "no mixta CellPalletHeightBox texto=[61.2350] A.alto=61.23456 B.alto=61.23456 descartes=0",
                    "sin lado B carga A.alto=61.23456 B.alto=60",
                    "sin lado B Ambos CellPalletHeightBox texto=[]",
                    "sin lado B CellPalletHeightBox texto=[61.235] A.alto=61.23456 descartes=0",
                    "cambio de celda carga A.alto=61.23456 B.alto=60",
                    "cambio de celda Ambos CellPalletHeightBox texto=[]",
                    "cambio de celda CellPalletHeightBox=61.235 seleccion(0,1) A.alto=61.235 B.alto=61.235 descartes=0",
                },
                log);
        }
    }
}
