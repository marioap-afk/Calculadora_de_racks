using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.PushBack;
using RackCad.UI.Controls;
using RackCad.UI.RackFrames;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-40, QUINTA ronda del Owner — los once casos que la validacion manual dejo escritos.
    ///
    /// <para>
    /// El modelo de destinos son DOS EJES INDEPENDIENTES —cabeceras y lineas— cuyo PRODUCTO CARTESIANO son las
    /// cabeceras fisicas que la operacion toca. La configuracion ORIGEN es independiente de los destinos: se
    /// obtiene una vez y se aplica tantas veces como haga falta, sin volver a abrir el configurador. Ese era el
    /// defecto central: el alcance solo se evaluaba al recibir una configuracion NUEVA.
    /// </para>
    /// </summary>
    public sealed class PushBackHeaderCartesianTests
    {
        private static ComboBox Combo(RackPushBackSystemWindow w, string name) => (ComboBox)w.FindName(name);
        private static Button Btn(RackPushBackSystemWindow w, string name) => (Button)w.FindName(name);
        private static CheckBox Check(RackPushBackSystemWindow w, string name) => (CheckBox)w.FindName(name);
        private static NumericField Field(RackPushBackSystemWindow w, string name) => (NumericField)w.FindName(name);

        private static void Click(Button b) => b.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

        /// <summary>A rack with THREE fronts: four physical lines and several cabeceras.</summary>
        private static RackPushBackSystemWindow Fresh()
        {
            var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
            w.LoadNew();
            w.Show();
            Click(Btn(w, "AddFrontButton"));
            Click(Btn(w, "AddFrontButton"));
            Check(w, "AdvancedModulesToggle").IsChecked = true;
            return w;
        }

        private static RackPushBackSystemWindow Rackeditar(PushBackDesign drawn)
        {
            var store = new RackProjectStore();
            var reloaded = store.Deserialize(store.Serialize(RackProject.ForPushBack(drawn))).PushBackDesign;
            var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
            w.LoadExisting(reloaded, "rack-cartesiano", "R-CART");
            w.Show();
            Check(w, "AdvancedModulesToggle").IsChecked = true;
            return w;
        }

        private static PushBackDesign Draw(RackPushBackSystemWindow w)
        {
            Click(Btn(w, "InsertButton"));
            return w.DesignToInsert;
        }

        private static string[] Headers(RackPushBackSystemWindow w)
            => PushBackHeaderTestSupport.Headers(w).ToArray();

        private static int[] Lines(RackPushBackSystemWindow w)
            => PushBackHeaderTestSupport.Lines(w).ToArray();

        private static void Select(RackPushBackSystemWindow w, string moduleId)
            => Combo(w, "ModuleBox").SelectedIndex = w.EditorStateForTest.ModuleSession.Modules
                .Select((m, i) => new { m.ModuleId, i }).First(x => x.ModuleId == moduleId).i;

        private static void Target(RackPushBackSystemWindow w, string[] moduleIds, int[] lines)
            => PushBackHeaderTestSupport.Target(w, moduleIds, lines);

        private static void ApplyToSelection(RackPushBackSystemWindow w)
            => PushBackHeaderTestSupport.ApplyToSelection(w);

        private static double Drawn(RackPushBackSystemWindow w, string moduleId, int line)
            => PushBackHeaderTestSupport.Drawn(w, moduleId, line).Height;

        private static Action<RackFrameConfiguratorWindow> Customize(double height, double panelClear)
            => win =>
            {
                win.ViewModel.SimpleHeightText = height.ToString(CultureInfo.InvariantCulture);
                win.ViewModel.ApplySimpleConfiguration();
                win.ViewModel.Configuration.PanelClear = panelClear;
                win.Close();
            };

        /// <summary>Configure the selected cabecera; the result becomes the ORIGIN configuration.</summary>
        private static void Configure(RackPushBackSystemWindow w, double height, double panelClear = 41.5)
        {
            w.HeaderConfiguratorPresenter = Customize(height, panelClear);
            Click(Btn(w, "ConfigureModuleHeaderButton"));
        }

        // ===== CASO 1 — una instancia =========================================================================

        [Fact]
        public void Caso1_ConfigureC1_ApplyToLine1_ChangesOnlyThatInstance()
        {
            StaTestRunner.Run(() =>
            {
                var a = Fresh();
                var headers = Headers(a);
                var lines = Lines(a);

                Select(a, headers[0]);
                Target(a, new[] { headers[0] }, new[] { lines[0] });
                var originalOnLine2 = Drawn(a, headers[0], lines[1]);
                Configure(a, 187.0);
                var drawn = Draw(a);

                var b = Rackeditar(drawn);
                try
                {
                    Assert.Equal(187.0, Drawn(b, headers[0], lines[0]), 4);
                    Assert.Equal(originalOnLine2, Drawn(b, headers[0], lines[1]), 4);
                    Assert.Equal(originalOnLine2, Drawn(b, headers[1], lines[0]), 4);
                }
                finally { b.Close(); }
            });
        }

        // ===== CASO 2 — el defecto central: cambiar destinos SIN reconfigurar =================================

        /// <summary>
        /// CASO 2 del Owner. Sin volver a abrir «Configurar cabecera...», se cambia la seleccion a todas las lineas
        /// y se pulsa «Aplicar configuracion a la seleccion»: la cabecera cambia en TODAS. Contra el candidato
        /// <c>73325cf</c> esto no hacia nada — el alcance solo se evaluaba al recibir una configuracion nueva.
        /// </summary>
        [Fact]
        public void Caso2_REGRESION_ChangingTheDestinationsWithoutReconfiguring_IsAnOperation()
        {
            StaTestRunner.Run(() =>
            {
                var w = Fresh();
                try
                {
                    var headers = Headers(w);
                    var lines = Lines(w);

                    Select(w, headers[0]);
                    Target(w, new[] { headers[0] }, new[] { lines[0] });
                    Configure(w, 187.0);

                    // NO se vuelve a configurar: solo cambia la seleccion y se aplica.
                    Target(w, new[] { headers[0] }, lines);
                    ApplyToSelection(w);
                    Click(Btn(w, "ConfirmModuleButton"));

                    foreach (var line in lines)
                    {
                        Assert.Equal(187.0, Drawn(w, headers[0], line), 4);
                    }
                }
                finally { w.Close(); }
            });
        }

        // ===== CASO 3 — producto cartesiano ===================================================================

        [Fact]
        public void Caso3_TwoHeadersByTwoLines_ChangesExactlyThoseFourInstances()
        {
            StaTestRunner.Run(() =>
            {
                var w = Fresh();
                try
                {
                    var headers = Headers(w);
                    var lines = Lines(w);
                    Assert.True(headers.Length >= 3 && lines.Length >= 3);

                    Select(w, headers[0]);
                    Target(w, new[] { headers[0] }, new[] { lines[0] });
                    Configure(w, 187.0);

                    var untouched = Drawn(w, headers[1], lines[2]);
                    Target(w, new[] { headers[0], headers[2] }, new[] { lines[0], lines[1] });
                    ApplyToSelection(w);
                    Click(Btn(w, "ConfirmModuleButton"));

                    foreach (var id in new[] { headers[0], headers[2] })
                    {
                        Assert.Equal(187.0, Drawn(w, id, lines[0]), 4);
                        Assert.Equal(187.0, Drawn(w, id, lines[1]), 4);
                        Assert.NotEqual(187.0, Drawn(w, id, lines[2]));
                    }

                    Assert.Equal(untouched, Drawn(w, headers[1], lines[2]), 4);
                    Assert.NotEqual(187.0, Drawn(w, headers[1], lines[0]));
                }
                finally { w.Close(); }
            });
        }

        // ===== CASO 4 — todas las cabeceras x una linea =======================================================

        [Fact]
        public void Caso4_AllHeadersByOneLine_ChangesOnlyThatLine()
        {
            StaTestRunner.Run(() =>
            {
                var w = Fresh();
                try
                {
                    var headers = Headers(w);
                    var lines = Lines(w);

                    Select(w, headers[0]);
                    Target(w, new[] { headers[0] }, new[] { lines[0] });
                    Configure(w, 187.0);

                    Target(w, headers, new[] { lines[1] });
                    ApplyToSelection(w);
                    Click(Btn(w, "ConfirmModuleButton"));

                    foreach (var id in headers)
                    {
                        Assert.Equal(187.0, Drawn(w, id, lines[1]), 4);
                        Assert.NotEqual(187.0, Drawn(w, id, lines[2]));
                    }
                }
                finally { w.Close(); }
            });
        }

        // ===== CASO 5 — todas x todas =========================================================================

        [Fact]
        public void Caso5_AllHeadersByAllLines_ChangesTheWholeRack()
        {
            StaTestRunner.Run(() =>
            {
                var w = Fresh();
                try
                {
                    var headers = Headers(w);
                    var lines = Lines(w);

                    Select(w, headers[0]);
                    Configure(w, 187.0);

                    Click(Btn(w, "HeaderTargetsAllButton"));
                    Click(Btn(w, "HeaderLinesAllButton"));
                    ApplyToSelection(w);
                    Click(Btn(w, "ConfirmModuleButton"));

                    foreach (var id in headers)
                    {
                        foreach (var line in lines)
                        {
                            Assert.Equal(187.0, Drawn(w, id, line), 4);
                        }
                    }
                }
                finally { w.Close(); }
            });
        }

        // ===== CASO 6 — poste derivado por linea ==============================================================

        [Fact]
        public void Caso6_DerivedPostHeight_OnSelectedLinesOnly()
        {
            StaTestRunner.Run(() =>
            {
                var a = Fresh();
                var lines = Lines(a);

                Target(a, new[] { Headers(a)[0] }, new[] { lines[0], lines[1] });
                var box = Field(a, "DerivedPostLineHeightBox");
                box.SetNumber(137.0);
                Click(Btn(a, "ApplyDerivedPostLinesButton"));
                var drawn = Draw(a);

                var b = Rackeditar(drawn);
                try
                {
                    var overrides = b.EditorStateForTest.WorkingBaseline.Structure.DerivedPostLineOverrides;
                    Assert.Equal(2, overrides.Count);
                    Assert.All(overrides, item => Assert.Equal(137.0, item.Height, 4));
                    Assert.Equal(
                        new[] { lines[0], lines[1] },
                        overrides.Select(item => item.PostIndex).OrderBy(index => index).ToArray());

                    // Y la autoridad unica lo confirma linea por linea.
                    Assert.Equal(137.0, RackCad.Application.Systems.Dynamic.DynamicFrontGeometry
                        .DerivedPostHeightAtPost(b.EditorStateForTest.WorkingBaseline.Structure, lines[0], 999.0), 4);
                    Assert.Equal(999.0, RackCad.Application.Systems.Dynamic.DynamicFrontGeometry
                        .DerivedPostHeightAtPost(b.EditorStateForTest.WorkingBaseline.Structure, lines[2], 999.0), 4);
                }
                finally { b.Close(); }
            });
        }

        // ===== CASOS 7 y 8 — frontal y posterior ==============================================================

        /// <summary>
        /// CASOS 7 y 8. Personalizar una linea cambia SU corte lateral y, coherentemente, la vista FRONTAL — que
        /// dibuja el poste de cada linea. Contra el candidato anterior la frontal no cambiaba nunca: leia
        /// <c>PostHeight</c>, un valor derivado que ignora la configuracion de la cabecera.
        /// </summary>
        [Fact]
        public void Caso7y8_REGRESION_ALineCustomization_ReachesTheFrontalAndTheRearViews()
        {
            StaTestRunner.Run(() =>
            {
                var w = Fresh();
                try
                {
                    var headers = Headers(w);
                    var lines = Lines(w);

                    Select(w, headers[0]);
                    Target(w, headers, new[] { lines[0] });

                    var frontalBefore = Signature(w, PushBackFrontalEnd.EntradaSalida);
                    var posteriorBefore = Signature(w, PushBackFrontalEnd.Posterior);

                    Configure(w, 251.0);
                    Click(Btn(w, "ConfirmModuleButton"));

                    // La frontal CAMBIA: es la vista que dibuja el poste de esa linea.
                    Assert.NotEqual(frontalBefore, Signature(w, PushBackFrontalEnd.EntradaSalida));
                    Assert.NotEqual(posteriorBefore, Signature(w, PushBackFrontalEnd.Posterior));
                }
                finally { w.Close(); }
            });
        }

        /// <summary>La frontal usa la MISMA autoridad que el lateral: la altura de la cabecera de esa linea.</summary>
        [Fact]
        public void Caso7y8_TheFrontalReadsTheSameAuthorityAsTheLateral()
        {
            StaTestRunner.Run(() =>
            {
                var w = Fresh();
                try
                {
                    var headers = Headers(w);
                    var lines = Lines(w);
                    Select(w, headers[0]);
                    Target(w, headers, new[] { lines[0] });
                    Configure(w, 251.0);
                    Click(Btn(w, "ConfirmModuleButton"));

                    var structure = w.EditorStateForTest.WorkingBaseline.Structure;
                    Assert.Equal(
                        251.0,
                        RackCad.Application.Systems.Dynamic.DynamicFrontGeometry
                            .HeaderHeightAtPost(structure, null, lines[0]),
                        4);
                    Assert.NotEqual(
                        251.0,
                        RackCad.Application.Systems.Dynamic.DynamicFrontGeometry
                            .HeaderHeightAtPost(structure, null, lines[2]));
                }
                finally { w.Close(); }
            });
        }

        // ===== CASO 10 — round-trip ===========================================================================

        [Fact]
        public void Caso10_TheConfirmedSelectionsSurviveDrawingAndRackeditar()
        {
            StaTestRunner.Run(() =>
            {
                var a = Fresh();
                var headers = Headers(a);
                var lines = Lines(a);

                Select(a, headers[0]);
                Target(a, new[] { headers[0] }, new[] { lines[0] });
                Configure(a, 187.0);

                Target(a, new[] { headers[1] }, new[] { lines[2] });
                ApplyToSelection(a);
                var drawn = Draw(a);

                var b = Rackeditar(drawn);
                try
                {
                    Assert.Equal(187.0, Drawn(b, headers[0], lines[0]), 4);
                    Assert.Equal(187.0, Drawn(b, headers[1], lines[2]), 4);
                    Assert.NotEqual(187.0, Drawn(b, headers[1], lines[0]));
                    Assert.Equal("rack-cartesiano", b.RackId);
                }
                finally { b.Close(); }
            });
        }

        // ===== CASO 11 — cancelar =============================================================================

        [Fact]
        public void Caso11_CancelBeforeConfirmar_PersistsNothing()
        {
            StaTestRunner.Run(() =>
            {
                var w = Fresh();
                try
                {
                    var headers = Headers(w);
                    var lines = Lines(w);
                    Select(w, headers[0]);
                    Target(w, headers, lines);
                    var original = Drawn(w, headers[0], lines[0]);

                    Configure(w, 187.0);
                    Field(w, "DerivedPostLineHeightBox").SetNumber(137.0);
                    Click(Btn(w, "ApplyDerivedPostLinesButton"));
                    Click(Btn(w, "CancelModuleButton"));

                    Assert.Empty(w.EditorStateForTest.ModuleSession.OverriddenLines);
                    Assert.Empty(w.EditorStateForTest.ModuleSession.DerivedPostLines);
                    Assert.False(w.EditorStateForTest.ModuleSession.HasPendingChanges);
                    Assert.Equal(original, Drawn(w, headers[0], lines[0]), 4);
                }
                finally { w.Close(); }
            });
        }

        /// <summary>The frontal (or rear) plan the CURRENT baseline produces, through the real builder.</summary>
        private static string Signature(RackPushBackSystemWindow w, PushBackFrontalEnd end)
        {
            var system = w.EditorStateForTest.WorkingBaseline;
            var plan = system == null
                ? null
                : new RackCad.Application.Systems.PushBack.PushBackSystemFrontalBuilder()
                    .BuildPlan(system, RackCad.Application.Catalogs.JsonRackCatalogProvider.FromBaseDirectory().Load(), end);
            return plan == null
                ? string.Empty
                : string.Join("|", plan.Flatten().Instances.Select(instance => string.Join(
                    ",",
                    instance.Role,
                    instance.PieceId,
                    string.Join(";", instance.DynamicParameters.OrderBy(p => p.Key)
                        .Select(p => p.Key + "=" + p.Value.ToString("0.###"))))));
        }
    }
}
