using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.UI.Shell;
using RackCad.UI.Systems.PushBack;
using Xunit;
using static RackCad.UI.Tests.PushBackDeclaredCorrectionSupport;
using static RackCad.UI.Tests.PushBackFullEditorCharacterizationSupport;
using static RackCad.UI.Tests.PushBackStateNetSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4B PB-T1a — la red de estado de los controles del RACK de la ventana CLASICA de Push Back (D-5 del paquete de PB-T1): lo que
    /// cada control que hoy solo vive en la caja y se relee en cada pasada (<c>ReadInputs</c>) escribe en la salida persistida.
    /// CT-PBT1A-01 fija las tres casillas de anotacion, el nivel de cotas y las tres casillas de cotas, y CT-PBT1A-02 la unidad de peso y
    /// los fondos de la caja. Se fija lo que hay y no se corrige nada.
    ///
    /// <para>Cada metodo arma una lista de lineas y termina en UN <c>Assert.Equal</c>. Toda ventana se crea con
    /// <see cref="PushBackFullEditorCharacterizationSupport.Ventana"/>, su carga va dentro del <c>try</c> y se cierra en el
    /// <c>finally</c>; ninguna se muestra, no se usa el teclado ni el foco real, y cada metodo corre dentro de
    /// <see cref="EditorDiscardPrompt.Substitute(System.Func{string,bool})"/> con un contador de descartes que la ultima linea de cada
    /// ventana lleva, leido despues de cerrarla.</para>
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackStateNetRackControlsTests
    {
        private const string Base = "pb-01-base";

        /// <summary>Las anotaciones y las cotas de la salida: las tres casillas, el nivel de cotas y la politica por tipo de vista.</summary>
        private static string Anotaciones(string salida)
            => "NumberFronts=" + Leer(salida, "Structure.NumberFronts")
               + " NumberLevels=" + Leer(salida, "Structure.NumberLevels")
               + " DrawRackName=" + Leer(salida, "Structure.DrawRackName")
               + " Dimensions=" + Leer(salida, "Structure.Dimensions")
               + " DimensionViews=" + Leer(salida, "Structure.DimensionViews");

        /// <summary>Un clic del usuario en una casilla que escucha <c>Click</c>: invierte la casilla y eleva el clic real.</summary>
        private static void Pulsar(CheckBox casilla)
        {
            casilla.IsChecked = !(casilla.IsChecked == true);
            casilla.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, casilla));
        }

        /// <summary>Cierra la ventana y agrega su ultima linea con el contador de descartes leido despues.</summary>
        private static void Fin(RackPushBackSystemWindow w, List<string> log, string linea, Func<int> descartes)
        {
            Cerrar(w);
            log.Add(linea + " descartes=" + descartes());
        }

        [Fact]
        public void CT_PBT1A_01_TheAnnotationBoxesTheDimensionLevelAndTheDimensionViewBoxesEachReachTheOutputAlone()
        {
            var log = new List<string>();
            var descartes = 0;
            using (EditorDiscardPrompt.Substitute(_ => { descartes++; return true; }))
            {
                Sta(() =>
                {
                    // Sin tocar nada: el diseño tal cual (DimensionViews nulo sigue nulo).
                    var carga = Ventana();
                    try
                    {
                        Cargar(carga, Fixture(Base));
                        Fin(carga, log, "carga " + Anotaciones(Salida(carga)), () => descartes);
                    }
                    finally { Cerrar(carga); }

                    // Las tres casillas de anotacion, cada una sola.
                    foreach (var nombre in new[] { "NumberFrontsCheck", "NumberLevelsCheck", "DrawRackNameCheck" })
                    {
                        var w = Ventana();
                        try
                        {
                            Cargar(w, Fixture(Base));
                            Pulsar(Check(w, nombre));
                            Fin(w, log, nombre + "=" + Check(w, nombre).IsChecked + " " + Anotaciones(Salida(w)), () => descartes);
                        }
                        finally { Cerrar(w); }
                    }

                    // El nivel de cotas, cada indice solo.
                    for (var indice = 0; indice <= 3; indice++)
                    {
                        var w = Ventana();
                        try
                        {
                            Cargar(w, Fixture(Base));
                            Combo(w, "DimensionsBox").SelectedIndex = indice;
                            Fin(w, log, "DimensionsBox=" + indice + " " + Anotaciones(Salida(w)), () => descartes);
                        }
                        finally { Cerrar(w); }
                    }

                    // Las tres casillas de cotas por tipo de vista: tocar una escribe la politica de I-50.
                    foreach (var nombre in new[] { "DimensionsFrontalCheck", "DimensionsLateralCheck", "DimensionsPlantaCheck" })
                    {
                        var w = Ventana();
                        try
                        {
                            Cargar(w, Fixture(Base));
                            var antes = Check(w, nombre).IsChecked;
                            Check(w, nombre).IsChecked = !(antes == true);
                            Fin(w, log, nombre + " " + antes + "->" + Check(w, nombre).IsChecked + " " + Anotaciones(Salida(w)), () => descartes);
                        }
                        finally { Cerrar(w); }
                    }
                });
            }

            Assert.Equal(
            new[]
            {
                "carga NumberFronts=true NumberLevels=true DrawRackName=true Dimensions=2 DimensionViews=<null> descartes=0",
                "NumberFrontsCheck=False NumberFronts=false NumberLevels=true DrawRackName=true Dimensions=2 DimensionViews=<null> descartes=0",
                "NumberLevelsCheck=False NumberFronts=true NumberLevels=false DrawRackName=true Dimensions=2 DimensionViews=<null> descartes=0",
                "DrawRackNameCheck=False NumberFronts=true NumberLevels=true DrawRackName=false Dimensions=2 DimensionViews=<null> descartes=0",
                "DimensionsBox=0 NumberFronts=true NumberLevels=true DrawRackName=true Dimensions=0 DimensionViews=<null> descartes=0",
                "DimensionsBox=1 NumberFronts=true NumberLevels=true DrawRackName=true Dimensions=1 DimensionViews=<null> descartes=0",
                "DimensionsBox=2 NumberFronts=true NumberLevels=true DrawRackName=true Dimensions=2 DimensionViews=<null> descartes=0",
                "DimensionsBox=3 NumberFronts=true NumberLevels=true DrawRackName=true Dimensions=3 DimensionViews=<null> descartes=0",
                "DimensionsFrontalCheck True->False NumberFronts=true NumberLevels=true DrawRackName=true Dimensions=2 DimensionViews=6 descartes=0",
                "DimensionsLateralCheck True->False NumberFronts=true NumberLevels=true DrawRackName=true Dimensions=2 DimensionViews=5 descartes=0",
                "DimensionsPlantaCheck True->False NumberFronts=true NumberLevels=true DrawRackName=true Dimensions=2 DimensionViews=3 descartes=0",
            },
            log);
        }

        [Fact]
        public void CT_PBT1A_02_TheWeightUnitTheLoadedTonUnitAndTheDeepPalletsBoxReachTheOutput()
        {
            var log = new List<string>();
            var descartes = 0;
            using (EditorDiscardPrompt.Substitute(_ => { descartes++; return true; }))
            {
                Sta(() =>
                {
                    string Unidad(string salida) => "PalletDepth=" + Leer(salida, "Structure.PalletDepth") + " PalletWeightUnit=" + Leer(salida, "Structure.PalletWeightUnit")
                        + " PalletWeight=" + Leer(salida, "Structure.PalletWeight");

                    // «lb» elegida en la caja: sola no reconstruye la estructura, y con un cambio de fondo del pallet si.
                    var lb = Ventana();
                    try
                    {
                        Cargar(lb, Fixture(Base));
                        log.Add("carga [" + Unidad(Salida(lb)) + "]");
                        Combo(lb, "WeightUnitBox").SelectedItem = "lb";
                        log.Add("WeightUnitBox=lb [" + Unidad(Salida(lb)) + "]");
                        SetNumberR(lb, "DepthBox", 54);
                        Fin(lb, log, "DepthBox=54 [" + Unidad(Salida(lb)) + "]", () => descartes);
                    }
                    finally { Cerrar(lb); }

                    // La carga con «ton» (c-09): la caja no la ofrece; la salida y «Actualizar» la conservan hasta que algo reconstruye.
                    var ton = Ventana();
                    try
                    {
                        Cargar(ton, EditInner(Fixture(Base), inner => StructureNode(inner)["PalletWeightUnit"] = "ton"));
                        var caja = Combo(ton, "WeightUnitBox").SelectedItem;
                        var salida = Unidad(Salida(ton));
                        Click(ton, "UpdateButton");
                        Fin(ton, log, "carga ton caja=[" + (caja == null ? "<null>" : caja.ToString()) + "] salida=[" + salida + "] actualizar=[" + Unidad(Persist(ton.DesignToInsert)) + "]", () => descartes);
                    }
                    finally { Cerrar(ton); }

                    var reconstruye = Ventana();
                    try
                    {
                        Cargar(reconstruye, EditInner(Fixture(Base), inner => StructureNode(inner)["PalletWeightUnit"] = "ton"));
                        SetNumberR(reconstruye, "DepthBox", 54);
                        Fin(reconstruye, log, "carga ton y DepthBox=54 [" + Unidad(Salida(reconstruye)) + "]", () => descartes);
                    }
                    finally { Cerrar(reconstruye); }

                    // La caja de fondos editada: sola no mueve la salida, y fija el fondo del frente que se anade.
                    var fondos = Ventana();
                    try
                    {
                        Cargar(fondos, Fixture(Base));
                        SetNumberR(fondos, "PalletsDeepBox", 9);
                        var editada = Salida(fondos);
                        log.Add("PalletsDeepBox=9 PalletsDeep=" + Leer(editada, "Structure.PalletsDeep") + " Fronts[0].PalletsDeep=" + Leer(editada, "Structure.Fronts[0].PalletsDeep"));
                        Click(fondos, "AddFrontButton");
                        var anadido = Salida(fondos);
                        Fin(fondos, log, "+ frente PalletsDeep=" + Leer(anadido, "Structure.PalletsDeep") + " Fronts=" + Cuenta(anadido, "Structure.Fronts")
                            + " Fronts[3].PalletsDeep=" + Leer(anadido, "Structure.Fronts[3].PalletsDeep"), () => descartes);
                    }
                    finally { Cerrar(fondos); }
                });
            }

            Assert.Equal(
            new[]
            {
                "carga [PalletDepth=48 PalletWeightUnit=\"kg\" PalletWeight=1000]",
                "WeightUnitBox=lb [PalletDepth=48 PalletWeightUnit=\"kg\" PalletWeight=1000]",
                "DepthBox=54 [PalletDepth=54 PalletWeightUnit=\"lb\" PalletWeight=1000] descartes=0",
                "carga ton caja=[kg] salida=[PalletDepth=48 PalletWeightUnit=\"ton\" PalletWeight=1000] actualizar=[PalletDepth=48 PalletWeightUnit=\"ton\" PalletWeight=1000] descartes=0",
                "carga ton y DepthBox=54 [PalletDepth=54 PalletWeightUnit=\"kg\" PalletWeight=1000] descartes=0",
                "PalletsDeepBox=9 PalletsDeep=8 Fronts[0].PalletsDeep=8",
                "+ frente PalletsDeep=8 Fronts=4 Fronts[3].PalletsDeep=8 descartes=0",
            },
            log);
        }
    }
}
