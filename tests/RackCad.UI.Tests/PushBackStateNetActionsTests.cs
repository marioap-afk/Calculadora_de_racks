using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.UI.Shell;
using RackCad.UI.Systems.PushBack;
using Xunit;
using static RackCad.UI.Tests.PushBackDeclaredCorrectionSupport;
using static RackCad.UI.Tests.PushBackFullEditorCharacterizationSupport;
using static RackCad.UI.Tests.PushBackStateNetSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4B PB-T1a — la red de estado de los BOTONES DE ACCION y de la CARGA de la ventana CLASICA de Push Back: si cada uno de los once
    /// botones de accion esta habilitado con el aviso de D-PB-08 visible y sin el (CT-PBT1A-12, M-02 de PB-TC) y lo que hace una carga con
    /// una tolerancia de pallet de cero (CT-PBT1A-13, D-3). Se fija lo que hay y no se corrige nada.
    ///
    /// <para>Cada metodo arma una lista de lineas y termina en UN <c>Assert.Equal</c>. Toda ventana se crea con
    /// <see cref="PushBackFullEditorCharacterizationSupport.Ventana"/>, su carga va dentro del <c>try</c> y se cierra en el
    /// <c>finally</c>; ninguna se muestra, no se usa el teclado ni el foco real, y cada metodo corre dentro de
    /// <see cref="EditorDiscardPrompt.Substitute(System.Func{string,bool})"/> con un contador de descartes que la ultima linea de cada
    /// ventana lleva, leido despues de cerrarla. Ningun boton se pulsa: solo se lee su <c>IsEnabled</c>.</para>
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackStateNetActionsTests
    {
        /// <summary>Los once botones de accion de la ventana, en el orden de la barra.</summary>
        private static readonly string[] Botones =
        {
            "RestoreButton", "BomButton", "SaveLibraryButton", "UpdateButton", "InsertLateralButton", "InsertFrontalEntradaButton",
            "InsertFrontalPosteriorButton", "InsertPlantaButton", "InsertBatchButton", "InsertButton", "CloseButton",
        };

        /// <summary>Cierra la ventana y agrega su ultima linea con el contador de descartes leido despues.</summary>
        private static void Fin(RackPushBackSystemWindow w, List<string> log, string linea, Func<int> descartes)
        {
            Cerrar(w);
            log.Add(linea + " descartes=" + descartes());
        }

        /// <summary>El aviso y los once botones: <c>aviso=&lt;Visibility&gt; Boton=&lt;IsEnabled&gt;…</c>. Primero toca el aviso: sin el, falla ahi.</summary>
        private static string Habilitados(RackPushBackSystemWindow w)
        {
            var aviso = Aviso(w).Visibility;
            return "aviso=" + aviso + " " + string.Join(" ", Botones.Select(nombre => nombre.Replace("Button", string.Empty) + "=" + Btn(w, nombre).IsEnabled));
        }

        [Fact]
        public void CT_PBT1A_12_TheElevenActionButtonsAreEnabledWithTheSideBNoticeVisibleAndWithoutIt()
        {
            var log = new List<string>();
            var descartes = 0;
            using (EditorDiscardPrompt.Substitute(_ => { descartes++; return true; }))
            {
                Sta(() =>
                {
                    var w = Ventana();
                    try
                    {
                        Cargar(w, Fixture("pb-01-base"));
                        log.Add("sin aviso " + Habilitados(w));
                        Check(w, "SideBPresentCheck").IsChecked = true;
                        log.Add("con aviso " + Habilitados(w));
                        Check(w, "SideBPresentCheck").IsChecked = false;
                        Fin(w, log, "sin aviso otra vez " + Habilitados(w), () => descartes);
                    }
                    finally { Cerrar(w); }
                });
            }

            Assert.Equal(
            new[]
            {
                "sin aviso aviso=Collapsed Restore=True Bom=True SaveLibrary=True Update=True InsertLateral=True InsertFrontalEntrada=True InsertFrontalPosterior=True InsertPlanta=True InsertBatch=True Insert=True Close=True",
                "con aviso aviso=Visible Restore=True Bom=True SaveLibrary=True Update=True InsertLateral=True InsertFrontalEntrada=True InsertFrontalPosterior=True InsertPlanta=True InsertBatch=True Insert=True Close=True",
                "sin aviso otra vez aviso=Collapsed Restore=True Bom=True SaveLibrary=True Update=True InsertLateral=True InsertFrontalEntrada=True InsertFrontalPosterior=True InsertPlanta=True InsertBatch=True Insert=True Close=True descartes=0",
            },
            log);
        }

        [Fact]
        public void CT_PBT1A_13_LoadingExistingWithAZeroPalletToleranceThrowsAndTheWindowIsClosedInTheFinally()
        {
            var log = new List<string>();
            var descartes = 0;
            using (EditorDiscardPrompt.Substitute(_ => { descartes++; return true; }))
            {
                Sta(() =>
                {
                    var w = Ventana();
                    try
                    {
                        Exception excepcion = null;
                        try
                        {
                            Cargar(w, EditInner(Fixture("pb-01-base"), inner => Poner(inner, "Structure.PalletTolerance", 0)));
                        }
                        catch (Exception error)
                        {
                            excepcion = error;
                        }

                        log.Add("LoadExisting PalletTolerance=0 tipo=" + (excepcion == null ? "<ninguna>" : excepcion.GetType().FullName)
                            + " mensaje=[" + (excepcion == null ? string.Empty : excepcion.Message) + "]");
                    }
                    finally { Cerrar(w); }

                    log.Add("ventana cerrada en el finally=" + EstaCerrada(w) + " descartes=" + descartes);
                });
            }

            Assert.Equal(
            new[]
            {
                "LoadExisting PalletTolerance=0 tipo=System.ArgumentException mensaje=[La holgura transversal debe ser mayor que cero. (Parameter 'design')]",
                "ventana cerrada en el finally=True descartes=0",
            },
            log);
        }
    }
}
