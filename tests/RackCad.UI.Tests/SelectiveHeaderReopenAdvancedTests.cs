using System.Collections.Generic;
using System.Globalization;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.UI.Systems.Selective;
using Xunit;
using S = RackCad.UI.Tests.SelectiveHeaderBatchTestSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-53S, G5 — S-31 (N-01 = A; Proposal V2 §6.11): «Personalizar» de una cabecera del Selectivo YA personalizada abre el
    /// configurador compartido en el EDITOR AVANZADO; una estandar, en «Configuracion rapida».
    /// <para>
    /// El motivo es de datos, no de estetica: en modo rapido el unico camino para cambiar la altura es «Aplicar», que NO edita
    /// sino que RECONSTRUYE la cabecera desde la plantilla, y la receta perdida se copiaria a todos los «Fondos destino». El
    /// modo se elige desde la ventana del Selectivo con la propiedad publica del ViewModel; el configurador no se toca. Se
    /// recorre el handler REAL con la costura de presentacion, sin bucle modal.
    /// </para>
    /// </summary>
    public sealed class SelectiveHeaderReopenAdvancedTests
    {
        private static bool[] ModesSeenWhenCustomizing(bool alreadyCustom)
            => StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open();
                if (alreadyCustom)
                {
                    S.PlaceCustom(window, 1, 2, window.CustomizeSeedHeightForTest(1));
                }

                S.SelectPost(window, 2);
                var modes = new List<bool>();
                window.HeaderConfiguratorPresenter = S.EditIncrementally(null, modes);
                return S.WithShownWindow(window, () =>
                {
                    using (SelectiveCabeceraHeightPrompt.Substitute(_ => true))
                    {
                        EditorWindowTestSupport.ClickNamed(window, "CustomizePostButton");
                    }

                    return modes.ToArray();
                });
            });

        [Fact]
        public void S31_CustomizingAnAlreadyCustomCabecera_OpensTheAdvancedEditor()
        {
            Assert.Equal(new[] { true }, ModesSeenWhenCustomizing(alreadyCustom: true));
        }

        [Fact]
        public void S31_CustomizingAStandardCabecera_OpensTheQuickMode()
        {
            Assert.Equal(new[] { false }, ModesSeenWhenCustomizing(alreadyCustom: false));
        }

        /// <summary>
        /// Companera de N-01: el EDIT usa el RESULTADO REAL del configurador. «Aplicar» del modo rapido REEMPLAZA la
        /// configuracion del ViewModel por una instancia nueva; si la ventana leyera la semilla que entrego, la altura del
        /// usuario se perderia en silencio (el defecto PBH-01 que I-40 corrigio en Push Back).
        /// </summary>
        [Fact]
        public void S31_TheEditUsesTheConfiguratorsRealResult_NotTheSeedItHandedIn()
        {
            var r = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open();
                S.SelectPost(window, 2);
                var height = window.CustomizeSeedHeightForTest(1) + 18.0;
                window.HeaderConfiguratorPresenter = configurator =>
                {
                    configurator.ViewModel.SimpleHeightText = height.ToString(CultureInfo.InvariantCulture);
                    configurator.ViewModel.ApplySimpleConfiguration(); // reemplaza la Configuration del ViewModel
                    configurator.Close();
                };

                return S.WithShownWindow(window, () =>
                {
                    using (SelectiveCabeceraHeightPrompt.Substitute(_ => true))
                    {
                        EditorWindowTestSupport.ClickNamed(window, "CustomizePostButton");
                    }

                    return (Outcome: window.LastHeaderBatchOutcome, Height: window.EditorState.CabeceraAt(0, 1)?.Height ?? 0.0, Expected: height);
                });
            });

            Assert.IsType<HeaderBatchOutcome<SelectiveHeaderAddress>.Committed>(r.Outcome);
            Assert.Equal(r.Expected, r.Height);
        }
    }
}
