using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
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
    /// CARACTERIZACION de las SEIS ventanas del arquetipo A (I-39B).
    ///
    /// <para>Fija el comportamiento observable ACTUAL de teclado, cierre, dirty, acciones, preview y foco, que hasta
    /// I-39B no tenia NINGUNA cobertura en el repositorio: la unica cobertura de ese tipo era la que I-39A escribio
    /// para su piloto del arquetipo B.</para>
    ///
    /// <para><b>Fija lo que HAY, no lo que ADR-0029 querria.</b> Varias de estas aserciones documentan
    /// incumplimientos conocidos, y estan aqui precisamente para que la iniciativa hermana que los corrija tenga que
    /// cambiarlas a proposito y no por accidente. Cada una de ellas lo dice en su comentario. Cambiar cualquiera de
    /// esos comportamientos es trabajo observable que exige validacion del Owner, y NO se hace en I-39B.</para>
    /// </summary>
    public sealed class RichEditorCharacterizationTests
    {
        // ---- construccion de las seis ventanas reales ----

        private static RackSelectiveWindow Selective() => new RackSelectiveWindow(canInsertInAutoCad: true);
        private static RackDynamicSystemWindow Dynamic() => new RackDynamicSystemWindow(canInsertInAutoCad: true);
        private static RackPushBackSystemWindow PushBack() => new RackPushBackSystemWindow(canInsertInAutoCad: true);
        private static RackCantileverWindow Cantilever() => new RackCantileverWindow(canInsertInAutoCad: true);
        private static RackFlowBedWindow FlowBed() => new RackFlowBedWindow(canInsertInAutoCad: true);
        /// <summary>El configurador exige una cabecera utilizable: una configuracion vacia no sobrevive al
        /// round-trip del store. Se construye con el mismo servicio estandar que usa el resto de la suite.</summary>
        private static RackFrameConfiguratorWindow Frame() =>
            new RackFrameConfiguratorWindow(
                new HardcodedStandardRackFrameService().CreateDefault(), canInsertInAutoCad: true);

        private static IReadOnlyList<Window> All() => new Window[]
        {
            Selective(), Dynamic(), PushBack(), Cantilever(), FlowBed(), Frame()
        };

        private static ButtonBase Default(Window w) =>
            EditorWindowTestSupport.FindAll<ButtonBase>(w).FirstOrDefault(b => b is Button button && button.IsDefault);

        private static ButtonBase Cancel(Window w) =>
            EditorWindowTestSupport.FindAll<ButtonBase>(w).FirstOrDefault(b => b is Button button && button.IsCancel);

        private static string ContentOf(ButtonBase b) => b?.Content as string;

        private static string UiSource(string relative)
        {
            var dir = new System.IO.DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !System.IO.File.Exists(System.IO.Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.True(dir != null, "no se encontro la raiz del repositorio (RackCad.sln)");
            var path = System.IO.Path.Combine(dir.FullName, "src", "RackCad.UI", relative.Replace('/', System.IO.Path.DirectorySeparatorChar));
            Assert.True(System.IO.File.Exists(path), "no existe el archivo: " + path);
            return System.IO.File.ReadAllText(path);
        }

        // ---- 1. Enter: que boton es la accion por defecto, ventana por ventana ----

        [Fact]
        public void TheDefaultActionOfEachRichEditorIsWhatItIsToday()
        {
            StaTestRunner.Run(() =>
            {
                // Solo DOS de las seis tienen accion por defecto, y en ambas es un RECALCULO, no un dibujo:
                // es decir, Enter es hoy seguro donde existe. Las otras cuatro no tienen ninguna.
                Assert.Equal("Actualizar vista (recalcular)", ContentOf(Default(Dynamic())));
                Assert.Equal("Actualizar vista", ContentOf(Default(FlowBed())));

                // El Selectivo NO tiene default DELIBERADAMENTE: Enter en cualquier caja disparaba el recalculo y
                // LoadCellEditor revertia lo recien tecleado. La ausencia esta documentada en su propio XAML.
                Assert.Null(Default(Selective()));

                // Las otras tres simplemente no lo tienen. Push Back ademas no maneja Enter en ningun sitio.
                Assert.Null(Default(PushBack()));
                Assert.Null(Default(Cantilever()));
                Assert.Null(Default(Frame()));
            });
        }

        // ---- 2. Escape: cinco cierran, una no ----

        [Fact]
        public void TheSixCloseWithEscape()
        {
            StaTestRunner.Run(() =>
            {
                // CAMBIO DELIBERADO DE I-39B, no una acomodacion de la prueba. La caracterizacion original decia que
                // Push Back era la unica SIN IsCancel y que anadirselo sin politica convertiria Escape en un descarte
                // silencioso. La politica de cierre ya existe (OnClosing + ambito declarado), asi que ahora si
                // procede: Escape cierra las seis, y en las que declaran ambito pendiente pregunta antes.
                foreach (var (window, name) in new (Window, string)[]
                         {
                             (Selective(), "Selectivo"), (Dynamic(), "Dinamico"), (PushBack(), "Push Back"),
                             (Cantilever(), "Cantilever"), (FlowBed(), "Cama"), (Frame(), "Cabecera")
                         })
                {
                    Assert.True(Cancel(window) != null, name + " deberia tener un boton IsCancel");
                    Assert.Equal("Cerrar", ContentOf(Cancel(window)));
                }
            });
        }

        // ---- 3. Caminos de cierre: no hay ninguna politica ----

        [Fact]
        public void OnlyTheEditorsWithADeclaredScopeInterceptTheirClosing()
        {
            // CAMBIO DELIBERADO DE I-39B. Antes ninguna de las seis declaraba OnClosing y por tanto ningun camino de
            // cierre atravesaba politica alguna. Ahora la declaran EXACTAMENTE las dos que tienen un ambito
            // transaccional: Push Back (ModuleSession) y Cabecera (HasUnsavedManualEdits). Las otras cuatro no
            // declaran ambito y cierran directo — ADR-0029 D8 admite «no aplicable» como valor legitimo, y no se
            // inventa un dirty global que el producto no tiene.
            const System.Reflection.BindingFlags Declared =
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly;

            foreach (var type in new[] { typeof(RackPushBackSystemWindow), typeof(RackFrameConfiguratorWindow) })
            {
                Assert.True(type.GetMethod("OnClosing", Declared) != null, type.Name + " deberia declarar OnClosing");
            }

            foreach (var type in new[]
                     {
                         typeof(RackSelectiveWindow), typeof(RackDynamicSystemWindow),
                         typeof(RackCantileverWindow), typeof(RackFlowBedWindow)
                     })
            {
                Assert.True(type.GetMethod("OnClosing", Declared) == null, type.Name + " no deberia declarar OnClosing");
            }
        }

        [Fact]
        public void ClosingARichEditorNeverMaterialisesAnything()
        {
            StaTestRunner.Run(() =>
            {
                // Esto SI cumple ADR-0029 D7 y debe seguir cumpliendolo: cerrar no inserta, no actualiza y no guarda.
                // El contrato con el Plugin es que sin peticion de insercion el DWG no se toca.
                var selective = Selective();
                selective.Close();
                Assert.False(selective.InsertRequested);

                var flowBed = FlowBed();
                flowBed.Close();
                Assert.False(flowBed.InsertRequested);

                var pushBack = PushBack();
                pushBack.Close();
                Assert.Null(pushBack.InsertionRequest);
            });
        }

        // ---- 4. Dirty: dos ambitos declarados, cuatro sin ninguno ----

        [Fact]
        public void OnlyTwoRichEditorsDeclareATransactionalScope()
        {
            StaTestRunner.Run(() =>
            {
                // Push Back declara el suyo por RackModuleEditSession. Recien abierta, nada esta pendiente.
                var pushBack = PushBack();
                Assert.NotNull(pushBack.EditorStateForTest.ModuleSession);
                Assert.False(pushBack.EditorStateForTest.ModuleSession.HasPendingChanges);

                // La Cabecera declara el suyo por el ViewModel. Recien abierta, tampoco.
                var frame = Frame();
                Assert.False(frame.ViewModel.HasUnsavedManualEdits);
            });
        }

        [Fact]
        public void BothDeclaredScopesAreConsultedWhenTheWindowCloses()
        {
            StaTestRunner.Run(() =>
            {
                // CAMBIO DELIBERADO DE I-39B: el ambito existia y el cierre no lo consultaba. Sin nada pendiente el
                // cierre sigue siendo directo y sin dialogo, que es lo que se comprueba aqui; el caso con trabajo
                // pendiente lo cubre EditorClosePolicyTests.
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

        // ---- 5. Composicion y tamano ----

        [Fact]
        public void FourRichEditorsAreComposedOverTheSharedShellAndTwoAreNot()
        {
            StaTestRunner.Run(() =>
            {
                Assert.IsType<RackEditorVisualShell>(Selective().Content);
                Assert.IsType<RackEditorVisualShell>(Dynamic().Content);
                Assert.IsType<RackEditorVisualShell>(PushBack().Content);
                Assert.IsType<RackEditorVisualShell>(Cantilever().Content);

                // INCUMPLIMIENTO CARACTERIZADO: Cama y Cabecera conservan composicion propia. Migrarlas cambia
                // tamano, fondo y tipografia, y en la Cabecera tambien su layout de paneles persistido: es
                // observable y es de la hermana.
                Assert.IsNotType<RackEditorVisualShell>(FlowBed().Content);
                Assert.IsNotType<RackEditorVisualShell>(Frame().Content);
            });
        }

        [Fact]
        public void TheFourShellEditorsShareTheArchetypeSizeContractAndTheOtherTwoDoNot()
        {
            StaTestRunner.Run(() =>
            {
                var styles = new ResourceDictionary
                {
                    Source = new Uri("/RackCad.UI;component/Themes/AppStyles.xaml", UriKind.Relative)
                };
                var width = (double)styles["ShellInitialWidth"];
                var height = (double)styles["ShellInitialHeight"];
                var minWidth = (double)styles["ShellMinWidth"];
                var minHeight = (double)styles["ShellMinHeight"];

                foreach (var window in new Window[] { Selective(), Dynamic(), PushBack(), Cantilever() })
                {
                    Assert.Equal(width, window.Width);
                    Assert.Equal(height, window.Height);
                    Assert.Equal(minWidth, window.MinWidth);
                    Assert.Equal(minHeight, window.MinHeight);
                }

                // Tamanos propios, medidos: la Cama es mas pequena y la Cabecera mas ancha con un minimo de alto
                // INFERIOR al contractual del arquetipo A.
                var flowBed = FlowBed();
                Assert.Equal(1080.0, flowBed.Width);
                Assert.Equal(640.0, flowBed.Height);
                Assert.Equal(860.0, flowBed.MinWidth);
                Assert.Equal(520.0, flowBed.MinHeight);

                var frame = Frame();
                Assert.Equal(1320.0, frame.Width);
                Assert.Equal(820.0, frame.Height);
                Assert.Equal(1160.0, frame.MinWidth);
                Assert.Equal(560.0, frame.MinHeight);
                Assert.True(frame.MinHeight < minHeight);
            });
        }

        // ---- 6. Ownership y foco ----

        [Fact]
        public void AllSixDeclareCenterOwner()
        {
            StaTestRunner.Run(() =>
            {
                foreach (var window in All())
                {
                    Assert.Equal(WindowStartupLocation.CenterOwner, window.WindowStartupLocation);
                }
            });
        }

        [Fact]
        public void FiveDeclareAnInitialFocusAndTheHeaderConfiguratorDoesNot()
        {
            StaTestRunner.Run(() =>
            {
                // Se caracteriza la DECLARACION en el XAML, no el foco efectivo: el efectivo exige mostrar la
                // ventana y activar su ambito de foco. Se lee la fuente, que es el idiom de guarda del repositorio,
                // porque asi la asercion no depende de que un Binding se haya resuelto.
                foreach (var (relative, name) in new[]
                         {
                             ("Systems/Selective/RackSelectiveWindow.xaml", "Selectivo"),
                             ("Systems/Dynamic/RackDynamicSystemWindow.xaml", "Dinamico"),
                             ("Systems/PushBack/RackPushBackSystemWindow.xaml", "Push Back"),
                             ("Systems/Cantilever/RackCantileverWindow.xaml", "Cantilever"),
                             ("Systems/FlowBed/RackFlowBedWindow.xaml", "Cama")
                         })
                {
                    Assert.True(
                        UiSource(relative).Contains("FocusManager.FocusedElement", StringComparison.Ordinal),
                        name + " deberia declarar foco inicial");
                }

                // INCUMPLIMIENTO CARACTERIZADO de ADR-0029 D9: la Cabecera no declara foco inicial, ni por
                // FocusManager ni por una llamada a Focus(). Fijarlo mueve el cursor al abrir, que es observable, y
                // por tanto es de la hermana.
                var frame = UiSource("RackFrames/RackFrameConfiguratorWindow.xaml");
                var frameCode = UiSource("RackFrames/RackFrameConfiguratorWindow.xaml.cs");
                Assert.DoesNotContain("FocusManager.FocusedElement", frame, StringComparison.Ordinal);
                Assert.DoesNotContain(".Focus()", frameCode, StringComparison.Ordinal);
            });
        }

        [Fact]
        public void NoRichEditorDeclaresAnExplicitTabOrder()
        {
            StaTestRunner.Run(() =>
            {
                // INCUMPLIMIENTO CARACTERIZADO de ADR-0029 D9: el orden de tabulacion es hoy el de declaracion en las
                // seis. Darle un orden explicito es observable.
                foreach (var window in All())
                {
                    Assert.DoesNotContain(
                        EditorWindowTestSupport.FindAll<Control>(window),
                        c => c.TabIndex != int.MaxValue);
                }
            });
        }

        // ---- 7. La infraestructura de acciones y status sigue sin consumidores productivos ----

        [Fact]
        public void NoRichEditorConsumesTheSharedActionOrStatusInfrastructure()
        {
            StaTestRunner.Run(() =>
            {
                // INCUMPLIMIENTO CARACTERIZADO de ADR-0029 D11: las cuatro ventanas sobre el shell rellenan sus
                // categorias de accion con Button crudos y ninguna usa EditorStatusPresenter. Adoptarlos cambia
                // estilos, orden y tooltips, asi que es observable y es de la hermana. Si esta prueba empieza a
                // fallar es porque alguien adopto la infraestructura: la asercion tiene que cambiar a proposito.
                foreach (var window in new Window[] { Selective(), Dynamic(), PushBack(), Cantilever() })
                {
                    Assert.Empty(EditorWindowTestSupport.FindAll<EditorStatusPresenter>(window));
                }
            });
        }
    }
}
