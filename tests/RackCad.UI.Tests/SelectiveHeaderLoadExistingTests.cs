using System.Collections.Generic;
using System.Windows.Controls;
using RackCad.Application.Persistence;
using RackCad.Domain.RackFrames;
using Xunit;
using S = RackCad.UI.Tests.SelectiveHeaderBatchTestSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-53S, G5 — la mitad WPF de S-23 (Proposal V2 §13.3; contrato de I-53 §14.1). G4 fijo la mitad de Application de
    /// RACKEDITAR (store + <c>SelectiveEditorOpen</c>) sin <c>LoadExisting</c> de la ventana; esta prueba cierra la otra mitad:
    /// un rack con cabeceras personalizadas repartidas por fondos y postes se reabre en la ventana REAL, la ventana las
    /// representa en cada (fondo, poste), no pierde ninguna y no materializa ninguna donde no la habia, y lo que Actualizar
    /// entrega sigue siendo exactamente eso.
    /// </summary>
    public sealed class SelectiveHeaderLoadExistingTests
    {
        [Fact]
        public void S23_LoadExisting_WithCustomsSpreadOverFondosAndPosts_RepresentsThem_LosesNone_AndInventsNone()
        {
            var r = StaTestRunner.Run(() =>
            {
                // Fondo 1: 3 frentes (postes 0..3) con personalizadas en los postes 2 y 4.
                // Fondo 2: 2 frentes (postes 0..2) con personalizada en el poste 1.
                var design = S.Design(3, 2);
                design.PostCabeceras.Add(null);
                design.PostCabeceras.Add(S.Recipe(150.0));
                design.PostCabeceras.Add(null);
                design.PostCabeceras.Add(S.Recipe(160.0));
                design.ExtraFondoPostCabeceras.Add(new List<RackFrameConfiguration> { S.Recipe(170.0), null, null });

                var window = SelectiveWindowTestSupport.Open(canInsertInAutoCad: true);
                window.LoadExisting(SelectivePalletDesignDocument.From(design, "GUID-I53S-S23", "Selectivo S23"));
                var stateMap = S.CustomMap(window.EditorState);

                var legend = (TextBlock)window.FindName("PostCabeceraStatus");
                S.ShowFondo(window, 1);
                S.SelectPost(window, 2);
                var fondoOnePostTwo = legend.Text;
                S.SelectPost(window, 1);
                var fondoOnePostOne = legend.Text;
                S.ShowFondo(window, 2);
                S.SelectPost(window, 1);
                var fondoTwoPostOne = legend.Text;

                EditorWindowTestSupport.ClickNamed(window, "UpdateButton");
                var system = window.SystemToInsert;
                var payload = window.DesignToInsert;
                var persisted = new List<string>();
                foreach (var custom in payload.PostCabeceras) persisted.Add(custom == null ? "-" : S.R(custom.Height));
                persisted.Add("|");
                foreach (var custom in payload.ExtraFondoPostCabeceras[0]) persisted.Add(custom == null ? "-" : S.R(custom.Height));

                return (State: stateMap,
                    Legends: new[] { fondoOnePostTwo, fondoOnePostOne, fondoTwoPostOne },
                    Resolved: S.CustomMap(system, 2, 4),
                    Persisted: string.Join(" ", persisted),
                    Corresponds: S.Corresponds(payload, system));
            });

            Assert.Equal("0/0=- 0/1=150 0/2=- 0/3=160 1/0=170 1/1=- 1/2=- 1/3=x", r.State);
            Assert.Equal("Personalizada · fondo 1", r.Legends[0]);
            Assert.Equal("Por defecto (del tramo) · fondo 1", r.Legends[1]);
            Assert.Equal("Personalizada · fondo 2", r.Legends[2]);
            Assert.Equal(r.State, r.Resolved);                    // lo que dibuja cada vista lee exactamente eso
            Assert.Equal("- 150 - 160 | 170 - -", r.Persisted);   // Actualizar no pierde ni inventa ninguna
            Assert.True(r.Corresponds);
        }
    }
}
