using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.RackFrames;
using RackCad.UI.RackFrames;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-40 — la FRONTERA donde la cabecera personalizada de Push Back se perdia, y las dos operaciones nuevas
    /// sobre ella. Estas pruebas conducen la ventana REAL y el configurador REAL: la sustitucion de
    /// <c>Configuration</c> que causa el defecto solo ocurre dentro de <c>RackFrameConfiguratorViewModel</c>, asi
    /// que una prueba que se quede en Application no puede verla y una que sustituya el dialogo tampoco.
    ///
    /// <para>PBH-01: la configuracion efectiva es la que el configurador deja, no la instancia que se le entrego.</para>
    /// <para>PBH-02: alcance «esta cabecera» / «todas las cabeceras aplicables», atomico y con un solo Confirmar.</para>
    /// <para>PBH-03: reutilizar otra cabecera de la sesion como COPIA independiente.</para>
    /// </summary>
    public sealed class PushBackHeaderAuthorityWindowTests
    {
        private static ComboBox Combo(RackPushBackSystemWindow w, string name) => (ComboBox)w.FindName(name);
        private static Button Btn(RackPushBackSystemWindow w, string name) => (Button)w.FindName(name);
        private static CheckBox Check(RackPushBackSystemWindow w, string name) => (CheckBox)w.FindName(name);
        private static RadioButton Radio(RackPushBackSystemWindow w, string name) => (RadioButton)w.FindName(name);

        private static void Click(Button button) => button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

        /// <summary>A shown Push Back window with a valid model and the advanced module panel open.</summary>
        private static RackPushBackSystemWindow Advanced()
        {
            var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
            w.LoadNew();
            w.Show();
            Check(w, "AdvancedModulesToggle").IsChecked = true;
            return w;
        }

        private static string[] HeaderIds(RackPushBackSystemWindow w)
            => w.EditorStateForTest.ModuleSession.Modules
                .Where(module => module.IsHeader)
                .Select(module => module.ModuleId)
                .ToArray();

        /// <summary>Select a module by id in the real ComboBox, through the real SelectionChanged handler.</summary>
        private static void Select(RackPushBackSystemWindow w, string moduleId)
        {
            var index = w.EditorStateForTest.ModuleSession.Modules
                .Select((module, i) => new { module.ModuleId, i })
                .First(item => item.ModuleId == moduleId).i;
            Combo(w, "ModuleBox").SelectedIndex = index;
        }

        private static RackFrameConfiguration Staged(RackPushBackSystemWindow w, string moduleId)
            => w.EditorStateForTest.ModuleSession.HeaderConfigurationCopy(moduleId);

        /// <summary>
        /// The REAL user gesture that the defect hid: the quick-configuration tab of the shared configurator, where
        /// «Aplicar» regenerates the cabecera at the height the user typed. That call REPLACES the ViewModel's
        /// <c>Configuration</c> with a fresh clone, so the instance Push Back handed in stays at the old height.
        /// </summary>
        private static Action<RackFrameConfiguratorWindow> QuickConfigAt(double height)
            => window =>
            {
                window.ViewModel.SimpleHeightText = height.ToString(CultureInfo.InvariantCulture);
                window.ViewModel.ApplySimpleConfiguration();
                window.Close();
            };

        // ===== PBH-01 — la cabecera personalizada es la autoridad efectiva =====================================

        /// <summary>
        /// REGRESION de I-40 (PBH-01). Antes del arreglo, <c>ShowHeaderConfigurator</c> devolvia la COPIA que
        /// habia entregado, y esa copia queda obsoleta en cuanto el ViewModel reemplaza su <c>Configuration</c>:
        /// la altura personalizada del usuario no llegaba nunca a la sesion, ni al resolver, ni al dibujo.
        /// Con el arreglo, la configuracion efectiva se LEE de la ventana.
        /// </summary>
        [Fact]
        public void PBH01_REGRESION_ACustomHeightTypedInTheRealConfigurator_ReachesTheSession()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    var headerId = HeaderIds(w)[0];
                    Select(w, headerId);

                    var before = Staged(w, headerId)?.Height
                                 ?? w.EditorStateForTest.ModuleSession.Modules.First(m => m.ModuleId == headerId).Length;
                    var custom = 187.0;
                    Assert.NotEqual(custom, before);

                    w.HeaderConfiguratorPresenter = QuickConfigAt(custom);
                    Click(Btn(w, "ConfigureModuleHeaderButton"));

                    var staged = Staged(w, headerId);
                    Assert.NotNull(staged);
                    Assert.Equal(custom, staged.Height, 4);
                    Assert.True(w.EditorStateForTest.ModuleSession.HasPendingChanges);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>La altura personalizada llega hasta el sistema RESUELTO tras Confirmar: la travesia entera,
        /// desde el control real hasta la geometria.</summary>
        [Fact]
        public void PBH01_TheCustomHeight_ReachesTheResolvedSystem_AfterConfirmar()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    var headerId = HeaderIds(w)[0];
                    Select(w, headerId);

                    w.HeaderConfiguratorPresenter = QuickConfigAt(187.0);
                    Click(Btn(w, "ConfigureModuleHeaderButton"));
                    Click(Btn(w, "ConfirmModuleButton"));

                    var module = w.EditorStateForTest.WorkingBaseline.Structure.Modules
                        .First(m => m.ModuleId == headerId);
                    Assert.False(module.UseCalculatedHeaderConfiguration);
                    Assert.Equal(187.0, module.AssociatedFrameConfiguration.Height, 4);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>La PROCEDENCIA se ve en la ventana: «Personalizada» queda marcada, y el GUID del rack no
        /// cambia por personalizar una cabecera.</summary>
        [Fact]
        public void PBH01_ProvenanceShowsAsCustom_AndTheRackGuidNeverChanges()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    var identityBefore = w.RackId;
                    var headerId = HeaderIds(w)[0];
                    Select(w, headerId);

                    w.HeaderConfiguratorPresenter = QuickConfigAt(187.0);
                    Click(Btn(w, "ConfigureModuleHeaderButton"));

                    Assert.True(Radio(w, "ModuleCustomRadio").IsChecked);
                    Assert.False(Radio(w, "ModuleCalculatedRadio").IsChecked);

                    Click(Btn(w, "ConfirmModuleButton"));
                    Assert.Equal(identityBefore, w.RackId);
                }
                finally { w.Close(); }
            });
        }

        // ===== PBH-02 — alcance ================================================================================

        [Fact]
        public void PBH02_ThisHeader_IsTheDefaultScope_AndTouchesOnlyTheSelectedOne()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    var headers = HeaderIds(w);
                    Assert.True(headers.Length > 1, "el fixture necesita mas de una cabecera");
                    Select(w, headers[0]);

                    Assert.Equal(0, Combo(w, "HeaderScopeBox").SelectedIndex);

                    w.HeaderConfiguratorPresenter = QuickConfigAt(187.0);
                    Click(Btn(w, "ConfigureModuleHeaderButton"));

                    Assert.Equal(187.0, Staged(w, headers[0]).Height, 4);
                    foreach (var other in headers.Skip(1))
                    {
                        Assert.False(
                            w.EditorStateForTest.ModuleSession.Modules
                                .First(m => m.ModuleId == other).HasCustomHeaderConfiguration);
                    }
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void PBH02_AllApplicableHeaders_ReceiveIt_AndNoSeparatorDoes()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    var headers = HeaderIds(w);
                    Select(w, headers[0]);
                    Combo(w, "HeaderScopeBox").SelectedIndex = 1;   // Todas las cabeceras aplicables

                    w.HeaderConfiguratorPresenter = QuickConfigAt(187.0);
                    Click(Btn(w, "ConfigureModuleHeaderButton"));

                    foreach (var id in headers)
                    {
                        Assert.Equal(187.0, Staged(w, id).Height, 4);
                    }

                    foreach (var separator in w.EditorStateForTest.ModuleSession.Modules.Where(m => !m.IsHeader))
                    {
                        Assert.False(separator.HasCustomHeaderConfiguration);
                    }
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Una aplicacion a TODAS se confirma UNA vez y deja el rack entero coherente.</summary>
        [Fact]
        public void PBH02_ApplyingToAll_NeedsASingleConfirmar()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    var headers = HeaderIds(w);
                    Select(w, headers[0]);
                    Combo(w, "HeaderScopeBox").SelectedIndex = 1;

                    w.HeaderConfiguratorPresenter = QuickConfigAt(187.0);
                    Click(Btn(w, "ConfigureModuleHeaderButton"));
                    Click(Btn(w, "ConfirmModuleButton"));

                    Assert.False(w.EditorStateForTest.ModuleSession.HasPendingChanges);
                    foreach (var id in headers)
                    {
                        var module = w.EditorStateForTest.WorkingBaseline.Structure.Modules.First(m => m.ModuleId == id);
                        Assert.False(module.UseCalculatedHeaderConfiguration);
                        Assert.Equal(187.0, module.AssociatedFrameConfiguration.Height, 4);
                    }
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void PBH02_Cancelar_RestoresTheStateAfterApplyingToAll()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    var headers = HeaderIds(w);
                    Select(w, headers[0]);
                    Combo(w, "HeaderScopeBox").SelectedIndex = 1;

                    w.HeaderConfiguratorPresenter = QuickConfigAt(187.0);
                    Click(Btn(w, "ConfigureModuleHeaderButton"));
                    Click(Btn(w, "CancelModuleButton"));

                    Assert.False(w.EditorStateForTest.ModuleSession.HasPendingChanges);
                    foreach (var module in w.EditorStateForTest.ModuleSession.Modules)
                    {
                        Assert.False(module.HasCustomHeaderConfiguration);
                    }
                }
                finally { w.Close(); }
            });
        }

        // ===== PBH-03 — copia independiente ====================================================================

        [Fact]
        public void PBH03_CopyingAnotherHeader_CarriesItsConfiguration_AndKeepsBothIndependent()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    var headers = HeaderIds(w);

                    // Cabecera 1: personalizada a 187".
                    Select(w, headers[0]);
                    w.HeaderConfiguratorPresenter = QuickConfigAt(187.0);
                    Click(Btn(w, "ConfigureModuleHeaderButton"));

                    // Cabecera 2: copia de la 1.
                    Select(w, headers[1]);
                    var sources = Combo(w, "CopyHeaderFromBox");
                    Assert.True(sources.Items.Count > 0);
                    sources.SelectedIndex = 0;
                    Click(Btn(w, "CopyHeaderFromButton"));
                    Assert.Equal(187.0, Staged(w, headers[1]).Height, 4);

                    // Modificar la 2 no altera la 1.
                    w.HeaderConfiguratorPresenter = QuickConfigAt(211.0);
                    Click(Btn(w, "ConfigureModuleHeaderButton"));

                    Assert.Equal(211.0, Staged(w, headers[1]).Height, 4);
                    Assert.Equal(187.0, Staged(w, headers[0]).Height, 4);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>El origen nunca es la cabecera seleccionada: copiarse a si misma no es una operacion.</summary>
        [Fact]
        public void PBH03_TheCopySourceList_ExcludesTheSelectedHeader()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    var headers = HeaderIds(w);
                    var allModules = w.EditorStateForTest.ModuleSession.Modules;

                    Select(w, headers[0]);
                    var sources = Combo(w, "CopyHeaderFromBox");
                    Assert.Equal(headers.Length - 1, sources.Items.Count);
                    Assert.DoesNotContain(
                        sources.Items.Cast<string>(),
                        label => label.StartsWith(
                            (allModules.First(m => m.ModuleId == headers[0]).Index + 1).ToString(CultureInfo.CurrentCulture) + ".",
                            StringComparison.Ordinal));
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Copiar con el alcance «todas» reparte la MISMA configuracion como copias independientes.</summary>
        [Fact]
        public void PBH03_CopyingWithTheAllScope_ReachesEveryHeader()
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

                    Select(w, headers[1]);
                    Combo(w, "HeaderScopeBox").SelectedIndex = 1;
                    Combo(w, "CopyHeaderFromBox").SelectedIndex = 0;   // la primera de las OTRAS cabeceras
                    Click(Btn(w, "CopyHeaderFromButton"));

                    foreach (var id in headers)
                    {
                        Assert.Equal(187.0, Staged(w, id).Height, 4);
                    }
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Un separador no ofrece ni alcance ni copia: la seccion de cabecera entera se oculta.</summary>
        [Fact]
        public void PBH02_And_PBH03_AreInvisibleOnASeparator()
        {
            StaTestRunner.Run(() =>
            {
                var w = Advanced();
                try
                {
                    var separator = w.EditorStateForTest.ModuleSession.Modules.First(m => !m.IsHeader);
                    Select(w, separator.ModuleId);

                    Assert.Equal(Visibility.Collapsed, ((StackPanel)w.FindName("ModuleHeaderPanel")).Visibility);
                }
                finally { w.Close(); }
            });
        }
    }
}
