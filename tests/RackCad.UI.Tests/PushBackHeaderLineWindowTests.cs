using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.PushBack;
using RackCad.UI.Controls;
using RackCad.UI.RackFrames;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-40, cuarta entrega del Owner — la LINEA DE CABECERAS y la ALTURA DEL POSTE DERIVADO en la ventana real,
    /// atravesando el ciclo completo: configurar, dibujar y volver a entrar con RACKEDITAR.
    /// </summary>
    public sealed class PushBackHeaderLineWindowTests
    {
        private static ComboBox Combo(RackPushBackSystemWindow w, string name) => (ComboBox)w.FindName(name);
        private static Button Btn(RackPushBackSystemWindow w, string name) => (Button)w.FindName(name);
        private static CheckBox Check(RackPushBackSystemWindow w, string name) => (CheckBox)w.FindName(name);
        private static NumericField Field(RackPushBackSystemWindow w, string name) => (NumericField)w.FindName(name);

        private static void Click(Button b) => b.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

        /// <summary>A shown rack with TWO fronts, so it has three physical lines to tell apart.</summary>
        private static RackPushBackSystemWindow Fresh()
        {
            var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
            w.LoadNew();
            w.Show();
            Click(Btn(w, "AddFrontButton"));
            Check(w, "AdvancedModulesToggle").IsChecked = true;
            return w;
        }

        private static RackPushBackSystemWindow Rackeditar(PushBackDesign drawn)
        {
            var store = new RackProjectStore();
            var reloaded = store.Deserialize(store.Serialize(RackProject.ForPushBack(drawn))).PushBackDesign;
            var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
            w.LoadExisting(reloaded, "rack-linea", "R-LINEA");
            w.Show();
            Check(w, "AdvancedModulesToggle").IsChecked = true;
            return w;
        }

        private static PushBackDesign Draw(RackPushBackSystemWindow w)
        {
            Click(Btn(w, "InsertButton"));
            return w.DesignToInsert;
        }

        private static string[] HeaderIds(RackPushBackSystemWindow w)
            => w.EditorStateForTest.ModuleSession.Modules.Where(m => m.IsHeader).Select(m => m.ModuleId).ToArray();

        private static void Select(RackPushBackSystemWindow w, string moduleId)
            => Combo(w, "ModuleBox").SelectedIndex = w.EditorStateForTest.ModuleSession.Modules
                .Select((m, i) => new { m.ModuleId, i }).First(x => x.ModuleId == moduleId).i;

        private static Action<RackFrameConfiguratorWindow> Customize(double height, double panelClear)
            => win =>
            {
                win.ViewModel.SimpleHeightText = height.ToString(CultureInfo.InvariantCulture);
                win.ViewModel.ApplySimpleConfiguration();
                win.ViewModel.Configuration.PanelClear = panelClear;
                win.Close();
            };

        /// <summary>What a LINE actually draws for a module, read through the single authority.</summary>
        private static RackFrameConfiguration OnLine(RackPushBackSystemWindow w, string moduleId, int line)
        {
            var structure = w.EditorStateForTest.WorkingBaseline.Structure;
            var module = structure.Modules.First(m => m.ModuleId == moduleId);
            return DynamicFrontGeometry.HeaderConfigurationAtPost(structure, module, null, line);
        }

        // ===== La UX del alcance ===============================================================================

        /// <summary>Los tres alcances, dichos en palabras del usuario y en orden de menor a mayor.</summary>
        [Fact]
        public void TheThreeScopes_AreOfferedInOrder()
        {
            StaTestRunner.Run(() =>
            {
                var w = Fresh();
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

        /// <summary>La linea se ELIGE, y las que se ofrecen son las que el rack tiene de verdad.</summary>
        [Fact]
        public void TheLineSelector_ListsTheLinesTheRackActuallyHas()
        {
            StaTestRunner.Run(() =>
            {
                var w = Fresh();
                try
                {
                    Combo(w, "HeaderScopeBox").SelectedIndex = 1;   // Esta linea de cabeceras
                    var lines = Combo(w, "HeaderLineBox").Items.Cast<string>().ToList();

                    Assert.Equal(3, lines.Count);                  // dos frentes ⇒ tres lineas de postes
                    Assert.StartsWith("Linea 1", lines[0], StringComparison.Ordinal);
                    Assert.True(Combo(w, "HeaderLineBox").IsEnabled);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Fuera del alcance por linea el selector no se usa, y dice por que.</summary>
        [Fact]
        public void TheLineSelector_IsDisabledOutsideTheLineScope()
        {
            StaTestRunner.Run(() =>
            {
                var w = Fresh();
                try
                {
                    Combo(w, "HeaderScopeBox").SelectedIndex = 0;
                    Assert.False(Combo(w, "HeaderLineBox").IsEnabled);
                }
                finally { w.Close(); }
            });
        }

        // ===== El E2E que pidio el Owner =======================================================================

        /// <summary>
        /// Linea A con su propia configuracion, linea B intacta; dibujar; volver con RACKEDITAR; las diferencias
        /// siguen ahi. Despues «Todas las cabeceras» y las dos lineas reciben la misma.
        /// </summary>
        [Fact]
        public void E2E_OneLineChanges_TheOtherDoesNot_AndItSurvivesRackeditar()
        {
            StaTestRunner.Run(() =>
            {
                var a = Fresh();
                var headers = HeaderIds(a);
                Select(a, headers[0]);

                // Linea A = linea 1 (poste 0).
                Combo(a, "HeaderScopeBox").SelectedIndex = 1;
                Combo(a, "HeaderLineBox").SelectedIndex = 0;
                a.HeaderConfiguratorPresenter = Customize(187.0, 41.5);
                Click(Btn(a, "ConfigureModuleHeaderButton"));

                // Y la altura del poste derivado, que es del rack entero.
                var derived = Field(a, "DerivedPostHeightBox");
                derived.SetNumber(137.0);
                derived.RaiseEvent(new RoutedEventArgs(FrameworkElement.LostFocusEvent));

                var drawn = Draw(a);

                var b = Rackeditar(drawn);
                try
                {
                    // Linea A cambio; linea B (poste 1) no.
                    foreach (var id in HeaderIds(b))
                    {
                        Assert.Equal(187.0, OnLine(b, id, 0).Height, 4);
                        Assert.Equal(41.5, OnLine(b, id, 0).PanelClear, 4);
                        Assert.NotEqual(187.0, OnLine(b, id, 1).Height);
                    }

                    // Y el poste derivado conserva su altura propia.
                    Assert.Equal(137.0, b.EditorStateForTest.WorkingBaseline.Structure.DerivedPostHeight);

                    // Ahora TODAS: las dos lineas quedan iguales.
                    var headersB = HeaderIds(b);
                    Select(b, headersB[0]);
                    Combo(b, "HeaderScopeBox").SelectedIndex = 2;
                    b.HeaderConfiguratorPresenter = Customize(203.0, 44.0);
                    Click(Btn(b, "ConfigureModuleHeaderButton"));
                    Click(Btn(b, "ConfirmModuleButton"));

                    foreach (var id in headersB)
                    {
                        Assert.Equal(203.0, OnLine(b, id, 0).Height, 4);
                        Assert.Equal(203.0, OnLine(b, id, 1).Height, 4);
                    }
                }
                finally { b.Close(); }
            });
        }

        /// <summary>Copiar de otra cabecera funciona con el alcance por LINEA: el origen es la personalizacion real
        /// y el destino es esa linea, no el rack entero.</summary>
        [Fact]
        public void CopyingIntoALine_UsesTheRealSourceAndTouchesOnlyThatLine()
        {
            StaTestRunner.Run(() =>
            {
                var w = Fresh();
                try
                {
                    var headers = HeaderIds(w);

                    // Cabecera 1 personalizada (a nivel de modulo, para que sea un origen valido).
                    Select(w, headers[0]);
                    w.HeaderConfiguratorPresenter = Customize(187.0, 41.5);
                    Click(Btn(w, "ConfigureModuleHeaderButton"));
                    Click(Btn(w, "ConfirmModuleButton"));

                    // Se copia a la LINEA 2 (poste 1).
                    Select(w, headers[1]);
                    Combo(w, "HeaderScopeBox").SelectedIndex = 1;
                    Combo(w, "HeaderLineBox").SelectedIndex = 1;
                    Combo(w, "CopyHeaderFromBox").SelectedIndex = 0;
                    Click(Btn(w, "CopyHeaderFromButton"));
                    Click(Btn(w, "ConfirmModuleButton"));

                    foreach (var id in HeaderIds(w))
                    {
                        Assert.Equal(187.0, OnLine(w, id, 1).Height, 4);
                    }
                }
                finally { w.Close(); }
            });
        }

        /// <summary>Cancelar revierte tambien la operacion por linea.</summary>
        [Fact]
        public void CancelingALineOperation_LeavesNoLineChanged()
        {
            StaTestRunner.Run(() =>
            {
                var w = Fresh();
                try
                {
                    var headers = HeaderIds(w);
                    Select(w, headers[0]);
                    Combo(w, "HeaderScopeBox").SelectedIndex = 1;
                    Combo(w, "HeaderLineBox").SelectedIndex = 0;
                    w.HeaderConfiguratorPresenter = Customize(187.0, 41.5);
                    Click(Btn(w, "ConfigureModuleHeaderButton"));
                    Click(Btn(w, "CancelModuleButton"));

                    Assert.Empty(w.EditorStateForTest.ModuleSession.OverriddenLines);
                    Assert.False(w.EditorStateForTest.ModuleSession.HasPendingChanges);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>La altura del poste derivado sobrevive el ciclo completo y el GUID no cambia.</summary>
        [Fact]
        public void TheDerivedPostHeight_SurvivesTheWholeCycle()
        {
            StaTestRunner.Run(() =>
            {
                var a = Fresh();
                var derived = Field(a, "DerivedPostHeightBox");
                derived.SetNumber(137.0);
                derived.RaiseEvent(new RoutedEventArgs(FrameworkElement.LostFocusEvent));
                var drawn = Draw(a);

                var b = Rackeditar(drawn);
                try
                {
                    Assert.Equal(137.0, Field(b, "DerivedPostHeightBox").Value);
                    Assert.Equal(137.0, b.EditorStateForTest.WorkingBaseline.Structure.DerivedPostHeight);
                    Assert.Equal("rack-linea", b.RackId);
                }
                finally { b.Close(); }
            });
        }
    }
}
