using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Linq;
using RackCad.Application.RackFrames;
using RackCad.UI.RackFrames;
using RackCad.UI.Shell;
using RackCad.UI.Systems.Cantilever;
using RackCad.UI.Systems.Dynamic;
using RackCad.UI.Systems.FlowBed;
using RackCad.UI.Systems.PushBack;
using RackCad.UI.Systems.Selective;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// EL CONTRATO NUEVO de cierre de los editores ricos (I-39B), deliberadamente separado de la caracterización de
    /// la base.
    ///
    /// <para>Cada prueba de aquí tiene su gemela en <c>RichEditorCharacterizationTests</c>, conservada con
    /// <c>Skip</c> como evidencia de qué hacía el código antes. La transición se lee entera:
    /// <c>b61182f</c> caracterizó la base → ADR-0029 D7 y D8 autorizaron el cambio → esto prueba el resultado.
    /// El enfrentamiento aserción por aserción está en
    /// <c>docs/automation/evidence/I-39B-caracterizacion-base-vs-contrato.md</c>.</para>
    /// </summary>
    public sealed class RichEditorCloseContractTests
    {
        private static RackSelectiveWindow Selective() => SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
        private static RackDynamicSystemWindow Dynamic() => new RackDynamicSystemWindow(canInsertInAutoCad: true);
        private static RackPushBackSystemWindow PushBack() => new RackPushBackSystemWindow(canInsertInAutoCad: true);
        private static RackCantileverWindow Cantilever() => new RackCantileverWindow(canInsertInAutoCad: true);
        private static RackFlowBedWindow FlowBed() => new RackFlowBedWindow(canInsertInAutoCad: true);
        private static RackFrameConfiguratorWindow Frame() =>
            new RackFrameConfiguratorWindow(new HardcodedStandardRackFrameService().CreateDefault(), canInsertInAutoCad: true);

        private static ButtonBase Cancel(Window w) =>
            EditorWindowTestSupport.FindAll<ButtonBase>(w).FirstOrDefault(b => b is Button button && button.IsCancel);

        private const BindingFlags Declared =
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly;

        /// <summary>Reemplaza a <c>FiveOfTheSixCloseWithEscapeAndPushBackDoesNot</c>. ADR-0029 D7.</summary>
        [Fact]
        public void TheSixCloseWithEscape()
        {
            StaTestRunner.Run(() =>
            {
                // Push Back era la unica SIN IsCancel. Se le anadio DESPUES de que existiera la politica de cierre:
                // al reves habria convertido Escape en un descarte instantaneo en la unica ventana con ambito
                // declarado.
                foreach (var (window, name) in new (Window, string)[]
                         {
                             (Selective(), "Selectivo"), (Dynamic(), "Dinamico"), (PushBack(), "Push Back"),
                             (Cantilever(), "Cantilever"), (FlowBed(), "Cama"), (Frame(), "Cabecera")
                         })
                {
                    Assert.True(Cancel(window) != null, name + " deberia tener un boton IsCancel");
                    Assert.Equal("Cerrar", (string)Cancel(window).Content);
                }
            });
        }

        /// <summary>Reemplaza a <c>NoRichEditorInterceptsItsClosing</c>. ADR-0029 D7 y D8.</summary>
        [Fact]
        public void OnlyTheEditorsWithADeclaredScopeInterceptTheirClosing()
        {
            foreach (var type in new[] { typeof(RackPushBackSystemWindow), typeof(RackFrameConfiguratorWindow) })
            {
                Assert.True(type.GetMethod("OnClosing", Declared) != null, type.Name + " deberia declarar OnClosing");
            }

            // Las otras cuatro no declaran ambito transaccional y cierran directo: D8 admite «no aplicable» como
            // valor legitimo, y no se inventa un dirty global que el producto no tiene.
            foreach (var type in new[]
                     {
                         typeof(RackSelectiveWindow), typeof(RackDynamicSystemWindow),
                         typeof(RackCantileverWindow), typeof(RackFlowBedWindow)
                     })
            {
                Assert.True(type.GetMethod("OnClosing", Declared) == null, type.Name + " no deberia declarar OnClosing");
            }
        }

        /// <summary>Reemplaza a <c>NeitherDeclaredScopeIsConsultedWhenTheWindowCloses</c>. ADR-0029 D8.</summary>
        [Fact]
        public void ClosingWithoutPendingWorkStillAsksNothing()
        {
            StaTestRunner.Run(() =>
            {
                var asked = false;
                using (EditorDiscardPrompt.Substitute(_ => { asked = true; return true; }))
                {
                    var pushBack = PushBack();
                    pushBack.Close();
                    Assert.Null(pushBack.InsertionRequest);

                    var frame = Frame();
                    frame.Close();
                    Assert.False(frame.InsertRequested);
                }

                Assert.False(asked, "sin trabajo pendiente el cierre no debe preguntar nada");
            });
        }

        [Fact]
        public void TheFourEditorsWithoutScopeCloseWithoutAsking()
        {
            StaTestRunner.Run(() =>
            {
                var asked = false;
                using (EditorDiscardPrompt.Substitute(_ => { asked = true; return true; }))
                {
                    Selective().Close();
                    Dynamic().Close();
                    Cantilever().Close();
                    FlowBed().Close();
                }

                Assert.False(asked, "las cuatro sin ambito declarado cierran directo, como antes de I-39B");
            });
        }

        [Fact]
        public void NoCloseRouteMaterialisesAnything()
        {
            StaTestRunner.Run(() =>
            {
                // Esto ya se cumplia antes de I-39B y debe seguir cumpliendose: ninguna ruta de cierre inserta,
                // actualiza ni guarda.
                using (EditorDiscardPrompt.Substitute(_ => true))
                {
                    var selective = Selective();
                    selective.Close();
                    Assert.False(selective.InsertRequested);

                    var pushBack = PushBack();
                    pushBack.Close();
                    Assert.Null(pushBack.InsertionRequest);

                    var frame = Frame();
                    frame.Close();
                    Assert.False(frame.InsertRequested);
                }
            });
        }
    }
}
