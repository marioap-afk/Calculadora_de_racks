using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Systems.Selective;
using RackCad.UI.Systems.Selective;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-43, gate 8.6C (ARQ-43-01 + 02 + 09 + <c>FondosBox</c>): la frontera entre lo PENDIENTE y lo COMPROMETIDO.
    /// <para>
    /// Las cuatro cajas —<c>FondosBox</c>, <c>BayCountBox</c>, <c>FondoBox</c>, <c>CabeceraFondoBox</c>— son editores
    /// de un valor pendiente; la autoridad son los slots y las matrices. Un <c>LostFocus</c> o un Enter SIN edición no
    /// muta nada (O-43-02), ningún texto pendiente llega a un slot ni a <c>BuildDesign</c> sin un commit explícito
    /// (INV-13), y cuando varios pendientes se comprometen juntos la operación es atómica (INV-16) y ordenada
    /// (INV-17): estructura antes que valores, con <c>TargetFondos</c> re-resuelto en medio.
    /// </para>
    /// </summary>
    public sealed class SelectivePendingEditorsTests
    {
        // ---- helpers: gestos reales, sin seams nuevos ----

        private static RackSelectiveWindow Open(int fondos) => SelectiveWindowTestSupport.Open(fondos);

        private static TextBox Box(RackSelectiveWindow window, string name) => (TextBox)window.FindName(name);

        /// <summary>Teclear en una caja SIN salir del campo: el texto queda pendiente, nada se compromete.</summary>
        private static void Type(RackSelectiveWindow window, string name, string text) => Box(window, name).Text = text;

        /// <summary>Salir del campo: el gesto que compromete lo tecleado.</summary>
        private static void LostFocus(RackSelectiveWindow window, string name)
        {
            var box = Box(window, name);
            box.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, box));
        }

        private static void TypeAndLeave(RackSelectiveWindow window, string name, string text)
        {
            Type(window, name, text);
            LostFocus(window, name);
        }

        private static ComboBox FondoSelector(RackSelectiveWindow window) => (ComboBox)window.FindName("FondoSelectorBox");

        /// <summary>Frentes comprometidos de cada fondo (la matriz viva para el visible, su slot para el resto).</summary>
        private static int[] Counts(RackSelectiveWindow window)
        {
            var state = window.EditorState;
            return Enumerable.Range(0, state.FondoCount)
                .Select(k => k == state.SelectedFondo ? state.Bays.Count : state.FondoMatrices[k].Bays.Count)
                .ToArray();
        }

        private static double[] Depths(RackSelectiveWindow window)
            => window.EditorState.FondoMatrices.Select(m => m.Depth).ToArray();

        private static double[] CabeceraOverrides(RackSelectiveWindow window)
            => window.EditorState.FondoMatrices.Select(m => m.CabeceraOverride).ToArray();

        /// <summary>Fija los frentes de UN fondo: seleccionarlo, apuntar el destino a él y comprometer.</summary>
        private static void SetCountOfFondo(RackSelectiveWindow window, int oneBased, int count)
        {
            FondoSelector(window).SelectedIndex = oneBased - 1;
            SelectiveTargetsTestSupport.SetTargets(window, oneBased);
            TypeAndLeave(window, "BayCountBox", count.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>Fija el fondo de tarima de UN fondo, del mismo modo.</summary>
        private static void SetDepthOfFondo(RackSelectiveWindow window, int oneBased, double depth)
        {
            FondoSelector(window).SelectedIndex = oneBased - 1;
            SelectiveTargetsTestSupport.SetTargets(window, oneBased);
            TypeAndLeave(window, "FondoBox", depth.ToString(CultureInfo.InvariantCulture));
        }

        private static void SetCabeceraOfFondo(RackSelectiveWindow window, int oneBased, string text)
        {
            FondoSelector(window).SelectedIndex = oneBased - 1;
            SelectiveTargetsTestSupport.SetTargets(window, oneBased);
            TypeAndLeave(window, "CabeceraFondoBox", text);
        }

        /// <summary>La frontera transaccional que un test puede pulsar de verdad: «Recalcular tramo».</summary>
        private static void PressRecalcular(RackSelectiveWindow window)
            => EditorWindowTestSupport.ClickByContent(window, "Recalcular tramo");

        // =========================================================================================
        // A. LostFocus / Enter / «Recalcular» SIN edición no mutan nada (O-43-02, INV-11)
        // =========================================================================================

        [Fact]
        public void A_LeavingTheFrentesFieldWithoutEditing_ChangesNothing()
        {
            var (counts, recomputes) = StaTestRunner.Run(() =>
            {
                var window = Open(3);
                SetCountOfFondo(window, 1, 3);
                SetCountOfFondo(window, 2, 1);
                SetCountOfFondo(window, 3, 3);
                FondoSelector(window).SelectedIndex = 0;
                SelectiveTargetsTestSupport.SetAllTargets(window);

                var before = window.RecomputeCount;
                LostFocus(window, "BayCountBox"); // salir del campo sin haber tecleado nada
                return (Counts(window), window.RecomputeCount - before);
            });

            Assert.Equal(new[] { 3, 1, 3 }, counts); // el fondo 2 NO fue arrastrado a 3
            Assert.Equal(0, recomputes);             // y nada se recalculó
        }

        [Fact]
        public void A_PressingEnterInTheFrentesFieldWithoutEditing_ChangesNothing()
        {
            var counts = StaTestRunner.Run(() =>
            {
                var window = Open(3);
                SetCountOfFondo(window, 1, 3);
                SetCountOfFondo(window, 2, 1);
                SetCountOfFondo(window, 3, 3);
                FondoSelector(window).SelectedIndex = 0;
                SelectiveTargetsTestSupport.SetAllTargets(window);

                var box = Box(window, "BayCountBox");
                box.RaiseEvent(new System.Windows.Input.KeyEventArgs(
                    System.Windows.Input.Keyboard.PrimaryDevice,
                    new System.Windows.Interop.HwndSource(0, 0, 0, 0, 0, "t", System.IntPtr.Zero),
                    0,
                    System.Windows.Input.Key.Enter)
                { RoutedEvent = UIElement.KeyDownEvent });
                return Counts(window);
            });

            Assert.Equal(new[] { 3, 1, 3 }, counts);
        }

        [Fact]
        public void A_RecalcularWithoutEditing_DoesNotResizeTheTargets()
        {
            var counts = StaTestRunner.Run(() =>
            {
                var window = Open(3);
                SetCountOfFondo(window, 1, 3);
                SetCountOfFondo(window, 2, 1);
                SetCountOfFondo(window, 3, 3);
                FondoSelector(window).SelectedIndex = 0;
                SelectiveTargetsTestSupport.SetAllTargets(window);

                PressRecalcular(window);
                return Counts(window);
            });

            Assert.Equal(new[] { 3, 1, 3 }, counts);
        }

        [Fact]
        public void A_LeavingTheFrentesFieldWithoutEditing_LeavesTheCabecerasOfHiddenFondosAlone()
        {
            var customs = StaTestRunner.Run(() =>
            {
                var window = Open(3);
                SetCountOfFondo(window, 1, 3);
                SetCountOfFondo(window, 2, 1);
                SetCountOfFondo(window, 3, 3);
                FondoSelector(window).SelectedIndex = 0;

                // Una custom en el poste 3 del fondo 3, que solo existe mientras ese fondo tenga 3 frentes.
                SelectiveTargetsTestSupport.SetTargets(window, 3);
                var state = window.EditorState;
                state.SyncPostCabeceras();
                state.ApplyCabeceraToTargets(3, Cabecera(window, 250.0), c => c);

                SelectiveTargetsTestSupport.SetAllTargets(window);
                LostFocus(window, "BayCountBox");
                return state.CabeceraAt(2, 3) != null;
            });

            Assert.True(customs); // un redimensionado espurio la habría podado
        }

        // =========================================================================================
        // B. Retipear el MISMO valor visible SÍ es una edición (O-43-02)
        // =========================================================================================

        [Fact]
        public void B_RetypingTheSameVisibleValue_CountsAsAnEditAndReachesTheTargets()
        {
            var counts = StaTestRunner.Run(() =>
            {
                var window = Open(3);
                SetCountOfFondo(window, 1, 3);
                SetCountOfFondo(window, 2, 1);
                SetCountOfFondo(window, 3, 3);
                FondoSelector(window).SelectedIndex = 0;
                SelectiveTargetsTestSupport.SetAllTargets(window);

                // Retipear DE VERDAD: el usuario borra y vuelve a escribir, así que el texto pasa por un valor
                // intermedio. Asignar la misma cadena de golpe no dispara TextChanged en WPF y no sería el gesto
                // humano que O-43-02 describe.
                Type(window, "BayCountBox", string.Empty);
                TypeAndLeave(window, "BayCountBox", "3"); // el mismo 3 que ya mostraba: es una edición deliberada
                return Counts(window);
            });

            Assert.Equal(new[] { 3, 3, 3 }, counts);
        }

        // =========================================================================================
        // C / D. Las profundidades tampoco se mueven sin edición
        // =========================================================================================

        [Fact]
        public void C_LeavingThePalletDepthWithoutEditing_ChangesNoDepth()
        {
            var (depths, recomputes) = StaTestRunner.Run(() =>
            {
                var window = Open(3);
                SetDepthOfFondo(window, 1, 48.0);
                SetDepthOfFondo(window, 2, 60.0);
                SetDepthOfFondo(window, 3, 72.0);
                FondoSelector(window).SelectedIndex = 0;
                SelectiveTargetsTestSupport.SetAllTargets(window);

                var before = window.RecomputeCount;
                LostFocus(window, "FondoBox");
                return (Depths(window), window.RecomputeCount - before);
            });

            Assert.Equal(new[] { 48.0, 60.0, 72.0 }, depths);
            Assert.Equal(0, recomputes);
        }

        [Fact]
        public void D_LeavingTheCabeceraDepthWithoutEditing_ChangesNoOverride()
        {
            var overrides = StaTestRunner.Run(() =>
            {
                var window = Open(3);
                SetCabeceraOfFondo(window, 2, "37");
                SetCabeceraOfFondo(window, 3, "37");
                FondoSelector(window).SelectedIndex = 0; // el visible NO tiene override: su caja está vacía
                SelectiveTargetsTestSupport.SetAllTargets(window);

                LostFocus(window, "CabeceraFondoBox");
                return CabeceraOverrides(window);
            });

            // La caja vacía del fondo visible NO es un RESTORE: nadie la editó.
            Assert.Equal(new[] { 0.0, 37.0, 37.0 }, overrides);
        }

        // =========================================================================================
        // E. Vaciar la caja EXPLÍCITAMENTE sí restablece (la otra cara de O-43-02)
        // =========================================================================================

        [Fact]
        public void E_ClearingTheCabeceraDepthOnPurpose_RestoresEveryTarget()
        {
            var overrides = StaTestRunner.Run(() =>
            {
                var window = Open(3);
                SelectiveTargetsTestSupport.SetAllTargets(window);
                TypeAndLeave(window, "CabeceraFondoBox", "37");

                TypeAndLeave(window, "CabeceraFondoBox", string.Empty); // vaciada a propósito
                return CabeceraOverrides(window);
            });

            Assert.Equal(new[] { 0.0, 0.0, 0.0 }, overrides);
        }

        // =========================================================================================
        // F. S7: el texto pendiente no se filtra al slot del fondo visible ni a BuildDesign (INV-13)
        // =========================================================================================

        [Fact]
        public void F_TypedDepthWithoutLeavingTheField_NeverReachesTheVisibleFondosSlot()
        {
            var (before, afterNavigation, afterCommit) = StaTestRunner.Run(() =>
            {
                var window = Open(3);
                SelectiveTargetsTestSupport.SetTargets(window, 2, 3); // el fondo VISIBLE (1) no es destino
                FondoSelector(window).SelectedIndex = 0;

                var snapshot = Depths(window);
                Type(window, "FondoBox", "60");                 // tecleado, sin salir del campo
                Box(window, "FrenteBox").Text = "55";            // y una edición de celda pendiente
                window.EditorState.SelectCell(1, 0, extend: false); // clic de navegación: recompute incidental
                var navigated = Depths(window);

                LostFocus(window, "FondoBox");
                return (snapshot, navigated, Depths(window));
            });

            Assert.Equal(before, afterNavigation);                       // el slot visible no se movió
            Assert.Equal(before[0], afterCommit[0]);                     // y sigue sin moverse tras comprometer
            Assert.Equal(60.0, afterCommit[1]);                          // solo los destinos
            Assert.Equal(60.0, afterCommit[2]);
        }

        [Fact]
        public void F_BuildDesignNeverConsumesPendingText()
        {
            var (depthCount, palletDepth) = StaTestRunner.Run(() =>
            {
                var window = Open(2);
                Type(window, "FondosBox", "3");  // pendientes: nadie salió del campo
                Type(window, "FondoBox", "60");

                var design = window.BuildDesignForTest(out _);
                return (design.DepthCount, design.PalletDepth);
            });

            Assert.Equal(2, depthCount);      // los fondos COMPROMETIDOS, no el "3" tecleado
            Assert.NotEqual(60.0, palletDepth); // ni la profundidad tecleada
        }

        // =========================================================================================
        // G. Una frontera transaccional compromete lo pendiente antes de consumirlo
        // =========================================================================================

        [Fact]
        public void G_ATransactionalBoundaryCommitsThePendingValueBeforeConsumingIt()
        {
            var (depths, recomputes) = StaTestRunner.Run(() =>
            {
                var window = Open(3);
                SelectiveTargetsTestSupport.SetAllTargets(window);
                Type(window, "FondoBox", "60"); // pendiente, sin LostFocus

                var before = window.RecomputeCount;
                PressRecalcular(window);        // frontera: debe comprometerlo antes de construir
                return (Depths(window), window.RecomputeCount - before);
            });

            Assert.Equal(new[] { 60.0, 60.0, 60.0 }, depths);
            Assert.Equal(1, recomputes); // INV-07: una operación, un recompute
        }

        // =========================================================================================
        // H. Atomicidad: un pendiente inválido aborta y NO aplica los válidos (INV-16)
        // =========================================================================================

        [Fact]
        public void H_AnInvalidPendingFieldAbortsTheWholeCommit()
        {
            var (depths, overrides, counts, targets, recomputes, leftover) = StaTestRunner.Run(() =>
            {
                var window = Open(3);
                SelectiveTargetsTestSupport.SetAllTargets(window);
                var depthsBefore = Depths(window);

                Type(window, "FondoBox", "60");          // válido
                Type(window, "CabeceraFondoBox", "48");  // válido
                Type(window, "BayCountBox", "abc");      // INVÁLIDO

                var before = window.RecomputeCount;
                PressRecalcular(window);
                return (
                    Depths(window),
                    CabeceraOverrides(window),
                    Counts(window),
                    window.EditorState.TargetFondos.Fondos.ToArray(),
                    window.RecomputeCount - before,
                    Box(window, "BayCountBox").Text);
            });

            Assert.Equal(new[] { 48.0, 48.0, 48.0 }, depths);   // el 60 NO se aplicó
            Assert.Equal(new[] { 0.0, 0.0, 0.0 }, overrides);   // el 48 de cabecera tampoco
            Assert.Equal(new[] { 2, 2, 2 }, counts);            // la estructura no cambió
            Assert.Equal(new[] { 0, 1, 2 }, targets);           // TargetFondos intacto
            Assert.Equal(0, recomputes);
            Assert.Equal("abc", leftover);                      // sin auto-reparación en una frontera
        }

        [Fact]
        public void H_AnInvalidFondosBoxAlsoAbortsTheWholeCommit()
        {
            var (depths, fondoCount, leftover) = StaTestRunner.Run(() =>
            {
                var window = Open(2);
                SelectiveTargetsTestSupport.SetAllTargets(window);

                Type(window, "FondoBox", "60"); // válido
                Type(window, "FondosBox", "x"); // INVÁLIDO

                PressRecalcular(window);
                return (Depths(window), window.EditorState.FondoCount, Box(window, "FondosBox").Text);
            });

            Assert.Equal(new[] { 48.0, 48.0 }, depths);
            Assert.Equal(2, fondoCount);
            Assert.Equal("x", leftover);
        }

        // =========================================================================================
        // S / T / U. Orden: estructura antes que valores, con TargetFondos re-resuelto (INV-17)
        // =========================================================================================

        [Fact]
        public void S_StructureIsAppliedBeforeValues_SoANewFondoGetsTheTypedDepth()
        {
            var (depths, targets, recomputes) = StaTestRunner.Run(() =>
            {
                var window = Open(2);
                SelectiveTargetsTestSupport.SetAllTargets(window);

                Type(window, "FondoBox", "60");  // valor pendiente
                Type(window, "FondosBox", "3");  // y un cambio ESTRUCTURAL pendiente

                var before = window.RecomputeCount;
                PressRecalcular(window);
                return (Depths(window), window.EditorState.TargetFondos.Fondos.ToArray(), window.RecomputeCount - before);
            });

            Assert.Equal(new[] { 60.0, 60.0, 60.0 }, depths); // el fondo nuevo entra en "Todos" y recibe el 60
            Assert.Equal(new[] { 0, 1, 2 }, targets);
            Assert.Equal(1, recomputes);
        }

        [Fact]
        public void T_WithAnExplicitTargetSet_TheNewFondoKeepsItsClonedDepth()
        {
            var (depths, targets) = StaTestRunner.Run(() =>
            {
                var window = Open(2);
                SelectiveTargetsTestSupport.SetTargets(window, 1, 2); // Explicit {0,1}

                Type(window, "FondoBox", "60");
                Type(window, "FondosBox", "3");
                PressRecalcular(window);
                return (Depths(window), window.EditorState.TargetFondos.Fondos.ToArray());
            });

            Assert.Equal(new[] { 60.0, 60.0, 48.0 }, depths); // el fondo 3 no era destino: conserva lo clonado
            Assert.Equal(new[] { 0, 1 }, targets);
        }

        [Fact]
        public void U_StructuralPendingsCombine_FondoCountThenFrenteCount()
        {
            var (all, explicitOne) = StaTestRunner.Run(() =>
            {
                var a = Open(2);
                SelectiveTargetsTestSupport.SetAllTargets(a);
                Type(a, "FondosBox", "3");
                Type(a, "BayCountBox", "4");
                PressRecalcular(a);

                var b = Open(2);
                SelectiveTargetsTestSupport.SetTargets(b, 2); // Explicit {1}
                Type(b, "FondosBox", "3");
                Type(b, "BayCountBox", "4");
                PressRecalcular(b);

                return (Counts(a), Counts(b));
            });

            Assert.Equal(new[] { 4, 4, 4 }, all);
            Assert.Equal(new[] { 2, 4, 2 }, explicitOne); // el fondo nuevo conserva el conteo clonado
        }

        // =========================================================================================
        // W. Idempotencia: tras comprometer, la frontera no vuelve a aplicar (INV-07)
        // =========================================================================================

        [Fact]
        public void W_AfterItsOwnCommit_TheBoundaryFindsNothingToDo()
        {
            var (depths, recomputes) = StaTestRunner.Run(() =>
            {
                var window = Open(3);
                SelectiveTargetsTestSupport.SetAllTargets(window);
                TypeAndLeave(window, "FondoBox", "60"); // ya comprometido por su gesto propio

                var before = window.RecomputeCount;
                PressRecalcular(window);
                return (Depths(window), window.RecomputeCount - before);
            });

            Assert.Equal(new[] { 60.0, 60.0, 60.0 }, depths); // sin cambio adicional
            Assert.Equal(0, recomputes);                      // y sin recompute por ese pendiente
        }

        // =========================================================================================
        // X / V. Gesto propio estructural: hermano inválido aborta; solo aborta se auto-repara
        // =========================================================================================

        [Fact]
        public void X_AStructuralGestureAbortsWhenASiblingIsDirtyAndInvalid()
        {
            var (counts, leftover) = StaTestRunner.Run(() =>
            {
                var window = Open(2);
                SelectiveTargetsTestSupport.SetAllTargets(window);

                Type(window, "FondoBox", "abc");     // hermano sucio e inválido
                TypeAndLeave(window, "BayCountBox", "4");
                return (Counts(window), Box(window, "FondoBox").Text);
            });

            Assert.Equal(new[] { 2, 2 }, counts);  // no redimensiona
            Assert.Equal("abc", leftover);          // y NO hace Show sobre el dirty
        }

        [Fact]
        public void V_AnInvalidFrenteCountAloneStillAutoRepairs()
        {
            var (counts, shown) = StaTestRunner.Run(() =>
            {
                var window = Open(2);
                SelectiveTargetsTestSupport.SetAllTargets(window);

                TypeAndLeave(window, "BayCountBox", "abc"); // sin hermanos sucios: gesto propio
                return (Counts(window), Box(window, "BayCountBox").Text);
            });

            Assert.Equal(new[] { 2, 2 }, counts); // sin mutación
            Assert.Equal("2", shown);              // restaurado al valor comprometido
        }

        // ---- helper de cabecera ----

        private static RackCad.Domain.RackFrames.RackFrameConfiguration Cabecera(RackSelectiveWindow window, double height)
            => new RackCad.Application.RackFrames.RackFrameConfigurationFactory(window.Session.Catalog).Build(
                RackCad.Application.RackFrames.RackFrameTemplateCatalog.FindStandardOrDefault(),
                "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA",
                height,
                42.0);
    }
}
