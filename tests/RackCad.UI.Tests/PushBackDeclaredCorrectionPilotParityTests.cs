using System.Collections.Generic;
using Xunit;
using static RackCad.UI.Tests.PushBackDeclaredCorrectionSupport;
using static RackCad.UI.Tests.PushBackFullEditorCharacterizationSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4B PB-TC — la ventana CLASICA de Push Back y el piloto del Workspace escriben el MISMO diseño. CT-PBTC-06 lo fija en
    /// los catorce diseños del corpus de la caracterizacion de la carga (sustituye a la Theory de WLC), y CT-PBTC-10 en cada campo
    /// exacto de F1 a F11 y en un rack con modulos personalizados (M-02, I-02).
    ///
    /// <para>Cada fila carga el sobre en la ventana (<c>admitida</c> es que la ventana lo admite) y compara lo que su
    /// <c>LastComputation.Design</c> escribiria con lo que escribe el piloto con el mismo catalogo. Ninguna ventana se muestra.</para>
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackDeclaredCorrectionPilotParityTests
    {
        [Fact]
        public void CT_PBTC_06_TheWindowAndThePilotWriteTheSameDesignForTheFourteenDesigns()
        {
            var nombres = new[]
            {
                "pb-01-base", "pb-02-modulos", "pb-03-fondos", "pb-04-blanco", "pb-05-topes", "pb-06-seguridad", "pb-07-combinado", "pb-08-redondeo",
                "pb-10-sin-datum", "pb-11-sin-i41", "pb-12-poste-nulo", "pb-13-sideb-no-presente", "pb-14-peralte-heredado", "pb-15-sin-composite",
            };

            var log = new List<string>();
            Sta(() =>
            {
                foreach (var nombre in nombres)
                {
                    log.Add(Paridad(nombre, Fixture(nombre)));
                }
            });

            Assert.Equal(
                new[]
                {
                    "pb-01-base admitida=True delta=[]",
                    "pb-02-modulos admitida=True delta=[]",
                    "pb-03-fondos admitida=True delta=[]",
                    "pb-04-blanco admitida=True delta=[]",
                    "pb-05-topes admitida=True delta=[]",
                    "pb-06-seguridad admitida=True delta=[]",
                    "pb-07-combinado admitida=True delta=[]",
                    "pb-08-redondeo admitida=True delta=[]",
                    "pb-10-sin-datum admitida=True delta=[]",
                    "pb-11-sin-i41 admitida=True delta=[]",
                    "pb-12-poste-nulo admitida=True delta=[]",
                    "pb-13-sideb-no-presente admitida=True delta=[]",
                    "pb-14-peralte-heredado admitida=True delta=[]",
                    "pb-15-sin-composite admitida=True delta=[]",
                },
                log);
        }

        [Fact]
        public void CT_PBTC_10_TheWindowAndThePilotWriteTheSameDesignForEveryExactFieldAndForCustomModules()
        {
            var log = new List<string>();
            Sta(() =>
            {
                foreach (var fila in FilasF())
                {
                    log.Add(Paridad(fila.Name, VarianteDe(fila)));
                }

                // M-02: pb-02-modulos (M1 con cabecera propia, M2 con longitud manual) con el fondo exacto.
                var m02 = EditInner(Fixture("pb-02-modulos"), inner => Poner(inner, "Structure.PalletDepth", 48.00044));
                log.Add(Paridad("M-02", m02));
            });

            Assert.Equal(
                new[]
                {
                    "F1 admitida=True delta=[]",
                    "F2 admitida=True delta=[]",
                    "F3 admitida=True delta=[]",
                    "F4 admitida=True delta=[]",
                    "F5 admitida=True delta=[]",
                    "F6 admitida=True delta=[]",
                    "F7 admitida=True delta=[]",
                    "F8 admitida=True delta=[]",
                    "F9 admitida=True delta=[]",
                    "F10 admitida=True delta=[]",
                    "F11 admitida=True delta=[]",
                    "M-02 admitida=True delta=[]",
                },
                log);
        }
    }
}
