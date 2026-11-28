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
    /// I-64 F4B PB-T1a — la red de estado de los MODULOS y de la SEGURIDAD de la ventana CLASICA de Push Back: la altura del poste derivado
    /// en una linea, que solo llega a la salida con «Confirmar» (CT-PBT1A-06, D-5), y la observacion de CT-16 de PB-T0 sobre aceptar
    /// Seguridad sin cambios (CT-PBT1A-11). Se fija lo que hay y no se corrige nada.
    ///
    /// <para>Cada metodo arma una lista de lineas y termina en UN <c>Assert.Equal</c>. Toda ventana se crea con
    /// <see cref="PushBackFullEditorCharacterizationSupport.Ventana"/>, su carga va dentro del <c>try</c> y se cierra en el
    /// <c>finally</c>; solo la de CT-PBT1A-11 se muestra, fuera de pantalla y sin activarse (M-03); no se usa el teclado ni el foco real, y
    /// cada metodo corre dentro de <see cref="EditorDiscardPrompt.Substitute(System.Func{string,bool})"/> con un contador de descartes que
    /// la ultima linea de cada ventana lleva, leido despues de cerrarla. Seguridad se acepta por su costura, sin ventana modal.</para>
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackStateNetModuleAndSafetyTests
    {
        /// <summary>Cierra la ventana y agrega su ultima linea con el contador de descartes leido despues.</summary>
        private static void Fin(RackPushBackSystemWindow w, List<string> log, string linea, Func<int> descartes)
        {
            Cerrar(w);
            log.Add(linea + " descartes=" + descartes());
        }

        /// <summary>Las alturas de poste derivado por linea de la salida y si la sesion tiene trabajo pendiente.</summary>
        private static string Derivado(RackPushBackSystemWindow w)
            => "DerivedPostLineOverrides=" + Leer(Salida(w), "Structure.DerivedPostLineOverrides") + " pendiente=" + w.PendingWork().HasPendingWork;

        [Fact]
        public void CT_PBT1A_06_TheDerivedPostHeightOfALineReachesTheOutputOnlyAfterConfirmAndCancellingLeavesNothing()
        {
            var log = new List<string>();
            var descartes = 0;
            using (EditorDiscardPrompt.Substitute(_ => { descartes++; return true; }))
            {
                Sta(() =>
                {
                    // Aplicar y confirmar.
                    var confirmar = Ventana();
                    try
                    {
                        Cargar(confirmar, Fixture("pb-01-base"));
                        Check(confirmar, "AdvancedModulesToggle").IsChecked = true;
                        var lineas = PushBackHeaderTestSupport.Lines(confirmar);
                        log.Add("lineas=[" + string.Join(",", lineas) + "] carga " + Derivado(confirmar));
                        PushBackHeaderTestSupport.Target(confirmar, Array.Empty<string>(), new[] { lineas[1] });
                        SetNumberR(confirmar, "DerivedPostLineHeightBox", 137);
                        Click(confirmar, "ApplyDerivedPostLinesButton");
                        log.Add("linea " + lineas[1] + " DerivedPostLineHeightBox=137 Aplicar " + Derivado(confirmar));
                        Click(confirmar, "ConfirmModuleButton");
                        Fin(confirmar, log, "Confirmar " + Derivado(confirmar), () => descartes);
                    }
                    finally { Cerrar(confirmar); }

                    // Aplicar y cancelar.
                    var cancelar = Ventana();
                    try
                    {
                        Cargar(cancelar, Fixture("pb-01-base"));
                        Check(cancelar, "AdvancedModulesToggle").IsChecked = true;
                        var lineas = PushBackHeaderTestSupport.Lines(cancelar);
                        PushBackHeaderTestSupport.Target(cancelar, Array.Empty<string>(), new[] { lineas[1] });
                        SetNumberR(cancelar, "DerivedPostLineHeightBox", 137);
                        Click(cancelar, "ApplyDerivedPostLinesButton");
                        log.Add("cancelar Aplicar " + Derivado(cancelar));
                        Click(cancelar, "CancelModuleButton");
                        Fin(cancelar, log, "Cancelar " + Derivado(cancelar), () => descartes);
                    }
                    finally { Cerrar(cancelar); }
                });
            }

            Assert.Equal(
            new[]
            {
                "lineas=[0,1,2,3] carga DerivedPostLineOverrides=<null> pendiente=False",
                "linea 1 DerivedPostLineHeightBox=137 Aplicar DerivedPostLineOverrides=<null> pendiente=True",
                "Confirmar DerivedPostLineOverrides=[{\"PostIndex\":1,\"Height\":137}] pendiente=False descartes=0",
                "cancelar Aplicar DerivedPostLineOverrides=<null> pendiente=True",
                "Cancelar DerivedPostLineOverrides=<null> pendiente=False descartes=0",
            },
            log);
        }

        /// <summary>Las selecciones de seguridad de la salida y de la ventana, y el delta de toda la salida (las lineas si son pocas).</summary>
        private static string Seguridad(RackPushBackSystemWindow w)
            => "ventana=[" + string.Join(",", w.SafetySelections.Select(s => s.ElementId + "/" + s.Side + "/" + (s.AuthoredSide.HasValue ? s.AuthoredSide.Value.ToString() : "<null>"))) + "]"
               + " salida=" + Cuenta(Salida(w), "Structure.SafetySelections") + " sha256=" + Huella(Leer(Salida(w), "Structure.SafetySelections"));

        /// <summary>Las lineas del BOM de la ultima computacion: cuantas, cuantas piezas y su huella.</summary>
        private static string Bom(RackPushBackSystemWindow w)
        {
            var lineas = w.LastComputation.Bom.Lines
                .Select(l => l.Category + "|" + l.ProfileId + "|" + l.Length.ToString("R", CultureInfo.InvariantCulture) + "|" + l.Quantity.ToString(CultureInfo.InvariantCulture))
                .ToList();
            return "n=" + lineas.Count + " piezas=" + w.LastComputation.Bom.TotalPieces + " sha256=" + Sha(lineas);
        }

        [Fact]
        public void CT_PBT1A_11_AcceptingSafetyWithoutChangesOnTheTopesRackFixesTheSelectionsAndTheBomLinesBeforeAndAfter()
        {
            var log = new List<string>();
            var descartes = 0;
            using (EditorDiscardPrompt.Substitute(_ => { descartes++; return true; }))
            {
                Sta(() =>
                {
                    var t = Fixture("pb-05-topes");
                    var w = Ventana();
                    try
                    {
                        w.WindowStartupLocation = WindowStartupLocation.Manual;
                        w.ShowInTaskbar = false;
                        w.ShowActivated = false;
                        w.Left = -10000;
                        w.Top = -10000;
                        Cargar(w, t);
                        w.Show();
                        w.UpdateLayout();

                        var antes = Salida(w);
                        log.Add("antes " + Seguridad(w));
                        log.Add("antes BOM " + Bom(w));
                        var recibidas = 0;
                        w.SafetyDialog = received =>
                        {
                            recibidas = received.Count;
                            return received.Select(s => s.DeepCopy()).ToList();
                        };
                        Click(w, "SafetyButton");
                        var despues = Salida(w);
                        log.Add("recibidas=" + recibidas);
                        log.Add("despues " + Seguridad(w));
                        log.Add("despues BOM " + Bom(w));
                        Fin(w, log, "delta de la salida " + Describe(Delta(antes, despues)), () => descartes);
                    }
                    finally { Cerrar(w); }
                });
            }

            Assert.Equal(
            new[]
            {
                "antes ventana=[] salida=0 sha256=4f53cda18c2baa0c0354bb5f9a3ecbe5ed12ab4d8e11ba873c2f11161202b945",
                "antes BOM n=12 piezas=459 sha256=98ee5b4e53d9b91920edc19fed4a59603d383d82c20df9c45332ce517e097274",
                "recibidas=0",
                "despues ventana=[PROTECTOR_BOTA_H_3_16_18/None/<null>] salida=1 sha256=ef822724fdb93a4f27b8d077c933f750a748673d328ea95f2e563d41f583bba1",
                "despues BOM n=12 piezas=459 sha256=98ee5b4e53d9b91920edc19fed4a59603d383d82c20df9c45332ce517e097274",
                "delta de la salida n=29 sha256=841d2d83ea1011a8d6d00af4028fc8f2ca60acbdcee4e72f614afb77261ef190 contenedores=[$.PushBack.DefensePieceId x1; $.PushBack.Structure.SafetySelections[0] x28] descartes=0",
            },
            log);
        }
    }
}
