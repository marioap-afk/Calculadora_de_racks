using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.Application.RackFrames;
using RackCad.Domain.RackFrames;
using RackCad.UI.RackFrames;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-40, SEGUNDA RONDA del Owner — las regresiones de los cuatro defectos que rechazaron el candidato
    /// <c>e31902b</c>.
    ///
    /// <para>
    /// El diagnostico: el ciclo de estado (sesion -> Commit -> reconciliacion -> resolver -> baseline -> reseed ->
    /// persistencia) conserva la cabecera personalizada COMPLETA en los doce puntos, y se comprueba aqui. Lo que
    /// perdia la personalizacion estaba DENTRO del configurador compartido: se abre siempre en «Configuracion
    /// rapida», y en ese modo la unica forma de cambiar el alto es «Aplicar», que **regenera la cabecera desde la
    /// plantilla** y conserva solo alto/fondo/poste/peralte/nombre. Todo lo demas —PanelClear, horizontales,
    /// paneles, excepciones— vuelve al estandar. De ahi «la altura funciona pero la cabecera queda predeterminada»,
    /// y de ahi que copiar esa cabecera propagara una configuracion estandar a todas.
    /// </para>
    /// </summary>
    public sealed class PushBackHeaderOwnerRoundTwoTests
    {
        private static ComboBox Combo(RackPushBackSystemWindow w, string name) => (ComboBox)w.FindName(name);
        private static Button Btn(RackPushBackSystemWindow w, string name) => (Button)w.FindName(name);
        private static CheckBox Check(RackPushBackSystemWindow w, string name) => (CheckBox)w.FindName(name);
        private static TextBlock Text(RackPushBackSystemWindow w, string name) => (TextBlock)w.FindName(name);

        private static void Click(Button b) => b.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

        private static RackPushBackSystemWindow Advanced()
        {
            var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
            w.LoadNew();
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

        private static RackFrameConfiguration Resolved(RackPushBackSystemWindow w, string moduleId)
            => PushBackHeaderTestSupport.Drawn(w, moduleId);

        /// <summary>El gesto del modo rapido: cambiar el alto y «Aplicar». REGENERA la cabecera desde la plantilla.</summary>
        private static Action<RackFrameConfiguratorWindow> QuickConfigAt(double height)
            => win =>
            {
                win.ViewModel.SimpleHeightText = height.ToString(CultureInfo.InvariantCulture);
                win.ViewModel.ApplySimpleConfiguration();
                win.Close();
            };

        /// <summary>Una personalizacion INCREMENTAL, la que el editor avanzado permite: se toca una propiedad y el
        /// resto de la cabecera se queda como estaba.</summary>
        private static Action<RackFrameConfiguratorWindow> EditIncrementally(Action<RackFrameConfiguration> edit)
            => win => { edit(win.ViewModel.Configuration); win.Close(); };

        // ===== La causa raiz, aislada ==========================================================================

        /// <summary>
        /// DEFECTO 1, causa raiz. «Aplicar» del modo rapido no edita: RECONSTRUYE la cabecera desde la plantilla.
        /// El alto sobrevive —por eso la geometria cambiaba y parecia funcionar— y todo lo demas vuelve al estandar.
        /// Esta prueba fija el hecho; no es un defecto del ciclo de estado de I-40.
        /// </summary>
        [Fact]
        public void CausaRaiz_QuickConfigApply_RegeneratesFromTheTemplate_AndDropsEveryOtherCustomProperty()
        {
            StaTestRunner.Run(() =>
            {
                var configuration = new HardcodedStandardRackFrameService().CreateDefault();
                var w = new RackFrameConfiguratorWindow(configuration, canInsertInAutoCad: false);
                try
                {
                    w.ViewModel.Configuration.PanelClear = 41.5;
                    w.ViewModel.Configuration.Horizontals[0].Elevation = 33.0;

                    w.ViewModel.SimpleHeightText = "187";
                    w.ViewModel.ApplySimpleConfiguration();

                    Assert.Equal(187.0, w.ViewModel.Configuration.Height, 4);      // el alto SI llega
                    Assert.NotEqual(41.5, w.ViewModel.Configuration.PanelClear);   // y lo demas se pierde
                    Assert.NotEqual(33.0, w.ViewModel.Configuration.Horizontals[0].Elevation);
                }
                finally { w.Close(); }
            });
        }

        // ===== DEFECTO 1 — la personalizada es la autoridad al REABRIR =========================================

        /// <summary>
        /// REGRESION del defecto 1. Una cabecera YA personalizada debe reabrirse en el **editor avanzado**, para que
        /// el usuario siga editandola de forma incremental. Abrirla en «Configuracion rapida» —como hacia el
        /// candidato rechazado— pone por delante un «Aplicar» que regenera la cabecera desde la plantilla y borra
        /// en silencio todo lo que el usuario habia confirmado.
        /// </summary>
        [Fact]
        public void D1_REGRESION_ACustomHeader_ReopensInTheAdvancedEditor()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    var id = HeaderIds(w)[0];
                    Select(w, id);

                    bool? modeOnFirstOpen = null;
                    w.HeaderConfiguratorPresenter = win
                        => { modeOnFirstOpen = win.ViewModel.IsAdvancedEditor; QuickConfigAt(187.0)(win); };
                    Click(Btn(w, "ConfigureModuleHeaderButton"));
                    Click(Btn(w, "ConfirmModuleButton"));

                    // Una cabecera todavia CALCULADA se genera: el modo rapido es el correcto.
                    Assert.False(modeOnFirstOpen);

                    bool? modeOnReopen = null;
                    w.HeaderConfiguratorPresenter = win => { modeOnReopen = win.ViewModel.IsAdvancedEditor; win.Close(); };
                    Click(Btn(w, "ConfigureModuleHeaderButton"));

                    // Ya personalizada: se EDITA, no se vuelve a generar.
                    Assert.True(modeOnReopen);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>
        /// REGRESION del defecto 1, extremo a extremo: personalizar, Confirmar, recalcular, reabrir y seguir
        /// editando de forma INCREMENTAL. La segunda edicion cambia el alto y **conserva** la propiedad
        /// personalizada de la primera: eso es «continuar editando una cabecera personalizada».
        /// </summary>
        [Fact]
        public void D1_REGRESION_IncrementalEditing_KeepsThePreviousCustomProperties()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    var id = HeaderIds(w)[0];
                    Select(w, id);

                    // Primera personalizacion: alto por el modo rapido + una propiedad propia.
                    w.HeaderConfiguratorPresenter = win =>
                    {
                        win.ViewModel.SimpleHeightText = "187";
                        win.ViewModel.ApplySimpleConfiguration();
                        win.ViewModel.Configuration.PanelClear = 41.5;
                        win.Close();
                    };
                    Click(Btn(w, "ConfigureModuleHeaderButton"));
                    Click(Btn(w, "ConfirmModuleButton"));

                    Assert.Equal(187.0, Resolved(w, id).Height, 4);
                    Assert.Equal(41.5, Resolved(w, id).PanelClear, 4);

                    // Segunda pasada: solo el alto, editando la configuracion que se recibio.
                    w.HeaderConfiguratorPresenter = EditIncrementally(c => c.Height = 205.0);
                    Click(Btn(w, "ConfigureModuleHeaderButton"));
                    Click(Btn(w, "ConfirmModuleButton"));

                    Assert.Equal(205.0, Resolved(w, id).Height, 4);
                    Assert.Equal(41.5, Resolved(w, id).PanelClear, 4);   // la primera personalizacion sigue viva
                    Assert.True(PushBackHeaderTestSupport.IsCustom(w, id));
                }
                finally { w.Close(); }
            });
        }

        /// <summary>El configurador recibe TODAS las propiedades personalizadas al reabrir, no solo el alto.</summary>
        [Fact]
        public void D1_ReopeningHandsBackEveryCustomProperty_NotOnlyTheHeight()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    var id = HeaderIds(w)[0];
                    Select(w, id);

                    w.HeaderConfiguratorPresenter = win =>
                    {
                        win.ViewModel.SimpleHeightText = "187";
                        win.ViewModel.ApplySimpleConfiguration();
                        win.ViewModel.Configuration.PanelClear = 41.5;
                        win.ViewModel.Configuration.Horizontals[0].Elevation = 33.0;
                        win.Close();
                    };
                    Click(Btn(w, "ConfigureModuleHeaderButton"));
                    Click(Btn(w, "ConfirmModuleButton"));

                    RackFrameConfiguration handedBack = null;
                    w.HeaderConfiguratorPresenter = win => { handedBack = win.ViewModel.Configuration; win.Close(); };
                    Click(Btn(w, "ConfigureModuleHeaderButton"));

                    Assert.Equal(187.0, handedBack.Height, 4);
                    Assert.Equal(41.5, handedBack.PanelClear, 4);
                    Assert.Equal(33.0, handedBack.Horizontals[0].Elevation, 4);
                }
                finally { w.Close(); }
            });
        }

        // ===== DEFECTO 2 — copiar debe copiar una personalizacion REAL =========================================

        /// <summary>
        /// REGRESION del defecto 2. Solo pueden ser ORIGEN las cabeceras que tienen una configuracion
        /// personalizada. El candidato rechazado ofrecia todas, incluidas las calculadas, asi que «copiar mi
        /// configuracion» podia propagar una configuracion estandar sin decirlo.
        /// </summary>
        [Fact]
        public void D2_REGRESION_OnlyCustomHeaders_AreOfferedAsCopySource()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    var headers = HeaderIds(w);

                    // Sin ninguna personalizada, no hay origen posible y el control lo dice.
                    Select(w, headers[1]);
                    Assert.Empty(Combo(w, "CopyHeaderFromBox").Items);
                    Assert.False(Combo(w, "CopyHeaderFromBox").IsEnabled);
                    Assert.False(Btn(w, "CopyHeaderFromButton").IsEnabled);

                    // Se personaliza la primera: aparece exactamente una, y es esa.
                    Select(w, headers[0]);
                    w.HeaderConfiguratorPresenter = QuickConfigAt(187.0);
                    Click(Btn(w, "ConfigureModuleHeaderButton"));
                    Click(Btn(w, "ConfirmModuleButton"));

                    Select(w, headers[1]);
                    var sources = Combo(w, "CopyHeaderFromBox").Items.Cast<string>().ToList();
                    Assert.Single(sources);
                    Assert.Contains("Cabecera 1", sources[0]);
                    Assert.True(Btn(w, "CopyHeaderFromButton").IsEnabled);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Las etiquetas nombran la cabecera como la cuenta el usuario —«Cabecera 1», «Cabecera 2»—, no por
        /// su posicion en la secuencia de modulos, que intercala separadores y saltaba 1, 3, 6, 8.</summary>
        [Fact]
        public void D2_REGRESION_HeadersAreNamedAsTheUserCountsThem()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    var headers = HeaderIds(w);
                    Assert.True(headers.Length >= 3);

                    foreach (var id in headers)
                    {
                        Select(w, id);
                        w.HeaderConfiguratorPresenter = QuickConfigAt(187.0);
                        Click(Btn(w, "ConfigureModuleHeaderButton"));
                    }

                    Click(Btn(w, "ConfirmModuleButton"));

                    Select(w, headers[0]);
                    var sources = Combo(w, "CopyHeaderFromBox").Items.Cast<string>().ToList();

                    // La cabecera 1 esta seleccionada, asi que las demas son «Cabecera 2», «Cabecera 3», ...
                    for (var i = 1; i < headers.Length; i++)
                    {
                        Assert.Contains(sources, label => label.StartsWith("Cabecera " + (i + 1), StringComparison.Ordinal));
                    }
                }
                finally { w.Close(); }
            });
        }

        // ===== DEFECTO 3 — la secuencia EXACTA que reporto el Owner ============================================

        /// <summary>
        /// REGRESION del defecto 3, la secuencia EXACTA del reporte: Cabecera 1 personalizada -> seleccionar
        /// Cabecera 2 -> copiar de Cabecera 1 -> aplicar a todas. Todas deben terminar con la personalizacion de
        /// Cabecera 1; ninguna puede volver a calculada.
        /// </summary>
        [Fact]
        public void D3_REGRESION_TheOwnerSequence_LeavesEveryHeaderWithTheCustomConfiguration()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    var headers = HeaderIds(w);

                    // 1) Cabecera 1 personalizada, con dos marcas propias, y confirmada.
                    Select(w, headers[0]);
                    w.HeaderConfiguratorPresenter = win =>
                    {
                        win.ViewModel.SimpleHeightText = "187";
                        win.ViewModel.ApplySimpleConfiguration();
                        win.ViewModel.Configuration.PanelClear = 41.5;
                        win.Close();
                    };
                    Click(Btn(w, "ConfigureModuleHeaderButton"));
                    Click(Btn(w, "ConfirmModuleButton"));
                    Assert.Equal(187.0, Resolved(w, headers[0]).Height, 4);

                    // 2) Seleccionar Cabecera 2. 3) Copiar de Cabecera 1. 4) Alcance: todas.
                    Select(w, headers[1]);
                    Combo(w, "HeaderScopeBox").SelectedIndex = 2;   // Todas las cabeceras
                    var sources = Combo(w, "CopyHeaderFromBox");
                    var index = sources.Items.Cast<string>()
                        .Select((label, i) => new { label, i })
                        .First(item => item.label.StartsWith("Cabecera 1", StringComparison.Ordinal)).i;
                    sources.SelectedIndex = index;
                    Click(Btn(w, "CopyHeaderFromButton"));
                    Click(Btn(w, "ConfirmModuleButton"));

                    // Todas con la personalizacion de Cabecera 1; ninguna calculada.
                    foreach (var id in headers)
                    {
                        Assert.Equal(187.0, Resolved(w, id).Height, 4);
                        Assert.Equal(41.5, Resolved(w, id).PanelClear, 4);
                        Assert.True(PushBackHeaderTestSupport.IsCustom(w, id));
                    }

                    // Y al reabrir cada una, esos mismos valores.
                    foreach (var id in headers)
                    {
                        Select(w, id);
                        RackFrameConfiguration seen = null;
                        w.HeaderConfiguratorPresenter = win => { seen = win.ViewModel.Configuration; win.Close(); };
                        Click(Btn(w, "ConfigureModuleHeaderButton"));
                        Assert.Equal(187.0, seen.Height, 4);
                        Assert.Equal(41.5, seen.PanelClear, 4);
                    }
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Cancelar despues de la operacion multiple deja el rack como estaba: ninguna cabecera alterada.</summary>
        [Fact]
        public void D3_CancelAfterTheMultipleApply_LeavesNoHeaderAltered()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    var headers = HeaderIds(w);
                    Select(w, headers[0]);
                    w.HeaderConfiguratorPresenter = QuickConfigAt(187.0);
                    Click(Btn(w, "ConfigureModuleHeaderButton"));
                    Click(Btn(w, "ConfirmModuleButton"));

                    Select(w, headers[1]);
                    Combo(w, "HeaderScopeBox").SelectedIndex = 2;   // Todas las cabeceras
                    Combo(w, "CopyHeaderFromBox").SelectedIndex = 0;
                    Click(Btn(w, "CopyHeaderFromButton"));
                    Click(Btn(w, "CancelModuleButton"));

                    // Solo la primera sigue personalizada: la operacion cancelada no dejo rastro.
                    Assert.Equal(187.0, Staged(w, headers[0]).Height, 4);
                    for (var i = 1; i < headers.Length; i++)
                    {
                        Assert.False(PushBackHeaderTestSupport.IsCustom(w, headers[i]));
                    }
                }
                finally { w.Close(); }
            });
        }

        // ===== DEFECTO 4 — UX del alcance ======================================================================

        /// <summary>REGRESION del defecto 4: el alcance se dice en palabras del usuario, sin el termino
        /// arquitectonico «aplicables».</summary>
        [Fact]
        public void D4_REGRESION_TheScopeIsWordedForAUser_NotForAnArchitect()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    var options = Combo(w, "HeaderScopeBox").Items.Cast<ComboBoxItem>()
                        .Select(item => (string)item.Content).ToList();

                    Assert.Equal(
                        new[] { "Solo esta cabecera", "Esta linea de cabeceras", "Todas las cabeceras" },
                        options);
                    Assert.DoesNotContain(options, option => option.Contains("aplicable"));
                }
                finally { w.Close(); }
            });
        }

        /// <summary>REGRESION del defecto 4: la operacion dice CUANTAS cabeceras cambio, en lugar de dejar que el
        /// usuario adivine el alcance real.</summary>
        [Fact]
        public void D4_REGRESION_TheStatusReportsHowManyHeadersWereChanged()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    var headers = HeaderIds(w);
                    Select(w, headers[0]);
                    Combo(w, "HeaderScopeBox").SelectedIndex = 2;   // Todas las cabeceras
                    w.HeaderConfiguratorPresenter = QuickConfigAt(187.0);
                    Click(Btn(w, "ConfigureModuleHeaderButton"));

                    var status = Text(w, "ModuleStatusText").Text;
                    Assert.Contains(headers.Length.ToString(CultureInfo.CurrentCulture), status);
                    Assert.Contains("cabecera", status, StringComparison.OrdinalIgnoreCase);
                    // Nombra las cabeceras como el usuario las cuenta, no por su id interno de modulo.
                    Assert.DoesNotContain("M1", status, StringComparison.Ordinal);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>La pantalla separa las tres operaciones: configurar, reutilizar y alcance.</summary>
        [Fact]
        public void D4_REGRESION_TheThreeOperationsAreVisuallySeparated()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    foreach (var name in new[] { "HeaderConfigureGroupTitle", "HeaderReuseGroupTitle", "HeaderScopeGroupTitle" })
                    {
                        Assert.NotNull(w.FindName(name));
                    }
                }
                finally { w.Close(); }
            });
        }
    }
}
