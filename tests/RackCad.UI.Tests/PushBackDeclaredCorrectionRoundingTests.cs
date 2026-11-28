using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using RackCad.Application.Systems.Dynamic;
using RackCad.Domain.Systems.Dynamic;
using RackCad.UI.Editor;
using RackCad.UI.Systems.PushBack;
using Xunit;
using static RackCad.UI.Tests.PushBackDeclaredCorrectionSupport;
using static RackCad.UI.Tests.PushBackFullEditorCharacterizationSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4B PB-TC — la correccion declarada (a) de D-PB-06 en la ventana CLASICA de Push Back: una caja que el usuario no edito
    /// no cambia el diseño (conserva el valor exacto del modelo), y una caja editada a un numero que se ve distinto manda. CT-PBTC-01
    /// la fija al cargar, CT-PBTC-02 en «Actualizar» de RACKEDITAR y en la carga de la biblioteca, y CT-PBTC-03 en la edicion, el
    /// «escribir lo mismo» y la seleccion de celda.
    ///
    /// <para>Cada metodo arma una lista de lineas y termina en UN <c>Assert.Equal</c>. Toda ventana se crea con
    /// <see cref="PushBackFullEditorCharacterizationSupport.Ventana"/> y su carga va dentro del <c>try</c>; ninguna se muestra y no
    /// se usa el teclado ni el foco real.</para>
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackDeclaredCorrectionRoundingTests
    {
        private const string Fondo = "pb-08-redondeo";
        private const string Escala = "Structure.AnnotationScale";
        private const string Altura00 = "Structure.Fronts[0].Levels[0].PalletHeight";
        private const string Altura10 = "Structure.Fronts[1].Levels[0].PalletHeight";

        /// <summary>La clase de una fila (paquete §4.1): solo F4 admite <c>CATALOGO</c>.</summary>
        private static string Clase(FilaF fila, RackPushBackSystemWindow w, double exacto, double salida)
        {
            if (salida == exacto) return "CONSERVA";
            if (fila.Name == "F4"
                && salida == DynamicLoadBeamGeometry.ResolveBeamDepth(w.Session.Catalog, DynamicRackDefaults.InOutBeamCatalogId, exacto))
            {
                return "CATALOGO";
            }

            return salida == R3(exacto) ? "REDONDEA" : "OTRA";
        }

        [Fact]
        public void CT_PBTC_01_TheLoadedFieldsKeepTheirExactValueAfterLoading()
        {
            var log = new List<string>();
            Sta(() =>
            {
                foreach (var fila in FilasF())
                {
                    var w = Ventana();
                    try
                    {
                        Cargar(w, VarianteDe(fila));
                        var salida = Salida(w);
                        if (salida.StartsWith("NO-VÁLIDA", StringComparison.Ordinal))
                        {
                            log.Add(fila.Name + " pb-01-base " + fila.Path + " exacto=" + Texto(fila.Exacto) + " :: " + salida);
                            continue;
                        }

                        var escrito = Num(salida, fila.Path);
                        var valor = escrito == "<null>" ? double.NaN : double.Parse(escrito, CultureInfo.InvariantCulture);
                        log.Add(fila.Name + " pb-01-base " + fila.Path + " exacto=" + Texto(fila.Exacto) + " salida=" + escrito + " "
                            + (escrito == "<null>" ? "OTRA" : Clase(fila, w, fila.Exacto, valor)));
                    }
                    finally { Cerrar(w); }
                }
            });

            Assert.Equal(
                new[]
                {
                    "F1 pb-01-base Structure.PalletDepth exacto=48.00044 salida=48.00044 CONSERVA",
                    "F2 pb-01-base Structure.PostPeralte exacto=3.00044 salida=3.00044 CONSERVA",
                    "F3 pb-01-base Structure.PalletTolerance exacto=4.00044 salida=4.00044 CONSERVA",
                    "F4 pb-01-base Structure.BeamDepth exacto=6.00044 salida=6 CATALOGO",
                    "F5 pb-01-base Structure.AnnotationScale exacto=1.50044 salida=1.50044 CONSERVA",
                    "F6 pb-01-base Structure.Fronts[0].FirstLevelHeight exacto=4.00044 salida=4.00044 CONSERVA",
                    "F7 pb-01-base Structure.Fronts[0].Levels[0].PalletFront exacto=42.00044 salida=42.00044 CONSERVA",
                    "F8 pb-01-base Structure.Fronts[0].Levels[0].PalletHeight exacto=60.00044 salida=60.00044 CONSERVA",
                    "F9 pb-01-base Structure.Fronts[0].Levels[0].PalletWeight exacto=1000.00044 salida=1000.00044 CONSERVA",
                    "F10 pb-01-base Structure.Fronts[0].Levels[0].ClearHeight exacto=6.00044 salida=6.00044 CONSERVA",
                    "F11 pb-01-base Structure.Fronts[0].Levels[0].BeamLengthOverride exacto=100.00044 salida=100.00044 CONSERVA",
                },
                log);
        }

        [Fact]
        public void CT_PBTC_02_UpdateInRackeditarAndTheLibraryLoadCarryTheExactValues()
        {
            var log = new List<string>();
            Sta(() =>
            {
                // (i) Por fila F1 a F11: «Actualizar» de RACKEDITAR.
                foreach (var fila in FilasF())
                {
                    var w = Ventana();
                    try
                    {
                        Cargar(w, VarianteDe(fila));
                        Assert.True(Btn(w, "UpdateButton").IsEnabled, StatusText(w));
                        Click(w, "UpdateButton");
                        var peticion = Assert.IsType<PushBackInsertionRequest>(w.InsertionRequest);
                        log.Add(fila.Name + " RACKEDITAR peticion=" + w.InsertRequested + " updateOnly=" + w.UpdateOnly
                            + " vistas=" + peticion.Views.Count + " diseno=" + Num(Persist(w.DesignToInsert), fila.Path));
                    }
                    finally { Cerrar(w); }
                }

                // (ii) pb-08-redondeo tal cual: «Actualizar» de RACKEDITAR.
                var update = Ventana();
                try
                {
                    Cargar(update, Fixture(Fondo));
                    Assert.True(Btn(update, "UpdateButton").IsEnabled, StatusText(update));
                    Click(update, "UpdateButton");
                    var peticion = Assert.IsType<PushBackInsertionRequest>(update.InsertionRequest);
                    var diseno = Persist(update.DesignToInsert);
                    log.Add(Fondo + " RACKEDITAR peticion=" + update.InsertRequested + " updateOnly=" + update.UpdateOnly
                        + " vistas=" + peticion.Views.Count + " AnnotationScale=" + Num(diseno, Escala)
                        + " PalletHeight=" + Num(diseno, Altura00)
                        + " sistema.AnnotationScale=" + Texto(update.SystemToInsert.Structure.AnnotationScale));
                }
                finally { Cerrar(update); }

                // (iii) pb-08-redondeo por la biblioteca: se carga y se lee la salida, sin pulsar nada.
                var biblioteca = Ventana();
                try
                {
                    CargarBiblioteca(biblioteca, Fixture(Fondo));
                    var salida = Salida(biblioteca);
                    log.Add(Fondo + " biblioteca AnnotationScale=" + Num(salida, Escala) + " PalletHeight=" + Num(salida, Altura00)
                        + " peticion=" + biblioteca.InsertRequested);
                }
                finally { Cerrar(biblioteca); }
            });

            Assert.Equal(
                new[]
                {
                    "F1 RACKEDITAR peticion=True updateOnly=True vistas=0 diseno=48.00044",
                    "F2 RACKEDITAR peticion=True updateOnly=True vistas=0 diseno=3.00044",
                    "F3 RACKEDITAR peticion=True updateOnly=True vistas=0 diseno=4.00044",
                    "F4 RACKEDITAR peticion=True updateOnly=True vistas=0 diseno=6",
                    "F5 RACKEDITAR peticion=True updateOnly=True vistas=0 diseno=1.50044",
                    "F6 RACKEDITAR peticion=True updateOnly=True vistas=0 diseno=4.00044",
                    "F7 RACKEDITAR peticion=True updateOnly=True vistas=0 diseno=42.00044",
                    "F8 RACKEDITAR peticion=True updateOnly=True vistas=0 diseno=60.00044",
                    "F9 RACKEDITAR peticion=True updateOnly=True vistas=0 diseno=1000.00044",
                    "F10 RACKEDITAR peticion=True updateOnly=True vistas=0 diseno=6.00044",
                    "F11 RACKEDITAR peticion=True updateOnly=True vistas=0 diseno=100.00044",
                    "pb-08-redondeo RACKEDITAR peticion=True updateOnly=True vistas=0 AnnotationScale=1.23456 PalletHeight=61.23456 sistema.AnnotationScale=1.23456",
                    "pb-08-redondeo biblioteca AnnotationScale=1.23456 PalletHeight=61.23456 peticion=False",
                },
                log);
        }

        [Fact]
        public void CT_PBTC_03_AnEditedBoxWinsAndAnUneditedOrReselectedBoxKeepsTheExactValue()
        {
            var log = new List<string>();
            Sta(() =>
            {
                // Diseño X-TC-03: pb-08-redondeo con la celda (1,0) tambien con mas de tres decimales.
                var texto = EditInner(Fixture(Fondo), inner => Poner(inner, Altura10, 60.00044));
                var w = Ventana();
                try
                {
                    Cargar(w, texto);
                    log.Add("carga " + Estado(w));

                    SetNumberR(w, "AnnotationScaleBox", 2.5);
                    log.Add("AnnotationScaleBox=" + Texto(2.5) + " " + Estado(w, false));

                    // R-03 (i): «61.2350» se ve igual que «61.235»: se conserva el exacto.
                    Assert.Equal("61.235", Field(w, "CellPalletHeightBox").Text);
                    var altura = Field(w, "CellPalletHeightBox");
                    altura.Text = "61.2350";
                    altura.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, altura));
                    log.Add("CellPalletHeightBox texto=" + altura.Text + " c00=" + Num(Salida(w), Altura00));

                    SetNumberR(w, "CellPalletHeightBox", 70.5);
                    log.Add("CellPalletHeightBox=" + Texto(70.5) + " c00=" + Num(Salida(w), Altura00));

                    w.SelectMatrixCell(1, 0, false);
                    log.Add("seleccion(1,0) caja=" + Field(w, "CellPalletHeightBox").Text + " c00=" + Num(Salida(w), Altura00) + " c10=" + Num(Salida(w), Altura10));

                    w.SelectMatrixCell(0, 0, false);
                    log.Add("seleccion(0,0) caja=" + Field(w, "CellPalletHeightBox").Text + " c00=" + Num(Salida(w), Altura00)
                        + " c10=" + Num(Salida(w), Altura10) + " AnnotationScale=" + Num(Salida(w), Escala));
                }
                finally { Cerrar(w); }
            });

            Assert.Equal(
                new[]
                {
                    "carga AnnotationScale=1.23456 c00=61.23456 c10=60.00044",
                    "AnnotationScaleBox=2.5 AnnotationScale=2.5 c00=61.23456",
                    "CellPalletHeightBox texto=61.2350 c00=61.23456",
                    "CellPalletHeightBox=70.5 c00=70.5",
                    "seleccion(1,0) caja=60 c00=70.5 c10=60.00044",
                    "seleccion(0,0) caja=70.5 c00=70.5 c10=60.00044 AnnotationScale=2.5",
                },
                log);
        }

        /// <summary>La escala y las dos celdas de la salida actual: <c>AnnotationScale=… c00=… c10=…</c> (o sin c10).</summary>
        private static string Estado(RackPushBackSystemWindow w, bool conC10 = true)
        {
            var salida = Salida(w);
            return "AnnotationScale=" + Num(salida, Escala) + " c00=" + Num(salida, Altura00) + (conC10 ? " c10=" + Num(salida, Altura10) : string.Empty);
        }
    }
}
