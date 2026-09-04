using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.RackFrames;
using RackCad.Domain.RackFrames;
using RackCad.UI.Systems.Selective;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-43, gate 8.6E (ARQ-43-05 + validación multi-destino + ARQ-43-18): "Personalizar" describe el fondo VISIBLE y
    /// valida TODOS los fondos destino, no solo el que se ve.
    /// <para>
    /// Una misma receta puede ser válida en un fondo, discrepante en otro y peligrosa en un tercero — los fondos
    /// tienen topologías y alturas propias. Avisar solo del visible deja pasar en silencio el caso que importa.
    /// </para>
    /// <para>
    /// Y el peralte del poste es una autoridad GLOBAL: si el usuario cancela el aviso, no puede quedar escrito.
    /// </para>
    /// </summary>
    public sealed class SelectiveCustomizeTargetsTests
    {
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";

        private static RackFrameConfiguration Custom(RackSelectiveWindow window, double height)
            => new RackFrameConfigurationFactory(window.Session.Catalog).Build(
                RackFrameTemplateCatalog.FindStandardOrDefault(), PostId, height, 42.0);

        private static ComboBox FondoSelector(RackSelectiveWindow window) => (ComboBox)window.FindName("FondoSelectorBox");

        private static void SetText(RackSelectiveWindow window, string name, string text)
        {
            EditorWindowTestSupport.SetText(window, name, text);
            var box = (TextBox)window.FindName(name);
            box.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, box));
        }

        /// <summary>Da a UN fondo su propia altura de celda, para que su poste resuelva distinto que el de al lado.</summary>
        private static void SetLevelHeightOfFondo(RackSelectiveWindow window, int oneBased, double alto)
        {
            FondoSelector(window).SelectedIndex = oneBased - 1;
            SelectiveTargetsTestSupport.SetTargets(window, oneBased);
            SetText(window, "AltoBox", alto.ToString(CultureInfo.InvariantCulture));
            EditorWindowTestSupport.ClickByContent(window, "Todas");
        }

        /// <summary>Fija los frentes de UN fondo.</summary>
        private static void SetFrentesOfFondo(RackSelectiveWindow window, int oneBased, int frentes)
        {
            FondoSelector(window).SelectedIndex = oneBased - 1;
            SelectiveTargetsTestSupport.SetTargets(window, oneBased);
            SetText(window, "BayCountBox", frentes.ToString(CultureInfo.InvariantCulture));
        }

        // =========================================================================================
        // K. La semilla usa la geometría del fondo SELECCIONADO
        // =========================================================================================

        [Fact]
        public void K_TheSeedHeightComesFromTheSelectedFondo_NotFromFondoZero()
        {
            var (onFondoOne, onFondoTwo) = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(2);
                SetLevelHeightOfFondo(window, 1, 40.0);
                SetLevelHeightOfFondo(window, 2, 90.0); // el fondo 2 es claramente más alto

                FondoSelector(window).SelectedIndex = 0;
                var one = window.CustomizeSeedHeightForTest(1);
                FondoSelector(window).SelectedIndex = 1;
                return (one, window.CustomizeSeedHeightForTest(1));
            });

            Assert.True(onFondoTwo > onFondoOne,
                "La semilla del fondo 2 (" + onFondoTwo + ") debería ser mayor que la del fondo 1 (" + onFondoOne + ").");
        }

        // =========================================================================================
        // L. La validación mira TODOS los destinos, no solo el visible
        // =========================================================================================

        [Fact]
        public void L_TheWarningNamesATargetFondoThatIsNotTheVisibleOne()
        {
            var messages = new List<string>();
            StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(2);
                SetLevelHeightOfFondo(window, 1, 40.0);
                SetLevelHeightOfFondo(window, 2, 90.0); // solo el fondo 2 quedará por debajo del nivel superior

                FondoSelector(window).SelectedIndex = 0;                 // se ve el fondo 1
                SelectiveTargetsTestSupport.SetTargets(window, 1, 2);    // pero se escribe en los dos

                // Una altura cómoda para el fondo 1 y demasiado baja para el fondo 2.
                var cfg = Custom(window, window.CustomizeSeedHeightForTest(1));
                using (SelectiveCabeceraHeightPrompt.Substitute(
                    m => { messages.Add(m); return true; },
                    m => messages.Add(m)))
                {
                    window.ApplyCustomizedCabeceraForTest(1, cfg, 0.0);
                }

                return 0;
            });

            var text = string.Join(" | ", messages);
            Assert.Contains("2", text); // el mensaje tiene que nombrar el fondo 2, que no es el visible
        }

        // =========================================================================================
        // M. Cancelar deja mutación CERO — incluido el peralte del poste
        // =========================================================================================

        [Fact]
        public void M_CancellingLeavesNoCustom_NoPostPeralte_AndNoRecompute()
        {
            var (customs, peralte, recomputes) = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(2);
                SetLevelHeightOfFondo(window, 1, 40.0);
                SetLevelHeightOfFondo(window, 2, 90.0);

                FondoSelector(window).SelectedIndex = 0;
                SelectiveTargetsTestSupport.SetTargets(window, 1, 2);
                var state = window.EditorState;
                state.SyncPostCabeceras();
                var peralteBefore = state.PostPeraltes[1];

                // Una altura RIDÍCULAMENTE baja: severa en los dos fondos.
                var cfg = Custom(window, 20.0);
                cfg.PostPeralte = 7.0; // y el usuario tocó el peralte en el configurador

                var before = window.RecomputeCount;
                using (SelectiveCabeceraHeightPrompt.Substitute(_ => false)) // Cancelar
                {
                    window.ApplyCustomizedCabeceraForTest(1, cfg, 0.0);
                }

                return (
                    new[] { state.CabeceraAt(0, 1), state.CabeceraAt(1, 1) },
                    (before: peralteBefore, after: state.PostPeraltes[1]),
                    window.RecomputeCount - before);
            });

            Assert.All(customs, c => Assert.Null(c));           // ninguna receta escrita
            Assert.Equal(peralte.before, peralte.after);        // el peralte GLOBAL no se tocó
            Assert.Equal(0, recomputes);                        // y no hubo recompute productivo
        }

        // =========================================================================================
        // Un destino sin ese poste se OMITE — ni se crea ni aborta
        // =========================================================================================

        [Fact]
        public void ATargetWithoutThatPost_IsSkipped_AndTheValidOnesStillReceiveTheCustom()
        {
            var (heights, frentes) = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(2);
                SetFrentesOfFondo(window, 1, 4); // postes 0..4
                SetFrentesOfFondo(window, 2, 1); // postes 0..1: el poste 4 NO existe aquí

                FondoSelector(window).SelectedIndex = 0;
                SelectiveTargetsTestSupport.SetTargets(window, 1, 2);
                var state = window.EditorState;
                state.SyncPostCabeceras();

                var cfg = Custom(window, window.CustomizeSeedHeightForTest(4));
                using (SelectiveCabeceraHeightPrompt.Substitute(_ => true))
                {
                    window.ApplyCustomizedCabeceraForTest(4, cfg, 0.0);
                }

                return (
                    new[] { state.CabeceraAt(0, 4)?.Height ?? 0.0, state.CabeceraAt(1, 4)?.Height ?? 0.0 },
                    new[] { state.Bays.Count, state.FondoMatrices[1].Bays.Count });
            });

            Assert.True(heights[0] > 0.0);          // el fondo 1 sí la recibe
            Assert.Equal(0.0, heights[1]);          // el fondo 2 se omite
            Assert.Equal(new[] { 4, 1 }, frentes);  // y NO se le inventaron frentes para que el poste existiera
        }

        // =========================================================================================
        // Solo informativas: avisa y aplica, sin pedir confirmación
        // =========================================================================================

        [Fact]
        public void InformativeOnly_WarnsAndApplies_WithoutAskingToConfirm()
        {
            var (asked, informed, applied) = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(2);
                SelectiveTargetsTestSupport.SetTargets(window, 1, 2);
                var state = window.EditorState;
                state.SyncPostCabeceras();

                // Bastante MÁS alta que la resuelta: difiere, pero nunca queda por debajo del nivel superior.
                var cfg = Custom(window, window.CustomizeSeedHeightForTest(1) + 40.0);

                var confirmations = 0;
                var informs = new List<string>();
                using (SelectiveCabeceraHeightPrompt.Substitute(
                    _ => { confirmations++; return true; },
                    m => informs.Add(m)))
                {
                    window.ApplyCustomizedCabeceraForTest(1, cfg, 0.0);
                }

                return (confirmations, informs.Count, state.CabeceraAt(0, 1) != null);
            });

            Assert.Equal(0, asked);   // no se pide confirmación por una diferencia informativa
            Assert.True(informed > 0); // pero sí se avisa
            Assert.True(applied);      // y se aplica
        }

        // =========================================================================================
        // C. Medio frente: el ancho sale del fondo SELECCIONADO
        // =========================================================================================

        [Fact]
        public void Medio_TheFullWidthComesFromTheSelectedFondo_EvenForAFrenteFondoZeroDoesNotHave()
        {
            var width = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(2);
                SetFrentesOfFondo(window, 1, 1); // el fondo 1 solo tiene el frente 0
                SetFrentesOfFondo(window, 2, 3); // el fondo 2 tiene 0..2

                FondoSelector(window).SelectedIndex = 1; // se edita el fondo 2
                return window.TramosFullWidthForTest(2); // un frente que el fondo 1 NO tiene
            });

            Assert.True(width > 0.0, "El ancho del frente 2 del fondo visible deberia ser > 0, no " + width + ".");
        }
    }
}
