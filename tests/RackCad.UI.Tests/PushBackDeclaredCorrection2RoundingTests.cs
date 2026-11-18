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
    /// I-64 F4B PB-TC2 — la correccion declarada (H-31) de D-PB-06 en la ventana CLASICA de Push Back, para las seis cajas que
    /// aun redondeaban: una caja que el usuario no edito no cambia el diseño (conserva el valor exacto del modelo) y una caja
    /// editada a un numero que se ve distinto manda. CT-PBTC2-01 fija el hueco, CT-PBTC2-02 la longitud de un modulo (pasar por
    /// la caja no vuelve manual un modulo calculado, y escribir otra longitud si) y CT-PBTC2-03 las cuatro avanzadas anulables
    /// (una caja vacia sigue siendo «el cálculo»).
    ///
    /// <para>Cada metodo arma una lista de lineas y termina en UN <c>Assert.Equal</c>. Toda ventana se crea con
    /// <see cref="PushBackFullEditorCharacterizationSupport.Ventana"/> y su carga va dentro del <c>try</c>; ninguna se muestra, no
    /// se usa el teclado ni el foco real, y cada metodo corre dentro de <see cref="EditorDiscardPrompt.Substitute(System.Func{string,bool})"/>
    /// con un contador de descartes que la ultima linea de cada ventana lleva, leido despues de cerrarla.</para>
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackDeclaredCorrection2RoundingTests
    {
        private const string Hueco = "Composite.Gap";
        private const string Modulo = "Structure.Modules[1]";
        private const string Cabecera = "Structure.ManualHeaderHeightOverride";
        private const string Separacion = "Structure.SeparatorSpacingOverride";
        private const string Cuenta = "Structure.SeparatorCountOverride";
        private const string Poste = "Structure.DerivedPostHeight";
        private const string Refuerzo = "Structure.DerivedPostReinforcementHeight";

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

        /// <summary>BoolR(json, ruta): el booleano en esa ruta (bajo <c>PushBack</c>) del JSON persistido, o <c>&lt;null&gt;</c>.</summary>
        private static string BoolR(string json, string ruta)
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

            return node == null ? "<null>" : node.GetValue<bool>().ToString();
        }

        /// <summary>StrR(json, ruta): el texto en esa ruta (bajo <c>PushBack</c>) del JSON persistido, o <c>&lt;null&gt;</c>.</summary>
        private static string StrR(string json, string ruta)
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

            return node == null ? "<null>" : node.GetValue<string>();
        }

        [Fact]
        public void CT_PBTC2_01_AnUneditedGapBoxKeepsTheExactGapUpToTheUpdate()
        {
            var log = new List<string>();
            var descartes = 0;
            using (EditorDiscardPrompt.Substitute(_ => { descartes++; return true; }))
            {
                Sta(() =>
                {
                    var texto = EditInner(Fixture("pb-20-ab"), inner => Poner(inner, Hueco, 54.00044));

                    // Ventana 1: la caja se siembra con «54», sale sin editar y «Actualizar» lleva el hueco exacto.
                    var w = Ventana();
                    try
                    {
                        Cargar(w, texto);
                        log.Add("carga Gap=" + NumR(Salida(w), Hueco));
                        log.Add("GapBox texto=" + Field(w, "GapBox").Text);

                        var caja = Field(w, "GapBox");
                        caja.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, caja));
                        log.Add("GapBox sin editar Gap=" + NumR(Salida(w), Hueco));

                        Click(w, "UpdateButton");
                        var linea = "peticion Gap=" + NumR(Persist(w.DesignToInsert), Hueco);
                        Cerrar(w);
                        log.Add(linea + " descartes=" + descartes);
                    }
                    finally { Cerrar(w); }

                    // Ventana 2: escribir otro numero manda.
                    var editada = Ventana();
                    try
                    {
                        Cargar(editada, texto);
                        SetNumberR(editada, "GapBox", 60);
                        var linea = "GapBox=" + Texto(60) + " Gap=" + NumR(Salida(editada), Hueco);
                        Cerrar(editada);
                        log.Add(linea + " descartes=" + descartes);
                    }
                    finally { Cerrar(editada); }
                });
            }

            Assert.Equal(
                new[]
                {
                    "carga Gap=54.00044",
                    "GapBox texto=54",
                    "GapBox sin editar Gap=54.00044",
                    "peticion Gap=54.00044 descartes=0",
                    "GapBox=60 Gap=60 descartes=0",
                },
                log);
        }

        [Fact]
        public void CT_PBTC2_02_PassingThroughTheModuleLengthKeepsACalculatedModuleAndAnEditMakesItManual()
        {
            var log = new List<string>();
            var descartes = 0;
            using (EditorDiscardPrompt.Substitute(_ => { descartes++; return true; }))
            {
                Sta(() =>
                {
                    var texto = EditInner(Fixture("pb-31-v2-modulos"), inner => Poner(inner, Modulo + ".Length", 47.99213));

                    // Ventana 1: pasar por la longitud sin editar no escenifica nada.
                    var w = Ventana();
                    try
                    {
                        Cargar(w, texto);
                        SelectModule(w, "M2");
                        log.Add("M2 texto=" + Field(w, "ModuleLengthBox").Text);

                        var caja = Field(w, "ModuleLengthBox");
                        caja.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, caja));
                        log.Add("sin editar pendiente=" + w.PendingWork().HasPendingWork);

                        Click(w, "UpdateButton");
                        var diseno = Persist(w.DesignToInsert);
                        var linea = "sin editar peticion " + StrR(diseno, Modulo + ".ModuleId")
                            + " Length=" + NumR(diseno, Modulo + ".Length")
                            + " IsCalculated=" + BoolR(diseno, Modulo + ".IsCalculated")
                            + " IsManualOverride=" + BoolR(diseno, Modulo + ".IsManualOverride");
                        Cerrar(w);
                        log.Add(linea + " descartes=" + descartes);
                    }
                    finally { Cerrar(w); }

                    // Ventana 2: escribir otra longitud escenifica el cambio y el modulo pasa a manual.
                    var editada = Ventana();
                    try
                    {
                        Cargar(editada, texto);
                        SelectModule(editada, "M2");
                        SetNumberR(editada, "ModuleLengthBox", 56);
                        log.Add("editado pendiente=" + editada.PendingWork().HasPendingWork);

                        Click(editada, "UpdateButton");
                        var diseno = Persist(editada.DesignToInsert);
                        var linea = "editado peticion " + StrR(diseno, Modulo + ".ModuleId")
                            + " Length=" + NumR(diseno, Modulo + ".Length")
                            + " IsCalculated=" + BoolR(diseno, Modulo + ".IsCalculated")
                            + " IsManualOverride=" + BoolR(diseno, Modulo + ".IsManualOverride");
                        Cerrar(editada);
                        log.Add(linea + " descartes=" + descartes);
                    }
                    finally { Cerrar(editada); }
                });
            }

            Assert.Equal(
                new[]
                {
                    "M2 texto=47.992",
                    "sin editar pendiente=False",
                    "sin editar peticion M2 Length=47.99213 IsCalculated=True IsManualOverride=False descartes=0",
                    "editado pendiente=True",
                    "editado peticion M2 Length=56 IsCalculated=False IsManualOverride=True descartes=0",
                },
                log);
        }

        [Fact]
        public void CT_PBTC2_03_UneditedNullableAdvancedBoxesKeepTheExactValueAndAnEmptyBoxIsTheCalculation()
        {
            var log = new List<string>();
            var descartes = 0;
            using (EditorDiscardPrompt.Substitute(_ => { descartes++; return true; }))
            {
                Sta(() =>
                {
                    // pb-06-seguridad: la cabecera y la separacion con mas de tres decimales.
                    var seguridad = EditInner(
                        Fixture("pb-06-seguridad"),
                        inner =>
                        {
                            Poner(inner, Cabecera, 150.00044);
                            Poner(inner, Separacion, 18.00044);
                        });
                    var w = Ventana();
                    try
                    {
                        Cargar(w, seguridad);
                        Check(w, "AdvancedModulesToggle").IsChecked = true;
                        log.Add("pb-06-seguridad carga ManualHeaderHeightOverride=" + NumR(Salida(w), Cabecera)
                            + " SeparatorSpacingOverride=" + NumR(Salida(w), Separacion));
                        log.Add("pb-06-seguridad textos RackHeaderHeightBox=" + Field(w, "RackHeaderHeightBox").Text
                            + " RackSeparatorSpacingBox=" + Field(w, "RackSeparatorSpacingBox").Text);

                        var cuenta = Field(w, "RackSeparatorCountBox");
                        cuenta.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, cuenta));
                        log.Add("pb-06-seguridad sin editar ManualHeaderHeightOverride=" + NumR(Salida(w), Cabecera)
                            + " SeparatorSpacingOverride=" + NumR(Salida(w), Separacion));

                        SetNumberR(w, "RackSeparatorSpacingBox", 21.5);
                        log.Add("pb-06-seguridad RackSeparatorSpacingBox=" + Texto(21.5) + " ManualHeaderHeightOverride=" + NumR(Salida(w), Cabecera)
                            + " SeparatorSpacingOverride=" + NumR(Salida(w), Separacion));

                        var altura = Field(w, "RackHeaderHeightBox");
                        EditorWindowTestSupport.SetText(w, "RackHeaderHeightBox", string.Empty);
                        altura.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, altura));
                        log.Add("pb-06-seguridad RackHeaderHeightBox vacia ManualHeaderHeightOverride=" + NumR(Salida(w), Cabecera)
                            + " SeparatorSpacingOverride=" + NumR(Salida(w), Separacion));

                        Click(w, "UpdateButton");
                        var diseno = Persist(w.DesignToInsert);
                        var linea = "pb-06-seguridad peticion ManualHeaderHeightOverride=" + NumR(diseno, Cabecera)
                            + " SeparatorSpacingOverride=" + NumR(diseno, Separacion)
                            + " SeparatorCountOverride=" + NumR(diseno, Cuenta);
                        Cerrar(w);
                        log.Add(linea + " descartes=" + descartes);
                    }
                    finally { Cerrar(w); }

                    // pb-07-combinado: el poste derivado y su refuerzo con mas de tres decimales.
                    var combinado = EditInner(
                        Fixture("pb-07-combinado"),
                        inner =>
                        {
                            Poner(inner, Poste, 140.00044);
                            Poner(inner, Refuerzo, 24.00044);
                        });
                    var v = Ventana();
                    try
                    {
                        Cargar(v, combinado);
                        Check(v, "AdvancedModulesToggle").IsChecked = true;
                        log.Add("pb-07-combinado carga DerivedPostHeight=" + NumR(Salida(v), Poste)
                            + " DerivedPostReinforcementHeight=" + NumR(Salida(v), Refuerzo));
                        log.Add("pb-07-combinado textos DerivedPostHeightBox=" + Field(v, "DerivedPostHeightBox").Text
                            + " DerivedPostReinforcementHeightBox=" + Field(v, "DerivedPostReinforcementHeightBox").Text);

                        var poste = Field(v, "DerivedPostHeightBox");
                        poste.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, poste));
                        log.Add("pb-07-combinado sin editar DerivedPostHeight=" + NumR(Salida(v), Poste)
                            + " DerivedPostReinforcementHeight=" + NumR(Salida(v), Refuerzo));

                        Click(v, "UpdateButton");
                        var diseno = Persist(v.DesignToInsert);
                        var linea = "pb-07-combinado peticion DerivedPostHeight=" + NumR(diseno, Poste)
                            + " DerivedPostReinforcementHeight=" + NumR(diseno, Refuerzo);
                        Cerrar(v);
                        log.Add(linea + " descartes=" + descartes);
                    }
                    finally { Cerrar(v); }
                });
            }

            Assert.Equal(
                new[]
                {
                    "pb-06-seguridad carga ManualHeaderHeightOverride=150.00044 SeparatorSpacingOverride=18.00044",
                    "pb-06-seguridad textos RackHeaderHeightBox=150 RackSeparatorSpacingBox=18",
                    "pb-06-seguridad sin editar ManualHeaderHeightOverride=150.00044 SeparatorSpacingOverride=18.00044",
                    "pb-06-seguridad RackSeparatorSpacingBox=21.5 ManualHeaderHeightOverride=150.00044 SeparatorSpacingOverride=21.5",
                    "pb-06-seguridad RackHeaderHeightBox vacia ManualHeaderHeightOverride=<null> SeparatorSpacingOverride=21.5",
                    "pb-06-seguridad peticion ManualHeaderHeightOverride=<null> SeparatorSpacingOverride=21.5 SeparatorCountOverride=3 descartes=0",
                    "pb-07-combinado carga DerivedPostHeight=140.00044 DerivedPostReinforcementHeight=24.00044",
                    "pb-07-combinado textos DerivedPostHeightBox=140 DerivedPostReinforcementHeightBox=24",
                    "pb-07-combinado sin editar DerivedPostHeight=140.00044 DerivedPostReinforcementHeight=24.00044",
                    "pb-07-combinado peticion DerivedPostHeight=140.00044 DerivedPostReinforcementHeight=24.00044 descartes=0",
                },
                log);
        }
    }
}
