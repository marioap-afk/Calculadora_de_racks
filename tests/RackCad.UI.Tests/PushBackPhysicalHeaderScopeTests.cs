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
    /// I-40 — DECISION FINAL del Owner sobre la semantica de los tres alcances.
    ///
    /// <para>
    /// La unidad de edicion de una cabecera es la INSTANCIA FISICA, identificada por <c>(PostIndex, ModuleId)</c>:
    /// </para>
    /// <code>
    ///   una cabecera fisica  ⊂  una linea fisica  ⊂  todo el rack
    /// </code>
    /// <list type="bullet">
    /// <item><b>Solo esta cabecera</b> = un unico <c>(PostIndex, ModuleId)</c>.</item>
    /// <item><b>Esta linea de cabeceras</b> = todas las cabeceras de ese <c>PostIndex</c>.</item>
    /// <item><b>Todas las cabeceras</b> = todas las lineas.</item>
    /// </list>
    /// <para>
    /// «Solo esta cabecera» dejo de significar «el modulo en todas sus lineas»: aquella lectura era consecuencia de
    /// que el modelo no tenia dimension por linea, y ahora <c>DynamicHeaderLineOverride</c> la tiene.
    /// </para>
    /// </summary>
    public sealed class PushBackPhysicalHeaderScopeTests
    {
        private const int SoloEstaCabecera = 0;
        private const int EstaLinea = 1;
        private const int TodasLasCabeceras = 2;

        private static ComboBox Combo(RackPushBackSystemWindow w, string name) => (ComboBox)w.FindName(name);
        private static Button Btn(RackPushBackSystemWindow w, string name) => (Button)w.FindName(name);
        private static CheckBox Check(RackPushBackSystemWindow w, string name) => (CheckBox)w.FindName(name);

        private static void Click(Button b) => b.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

        /// <summary>A rack with TWO fronts: three physical lines (postes 0, 1 y 2) and several cabeceras.</summary>
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
            w.LoadExisting(reloaded, "rack-fisica", "R-FISICA");
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

        /// <summary>Point the editor at one physical cabecera: its module, its line and the scope.</summary>
        private static void Address(RackPushBackSystemWindow w, string moduleId, int lineIndex, int scope)
        {
            Select(w, moduleId);
            Combo(w, "HeaderScopeBox").SelectedIndex = scope;
            if (scope != TodasLasCabeceras)
            {
                Combo(w, "HeaderLineBox").SelectedIndex = lineIndex;
            }
        }

        private static Action<RackFrameConfiguratorWindow> Customize(double height, double panelClear)
            => win =>
            {
                win.ViewModel.SimpleHeightText = height.ToString(CultureInfo.InvariantCulture);
                win.ViewModel.ApplySimpleConfiguration();
                win.ViewModel.Configuration.PanelClear = panelClear;
                win.Close();
            };

        /// <summary>What the physical cabecera (line, module) actually draws.</summary>
        private static RackFrameConfiguration Drawn(RackPushBackSystemWindow w, string moduleId, int line)
            => PushBackHeaderTestSupport.Drawn(w, moduleId, line);

        // ===== A — SOLO ESTA CABECERA =========================================================================

        /// <summary>
        /// A. Personalizar (Linea 1, Modulo A) deja la Linea 2 del MISMO modulo intacta — y la diferencia sobrevive
        /// a Actualizar y a RACKEDITAR. Es el corazon de la decision: la instancia fisica es la unidad.
        /// </summary>
        [Fact]
        public void A_OnlyThisHeader_ChangesOneInstance_AndTheSameModuleOnAnotherLineIsUntouched()
        {
            StaTestRunner.Run(() =>
            {
                var a = Fresh();
                var moduleA = HeaderIds(a)[0];

                Address(a, moduleA, lineIndex: 0, scope: SoloEstaCabecera);
                var originalOnLine2 = Drawn(a, moduleA, 1).Height;

                a.HeaderConfiguratorPresenter = Customize(187.0, 41.5);
                Click(Btn(a, "ConfigureModuleHeaderButton"));
                var drawn = Draw(a);

                var b = Rackeditar(drawn);
                try
                {
                    Assert.Equal(187.0, Drawn(b, moduleA, 0).Height, 4);
                    Assert.Equal(41.5, Drawn(b, moduleA, 0).PanelClear, 4);
                    Assert.Equal(originalOnLine2, Drawn(b, moduleA, 1).Height, 4);
                }
                finally { b.Close(); }
            });
        }

        /// <summary>Y tampoco toca a las OTRAS cabeceras de su propia linea.</summary>
        [Fact]
        public void A_OnlyThisHeader_LeavesTheOtherHeadersOfItsOwnLineUntouched()
        {
            StaTestRunner.Run(() =>
            {
                var w = Fresh();
                try
                {
                    var headers = HeaderIds(w);
                    Address(w, headers[0], lineIndex: 0, scope: SoloEstaCabecera);
                    var originalB = Drawn(w, headers[1], 0).Height;

                    w.HeaderConfiguratorPresenter = Customize(187.0, 41.5);
                    Click(Btn(w, "ConfigureModuleHeaderButton"));
                    Click(Btn(w, "ConfirmModuleButton"));

                    Assert.Equal(187.0, Drawn(w, headers[0], 0).Height, 4);
                    Assert.Equal(originalB, Drawn(w, headers[1], 0).Height, 4);
                }
                finally { w.Close(); }
            });
        }

        // ===== B — ESTA LINEA =================================================================================

        /// <summary>B. La linea 1 recibe A y B; la linea 2 se queda como estaba.</summary>
        [Fact]
        public void B_ThisLine_ChangesEveryHeaderOfThatLineOnly()
        {
            StaTestRunner.Run(() =>
            {
                var w = Fresh();
                try
                {
                    var headers = HeaderIds(w);
                    var originalsOnLine2 = headers.ToDictionary(id => id, id => Drawn(w, id, 1).Height);

                    Address(w, headers[0], lineIndex: 0, scope: EstaLinea);
                    w.HeaderConfiguratorPresenter = Customize(187.0, 41.5);
                    Click(Btn(w, "ConfigureModuleHeaderButton"));
                    Click(Btn(w, "ConfirmModuleButton"));

                    foreach (var id in headers)
                    {
                        Assert.Equal(187.0, Drawn(w, id, 0).Height, 4);
                        Assert.Equal(originalsOnLine2[id], Drawn(w, id, 1).Height, 4);
                    }
                }
                finally { w.Close(); }
            });
        }

        // ===== C — TODAS ======================================================================================

        /// <summary>C. «Todas» alcanza a todas las lineas, y las personalizaciones de linea anteriores no
        /// sobreviven para seguir mostrando valores viejos.</summary>
        [Fact]
        public void C_All_ReachesEveryLine_AndOldLineOverridesDoNotSurvive()
        {
            StaTestRunner.Run(() =>
            {
                var w = Fresh();
                try
                {
                    var headers = HeaderIds(w);

                    // Primero una personalizacion por linea que luego debe quedar superada.
                    Address(w, headers[0], lineIndex: 0, scope: EstaLinea);
                    w.HeaderConfiguratorPresenter = Customize(187.0, 41.5);
                    Click(Btn(w, "ConfigureModuleHeaderButton"));
                    Click(Btn(w, "ConfirmModuleButton"));

                    Address(w, headers[0], lineIndex: 0, scope: TodasLasCabeceras);
                    w.HeaderConfiguratorPresenter = Customize(203.0, 44.0);
                    Click(Btn(w, "ConfigureModuleHeaderButton"));
                    Click(Btn(w, "ConfirmModuleButton"));

                    Assert.Empty(w.EditorStateForTest.ModuleSession.OverriddenLines);
                    foreach (var id in headers)
                    {
                        Assert.Equal(203.0, Drawn(w, id, 0).Height, 4);
                        Assert.Equal(203.0, Drawn(w, id, 1).Height, 4);
                        Assert.Equal(203.0, Drawn(w, id, 2).Height, 4);
                    }
                }
                finally { w.Close(); }
            });
        }

        // ===== D — CAMBIO POSTERIOR ===========================================================================

        /// <summary>D. Despues de «Todas», tocar UNA instancia cambia solo esa.</summary>
        [Fact]
        public void D_AfterAll_ChangingOneInstance_ChangesOnlyThatInstance()
        {
            StaTestRunner.Run(() =>
            {
                var w = Fresh();
                try
                {
                    var headers = HeaderIds(w);

                    Address(w, headers[0], lineIndex: 0, scope: TodasLasCabeceras);
                    w.HeaderConfiguratorPresenter = Customize(203.0, 44.0);
                    Click(Btn(w, "ConfigureModuleHeaderButton"));
                    Click(Btn(w, "ConfirmModuleButton"));

                    // Solo (Linea 2, Modulo A).
                    Address(w, headers[0], lineIndex: 1, scope: SoloEstaCabecera);
                    w.HeaderConfiguratorPresenter = Customize(221.0, 45.5);
                    Click(Btn(w, "ConfigureModuleHeaderButton"));
                    Click(Btn(w, "ConfirmModuleButton"));

                    Assert.Equal(221.0, Drawn(w, headers[0], 1).Height, 4);
                    Assert.Equal(203.0, Drawn(w, headers[0], 0).Height, 4);
                    Assert.Equal(203.0, Drawn(w, headers[0], 2).Height, 4);
                    Assert.Equal(203.0, Drawn(w, headers[1], 1).Height, 4);
                }
                finally { w.Close(); }
            });
        }

        // ===== E — COPIAR =====================================================================================

        /// <summary>E. Copiar con «Solo esta cabecera» cambia EXACTAMENTE una instancia.</summary>
        [Fact]
        public void E_CopyingWithTheInstanceScope_ChangesExactlyOneInstance()
        {
            StaTestRunner.Run(() =>
            {
                var w = Fresh();
                try
                {
                    var headers = HeaderIds(w);

                    // Origen: (Linea 1, Modulo A) personalizado.
                    Address(w, headers[0], lineIndex: 0, scope: SoloEstaCabecera);
                    w.HeaderConfiguratorPresenter = Customize(187.0, 41.5);
                    Click(Btn(w, "ConfigureModuleHeaderButton"));
                    Click(Btn(w, "ConfirmModuleButton"));

                    var originals = headers.ToDictionary(id => id, id => Drawn(w, id, 1).Height);

                    // Destino: (Linea 2, Modulo B).
                    Address(w, headers[1], lineIndex: 1, scope: SoloEstaCabecera);
                    Combo(w, "CopyHeaderFromBox").SelectedIndex = 0;
                    Click(Btn(w, "CopyHeaderFromButton"));
                    Click(Btn(w, "ConfirmModuleButton"));

                    Assert.Equal(187.0, Drawn(w, headers[1], 1).Height, 4);
                    Assert.Equal(originals[headers[0]], Drawn(w, headers[0], 1).Height, 4);
                    for (var i = 2; i < headers.Length; i++)
                    {
                        Assert.Equal(originals[headers[i]], Drawn(w, headers[i], 1).Height, 4);
                    }
                }
                finally { w.Close(); }
            });
        }

        // ===== F — CANCELAR ===================================================================================

        /// <summary>F. Cancelar una operacion sobre una sola instancia no deja ningun override nuevo.</summary>
        [Fact]
        public void F_CancelingAnInstanceOperation_LeavesNoNewOverride()
        {
            StaTestRunner.Run(() =>
            {
                var w = Fresh();
                try
                {
                    var headers = HeaderIds(w);
                    Address(w, headers[0], lineIndex: 0, scope: SoloEstaCabecera);
                    var original = Drawn(w, headers[0], 0).Height;

                    w.HeaderConfiguratorPresenter = Customize(187.0, 41.5);
                    Click(Btn(w, "ConfigureModuleHeaderButton"));
                    Click(Btn(w, "CancelModuleButton"));

                    Assert.Empty(w.EditorStateForTest.ModuleSession.OverriddenLines);
                    Assert.False(w.EditorStateForTest.ModuleSession.HasPendingChanges);
                    Assert.Equal(original, Drawn(w, headers[0], 0).Height, 4);
                }
                finally { w.Close(); }
            });
        }

        // ===== G — ROUND-TRIP =================================================================================

        /// <summary>G. Dos instancias distintas, Actualizar, RACKEDITAR y save/load: cada una conserva la suya.</summary>
        [Fact]
        public void G_TwoDifferentInstances_SurviveDrawingRackeditarAndSaveLoad()
        {
            StaTestRunner.Run(() =>
            {
                var a = Fresh();
                var headers = HeaderIds(a);

                Address(a, headers[0], lineIndex: 0, scope: SoloEstaCabecera);
                a.HeaderConfiguratorPresenter = Customize(187.0, 41.5);
                Click(Btn(a, "ConfigureModuleHeaderButton"));

                Address(a, headers[0], lineIndex: 1, scope: SoloEstaCabecera);
                a.HeaderConfiguratorPresenter = Customize(221.0, 45.5);
                Click(Btn(a, "ConfigureModuleHeaderButton"));

                var drawn = Draw(a);

                var b = Rackeditar(drawn);
                try
                {
                    Assert.Equal(187.0, Drawn(b, headers[0], 0).Height, 4);
                    Assert.Equal(221.0, Drawn(b, headers[0], 1).Height, 4);

                    // Y una segunda vuelta completa por el DWG.
                    var again = Rackeditar(Draw(b));
                    try
                    {
                        Assert.Equal(187.0, Drawn(again, headers[0], 0).Height, 4);
                        Assert.Equal(221.0, Drawn(again, headers[0], 1).Height, 4);
                        Assert.Equal("rack-fisica", again.RackId);
                    }
                    finally { again.Close(); }
                }
                finally { }
            });
        }
    }
}
