using System.Collections.Generic;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.UI.Controls;
using Xunit;
using static RackCad.UI.Tests.PushBackFullEditorCharacterizationSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4B PB-T2b - CT-PBT2B-10 (Q-PBT2-3, opcion A): el campo de escala del panel y la caja <c>AnnotationScaleBox</c> de la
    /// ventana aceptan y rechazan los mismos textos, con los mismos mensajes. El panel valida con
    /// <c>PushBackPilotFieldAdapter.Validate</c> (Application, anterior a F4B) y la ventana con
    /// <c>NumericFieldValidation.Validate</c> con las reglas de su caja (obligatoria, no entera, mayor que cero). PB-T2b no unifica
    /// las dos implementaciones: fija con esta prueba que no se separan, y lo deja para PB-T4.
    ///
    /// <para>Es una GUARDA: pasa antes y despues de la implementacion de la ventana, y discrimina por su autoprueba, que compara el
    /// campo del panel con una caja que SI admite el cero. Corre en el hilo STA con la cultura invariante.</para>
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackFieldValidationConcordanceTests
    {
        private static bool Iguales(string texto, bool minimoInclusivo)
        {
            var panel = new PushBackPilotFieldAdapter().Validate(PushBackPilotFieldAdapter.AnnotationScaleKey, texto);
            var caja = NumericFieldValidation.Validate(texto, false, false, 0.0, null, minimoInclusivo, true);
            return panel.IsValid == !caja.HasError && panel.Message == caja.Message;
        }

        [Fact]
        public void CT_PBT2B_10_ThePanelScaleFieldAndTheWindowScaleBoxAcceptAndRejectTheSameTexts()
        {
            var log = new List<string>();
            Sta(() =>
            {
                foreach (var texto in new[] { string.Empty, "   ", "abc", "0", "-1", "0,5", "0.5", "2.5", "1e3", "1.234,5" })
                {
                    log.Add("[" + texto + "] iguales=" + Iguales(texto, false));
                }

                // La autoprueba: una caja que admite el cero ya no coincide con el campo del panel.
                log.Add("autoprueba [0] inclusivo iguales=" + Iguales("0", true));
            });

            Assert.Equal(
                new[]
                {
                    "[] iguales=True",
                    "[   ] iguales=True",
                    "[abc] iguales=True",
                    "[0] iguales=True",
                    "[-1] iguales=True",
                    "[0,5] iguales=True",
                    "[0.5] iguales=True",
                    "[2.5] iguales=True",
                    "[1e3] iguales=True",
                    "[1.234,5] iguales=True",
                    "autoprueba [0] inclusivo iguales=False",
                },
                log);
        }
    }
}
