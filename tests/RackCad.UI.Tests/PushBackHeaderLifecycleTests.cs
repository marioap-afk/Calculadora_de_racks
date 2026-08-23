using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.Application.Persistence;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.PushBack;
using RackCad.UI.RackFrames;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-40, TERCERA ronda del Owner — el ciclo de vida REAL: dibujar y volver a abrir con RACKEDITAR.
    ///
    /// <para>
    /// Por que las pruebas de la ronda 2 daban un falso verde: TODAS mantenian viva la MISMA
    /// <c>RackModuleEditSession</c> y pulsaban «Confirmar» antes de mirar el resultado. El Owner no hace eso: pulsa
    /// **Actualizar** —el boton que dibuja— y despues vuelve a entrar con **RACKEDITAR**, que construye una ventana
    /// NUEVA sobre el diseno EMBEBIDO en el DWG. Ese ciclo no estaba cubierto por ninguna prueba, y es donde el
    /// trabajo se perdia.
    /// </para>
    ///
    /// <para>
    /// Estas pruebas distinguen siempre entre MISMA sesion y sesion NUEVA: la ventana se destruye y se reconstruye
    /// desde el JSON persistido, exactamente como hace <c>RackPushBackCommands</c> al ejecutar RACKEDITAR.
    /// </para>
    /// </summary>
    public sealed class PushBackHeaderLifecycleTests
    {
        private static ComboBox Combo(RackPushBackSystemWindow w, string name) => (ComboBox)w.FindName(name);
        private static Button Btn(RackPushBackSystemWindow w, string name) => (Button)w.FindName(name);
        private static CheckBox Check(RackPushBackSystemWindow w, string name) => (CheckBox)w.FindName(name);

        private static void Click(Button b) => b.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

        private static RackPushBackSystemWindow Fresh()
        {
            var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
            w.LoadNew();
            w.Show();
            Check(w, "AdvancedModulesToggle").IsChecked = true;
            return w;
        }

        /// <summary>The window RACKEDITAR builds: a NEW one, over the design embedded in the DWG.</summary>
        private static RackPushBackSystemWindow Rackeditar(PushBackDesign drawn)
        {
            var store = new RackProjectStore();
            var reloaded = store.Deserialize(store.Serialize(RackProject.ForPushBack(drawn))).PushBackDesign;

            var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
            w.LoadExisting(reloaded, "rack-i40", "R-I40");
            w.Show();
            Check(w, "AdvancedModulesToggle").IsChecked = true;
            return w;
        }

        private static string[] HeaderIds(RackPushBackSystemWindow w)
            => w.EditorStateForTest.ModuleSession.Modules.Where(m => m.IsHeader).Select(m => m.ModuleId).ToArray();

        private static void Select(RackPushBackSystemWindow w, string moduleId)
            => Combo(w, "ModuleBox").SelectedIndex = w.EditorStateForTest.ModuleSession.Modules
                .Select((m, i) => new { m.ModuleId, i }).First(x => x.ModuleId == moduleId).i;

        // I-40 (decision final): la unidad es la cabecera FISICA (PostIndex, ModuleId).
        private static RackFrameConfiguration Staged(RackPushBackSystemWindow w, string moduleId)
            => PushBackHeaderTestSupport.Staged(w, moduleId);

        /// <summary>«Dibujar»: el mismo camino RequestDraw que usan Actualizar e Insertar. Cierra la ventana y deja
        /// el diseno que se embebe en el DWG.</summary>
        private static PushBackDesign Draw(RackPushBackSystemWindow w)
        {
            Click(Btn(w, "InsertButton"));
            return w.DesignToInsert;
        }

        /// <summary>Una personalizacion inequivoca en tres ejes: alto, PanelClear y una horizontal.</summary>
        private static Action<RackFrameConfiguratorWindow> Customize(double height, double panelClear, double elevation)
            => win =>
            {
                win.ViewModel.SimpleHeightText = height.ToString(CultureInfo.InvariantCulture);
                win.ViewModel.ApplySimpleConfiguration();
                win.ViewModel.Configuration.PanelClear = panelClear;
                win.ViewModel.Configuration.Horizontals[0].Elevation = elevation;
                win.Close();
            };

        private static void AssertCustom(RackFrameConfiguration configuration, double height, double panelClear, double elevation)
        {
            Assert.NotNull(configuration);
            Assert.Equal(height, configuration.Height, 4);
            Assert.Equal(panelClear, configuration.PanelClear, 4);
            Assert.Equal(elevation, configuration.Horizontals[0].Elevation, 4);
        }

        // ===== CASO A del Owner: dibujar sin haber pulsado «Confirmar» =========================================

        /// <summary>
        /// REGRESION del caso A. El Owner personaliza, copia y pulsa **Actualizar** SIN pulsar «Confirmar»: lo que
        /// tiene delante en el panel es su personalizacion, asi que eso es lo que debe dibujarse.
        /// <para>
        /// Contra el candidato <c>3669adc</c>, <c>RequestDraw</c> recalculaba SIN confirmar la sesion de modulos, de
        /// modo que las ediciones escenificadas se descartaban en silencio y el rack se dibujaba —y se EMBEBIA— con
        /// la cabecera estandar. Medido: la sesion tenia 187/41.5 y el diseno dibujado salia con 192/44 y
        /// procedencia calculada.
        /// </para>
        /// </summary>
        [Fact]
        public void L1_REGRESION_Drawing_AppliesTheStagedModuleEdits_InsteadOfDiscardingThem()
        {
            StaTestRunner.Run(() =>
            {
                var w = Fresh();
                var headers = HeaderIds(w);

                Select(w, headers[0]);
                w.HeaderConfiguratorPresenter = Customize(187.0, 41.5, 33.0);
                Click(Btn(w, "ConfigureModuleHeaderButton"));

                Select(w, headers[1]);
                Combo(w, "CopyHeaderFromBox").SelectedIndex = 0;
                Click(Btn(w, "CopyHeaderFromButton"));

                Assert.True(w.EditorStateForTest.ModuleSession.HasPendingChanges);

                var drawn = Draw(w);   // Actualizar / Insertar, SIN «Confirmar»

                foreach (var id in headers.Take(2))
                {
                    // La personalizacion viaja como configuracion de la LINEA editada, que es la unidad de edicion.
                    var line = drawn.Structure.HeaderLineOverrides.First(o => o.ModuleId == id);
                    AssertCustom(line.Header, 187.0, 41.5, 33.0);
                }
            });
        }

        // ===== CASO B del Owner: volver a entrar con RACKEDITAR ================================================

        /// <summary>
        /// REGRESION del caso B, el ciclo completo: personalizar, dibujar, DESTRUIR el editor y volver a abrir con
        /// RACKEDITAR. La cabecera debe seguir siendo personalizada Y llevar sus valores reales — nunca la
        /// procedencia «personalizada» con los datos predeterminados.
        /// </summary>
        [Fact]
        public void L2_REGRESION_AfterRackeditar_TheCustomHeaderKeepsItsRealConfiguration()
        {
            StaTestRunner.Run(() =>
            {
                var a = Fresh();
                var headers = HeaderIds(a);
                Select(a, headers[0]);
                a.HeaderConfiguratorPresenter = Customize(187.0, 41.5, 33.0);
                Click(Btn(a, "ConfigureModuleHeaderButton"));
                var drawn = Draw(a);                       // la sesion A muere aqui

                var b = Rackeditar(drawn);                 // sesion NUEVA, desde el DWG
                Assert.True(PushBackHeaderTestSupport.IsCustom(b, headers[0]));
                AssertCustom(Staged(b, headers[0]), 187.0, 41.5, 33.0);
                AssertCustom(PushBackHeaderTestSupport.Drawn(b, headers[0]), 187.0, 41.5, 33.0);

                // El configurador recibe esos mismos valores...
                Select(b, headers[0]);
                RackFrameConfiguration seen = null;
                b.HeaderConfiguratorPresenter = win => { seen = win.ViewModel.Configuration; win.Close(); };
                Click(Btn(b, "ConfigureModuleHeaderButton"));
                AssertCustom(seen, 187.0, 41.5, 33.0);

                // ...y sigue siendo un origen valido de «Copiar de».
                Select(b, headers[1]);
                var sources = Combo(b, "CopyHeaderFromBox").Items.Cast<string>().ToList();
                Assert.Single(sources);
                Assert.StartsWith("Cabecera 1", sources[0], StringComparison.Ordinal);
            });
        }

        /// <summary>
        /// El ciclo DOBLE que pidio el Owner: personalizar C1, dibujar, reabrir, copiar C1 a C2, dibujar otra vez,
        /// reabrir otra vez. Las dos conservan exactamente su personalizacion.
        /// </summary>
        [Fact]
        public void L3_TwoRackeditarCyclesInARow_KeepEveryCustomConfiguration()
        {
            StaTestRunner.Run(() =>
            {
                var a = Fresh();
                var headers = HeaderIds(a);
                Select(a, headers[0]);
                a.HeaderConfiguratorPresenter = Customize(187.0, 41.5, 33.0);
                Click(Btn(a, "ConfigureModuleHeaderButton"));
                var firstDrawn = Draw(a);

                var b = Rackeditar(firstDrawn);
                Select(b, headers[1]);
                Combo(b, "CopyHeaderFromBox").SelectedIndex = 0;
                Click(Btn(b, "CopyHeaderFromButton"));
                var secondDrawn = Draw(b);

                var c = Rackeditar(secondDrawn);
                foreach (var id in headers.Take(2))
                {
                    Assert.True(PushBackHeaderTestSupport.IsCustom(c, id));
                    AssertCustom(Staged(c, id), 187.0, 41.5, 33.0);
                }
            });
        }

        /// <summary>Tras reabrir, aplicar a TODAS desde la personalizada no devuelve ninguna al estandar.</summary>
        [Fact]
        public void L4_AfterRackeditar_ApplyingToAll_SendsNoHeaderBackToTheStandard()
        {
            StaTestRunner.Run(() =>
            {
                var a = Fresh();
                var headers = HeaderIds(a);
                Select(a, headers[0]);
                a.HeaderConfiguratorPresenter = Customize(187.0, 41.5, 33.0);
                Click(Btn(a, "ConfigureModuleHeaderButton"));
                var drawn = Draw(a);

                var b = Rackeditar(drawn);
                Select(b, headers[1]);
                PushBackHeaderTestSupport.Target(b, HeaderIds(b), PushBackHeaderTestSupport.Lines(b));
                Combo(b, "CopyHeaderFromBox").SelectedIndex = 0;
                Click(Btn(b, "CopyHeaderFromButton"));
                var again = Draw(b);

                var c = Rackeditar(again);
                foreach (var id in HeaderIds(c))
                {
                    Assert.True(PushBackHeaderTestSupport.IsCustom(c, id));
                    AssertCustom(Staged(c, id), 187.0, 41.5, 33.0);
                }
            });
        }

        /// <summary>La transaccion sigue siendo transaccion: lo CANCELADO no se dibuja ni se embebe.</summary>
        [Fact]
        public void L5_Cancelar_IsStillTransactional_EvenWhenDrawingRightAfterwards()
        {
            StaTestRunner.Run(() =>
            {
                var w = Fresh();
                var headers = HeaderIds(w);
                Select(w, headers[0]);
                w.HeaderConfiguratorPresenter = Customize(187.0, 41.5, 33.0);
                Click(Btn(w, "ConfigureModuleHeaderButton"));
                Click(Btn(w, "CancelModuleButton"));

                var drawn = Draw(w);

                Assert.Empty(drawn.Structure.HeaderLineOverrides);
                foreach (var id in headers)
                {
                    var module = drawn.Structure.Modules.First(m => m.ModuleId == id);
                    Assert.True(module.UseCalculatedHeaderConfiguration);
                }
            });
        }

        /// <summary>La geometria, el BOM y el GUID siguen coherentes en el ciclo completo.</summary>
        [Fact]
        public void L6_TheDrawnGeometryBomAndGuid_StayCoherentAcrossTheCycle()
        {
            StaTestRunner.Run(() =>
            {
                var a = Fresh();
                var headers = HeaderIds(a);
                Select(a, headers[0]);
                a.HeaderConfiguratorPresenter = Customize(187.0, 41.5, 33.0);
                Click(Btn(a, "ConfigureModuleHeaderButton"));
                var drawn = Draw(a);

                var b = Rackeditar(drawn);
                Assert.Equal("rack-i40", b.RackId);

                // El sistema resuelto que alimenta dibujo y BOM lleva la cabecera personalizada.
                AssertCustom(PushBackHeaderTestSupport.Drawn(b, headers[0]), 187.0, 41.5, 33.0);
            });
        }
    }
}
