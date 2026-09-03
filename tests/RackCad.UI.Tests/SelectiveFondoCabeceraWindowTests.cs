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
    /// STA tests for the REAL <see cref="RackSelectiveWindow"/> wiring of I-43 Gate 4: a custom cabecera belongs to
    /// <c>(fondo, poste)</c>, is applied over the SAME target fondos the cell editor uses, and the per-post PERALTE
    /// stays a global authority. The window's own controls and handlers drive everything.
    /// </summary>
    public sealed class SelectiveFondoCabeceraWindowTests
    {
        private static RackSelectiveWindow OpenWith(int fondos) => SelectiveWindowTestSupport.Open(fondos);

        private static void RaiseLostFocus(Window window, string name)
        {
            var box = (TextBox)window.FindName(name);
            box.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, box));
        }

        /// <summary>Choose the target fondos through the REAL dropdown (I-43, gate 8A): "todos", or a comma/plus
        /// separated list of one-based fondo numbers.</summary>
        private static void SetTargetFondos(RackSelectiveWindow window, string text)
        {
            if (text.Equals("todos", System.StringComparison.OrdinalIgnoreCase))
            {
                SelectiveTargetsTestSupport.SetAllTargets(window);
                return;
            }

            var wanted = text.Split(new[] { ',', '+', ' ' }, System.StringSplitOptions.RemoveEmptyEntries)
                .SelectMany(token =>
                {
                    var range = token.Split('-');
                    return range.Length == 2
                        ? Enumerable.Range(int.Parse(range[0]), int.Parse(range[1]) - int.Parse(range[0]) + 1)
                        : new[] { int.Parse(token) };
                })
                .ToArray();
            SelectiveTargetsTestSupport.SetTargets(window, wanted);
        }

        private static RackFrameConfiguration Custom(RackSelectiveWindow window, double height)
            => new RackFrameConfigurationFactory(window.Session.Catalog).Build(
                RackFrameTemplateCatalog.FindStandardOrDefault(),
                window.EditorState.PostCabeceras.Count > 0 ? "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA" : null,
                height,
                42.0);

        [Fact]
        public void ACabeceraAppliedOverSeveralFondos_LandsOnEachWithItsOwnCopy()
        {
            var (heights, independent) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(4);
                SetTargetFondos(window, "1+3");
                var state = window.EditorState;

                state.ApplyCabeceraToTargets(1, Custom(window, 250.0), c => new RackCad.Application.Persistence.RackFrameProjectStore().DeepCopy(c));

                var a = state.CabeceraAt(0, 1);
                var b = state.CabeceraAt(2, 1);
                a.Height = 999.0; // editing one must not move the other
                return (
                    new[] { state.CabeceraAt(0, 1)?.Height ?? 0.0, state.CabeceraAt(1, 1)?.Height ?? 0.0, state.CabeceraAt(2, 1)?.Height ?? 0.0 },
                    b.Height);
            });

            Assert.Equal(new[] { 999.0, 0.0, 250.0 }, heights); // fondos 1 and 3 (one-based); fondo 2 untouched
            Assert.Equal(250.0, independent);
        }

        [Fact]
        public void ThePostStatus_DescribesTheVisibleFondo_NotAlwaysFondoZero()
        {
            var (onFondoOne, onFondoTwo) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(2);
                SetTargetFondos(window, "1");
                window.EditorState.ApplyCabeceraToTargets(1, Custom(window, 260.0), c => c);
                ((ComboBox)window.FindName("PostSelectBox")).SelectedIndex = 1; // a real change: fires PostSelect_Changed

                var status = (TextBlock)window.FindName("PostCabeceraStatus");
                var first = status.Text;
                ((ComboBox)window.FindName("FondoSelectorBox")).SelectedIndex = 1; // switch to fondo 2
                return (first, status.Text);
            });

            Assert.Contains("Personalizada", onFondoOne);
            Assert.Contains("Por defecto", onFondoTwo); // the same post is standard on the other fondo
        }

        [Fact]
        public void ResettingOverSomeFondos_DoesNotClearTheGlobalPostPeralte()
        {
            // PostPeraltes is a GLOBAL per-post authority (I-43): a cabecera reset aimed at some fondos must not wipe it.
            var (peralte, cabeceras) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(3);
                var state = window.EditorState;
                SetTargetFondos(window, "1-3");
                state.ApplyCabeceraToTargets(0, Custom(window, 270.0), c => c);
                state.SyncPostCabeceras();
                state.PostPeraltes[0] = 7.0;

                SetTargetFondos(window, "2");
                ((ComboBox)window.FindName("PostSelectBox")).SelectedIndex = 0;
                EditorWindowTestSupport.ClickByContent(window, "Restablecer poste");

                return (state.PostPeraltes[0], new[]
                {
                    state.CabeceraAt(0, 0) != null, state.CabeceraAt(1, 0) != null, state.CabeceraAt(2, 0) != null
                });
            });

            Assert.Equal(7.0, peralte);                              // untouched by a partial reset
            Assert.Equal(new[] { true, false, true }, cabeceras);    // only fondo 2 was reset
        }

        [Fact]
        public void ResettingOverEveryFondo_StillClearsTheGlobalPeralte_AsItAlwaysDid()
        {
            var peralte = StaTestRunner.Run(() =>
            {
                var window = OpenWith(1); // a single fondo: the targets are every fondo, the legacy case
                var state = window.EditorState;
                state.SyncPostCabeceras();
                state.PostPeraltes[0] = 7.0;
                ((ComboBox)window.FindName("PostSelectBox")).SelectedIndex = 0;
                EditorWindowTestSupport.ClickByContent(window, "Restablecer poste");
                return state.PostPeraltes[0];
            });

            Assert.Equal(0.0, peralte);
        }

        [Fact]
        public void ACabeceraAimedAtAFondoWithoutThatPost_IsOmittedWithoutBlockingTheValidTargets()
        {
            var (applied, omitted) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(2);
                var state = window.EditorState;

                // Fondo 2 keeps two frentes (posts 0..2); fondo 1 grows to four (posts 0..4).
                EditorWindowTestSupport.SetText(window, "BayCountBox", "4");
                RaiseLostFocus(window, "BayCountBox");
                SetTargetFondos(window, "1,2");

                var result = state.ApplyCabeceraToTargets(4, Custom(window, 280.0), c => c);
                return (result.AppliedFondos.ToArray(), result.OmittedFondos.ToArray());
            });

            Assert.Equal(new[] { 0 }, applied);
            Assert.Equal(new[] { 1 }, omitted);
        }

        [Fact]
        public void TheCabeceraAxisReusesTheSameTargetFondos_WithNoSecondSelector()
        {
            // One grammar of fondos for the whole editor: there is exactly one target-fondo control.
            var count = StaTestRunner.Run(() =>
            {
                var window = OpenWith(2);
                return new[] { "TargetFondosList" }.Count(name => window.FindName(name) != null);
            });

            Assert.Equal(1, count);
        }
    }
}
