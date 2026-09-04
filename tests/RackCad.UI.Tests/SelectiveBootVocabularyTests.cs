using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;
using RackCad.UI;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-43, gate 8.6H (R2-10): la BOTA se llama distinto en cada sistema, y ese vocabulario no puede filtrarse de uno
    /// a otro.
    /// <para>
    /// En el Selectivo la bota se coloca por LADO —izquierda o derecha del poste—, que es como el usuario la ve y como
    /// se llamó siempre. I-42 introdujo para Push Back un vocabulario por UBICACIÓN —«Entrada/Salida» es su pasillo de
    /// carga y «Posterior» la cara opuesta— y lo dejó como único vocabulario de la ventana compartida, así que el
    /// Selectivo empezó a pedir al usuario que eligiera entre dos caras que su sistema no tiene.
    /// </para>
    /// <para>
    /// Los ORDINALES no cambian: el mismo valor persiste igual y una selección guardada sigue significando lo mismo.
    /// Lo único que cambia es cómo se nombran las cuatro opciones.
    /// </para>
    /// </summary>
    public sealed class SelectiveBootVocabularyTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        /// <summary>La ventana de seguridad tal como la abre CADA anfitrión.</summary>
        private static SelectiveSafetyWindow SafetyWindowFor(bool bootFamilyInSections, bool dynamicDefaults, bool placementNames = false)
            => new SelectiveSafetyWindow(
                Catalog?.SafetyElements ?? new List<SafetyElementCatalogEntry>(),
                new List<SelectiveSafetySelection>(),
                postCount: 3,
                levelsPerFrente: new[] { 2, 2 },
                fondoCount: 1,
                parrillaPlan: null,
                catalog: Catalog,
                resolvedSystem: null,
                fallbackLevelsArePerPost: false,
                introduction: null,
                includeDefensa: false,
                includeGuia: false,
                useDynamicSafetyDefaults: dynamicDefaults,
                extraSection: null,
                desviadorLevelsPerPost: null,
                defensaLowEndOnly: false,
                allowBlankFrontColumns: false,
                showDesviadorSide: true,
                bootFamilyInSections: bootFamilyInSections,
                bootUsesPlacementNames: placementNames);

        // =====================================================================================
        // Selectivo: por LADO
        // =====================================================================================

        [Fact]
        public void Selective_TheBootIsPlacedBySide_NotByPushBackLocation()
        {
            var options = StaTestRunner.Run(() => SafetyWindowFor(false, false).BootRowOptionsForTest.ToArray());

            Assert.Equal(new[] { "Ninguno", "Izquierda", "Derecha", "Ambas" }, options);
        }

        [Fact]
        public void Selective_TheBootNeverOffersPushBackWording()
        {
            var options = StaTestRunner.Run(() => SafetyWindowFor(false, false).BootRowOptionsForTest.ToArray());

            Assert.DoesNotContain("Entrada/Salida", options);
            Assert.DoesNotContain("Posterior", options);
        }

        // =====================================================================================
        // Push Back: por UBICACIÓN, y en SU propia sección
        // =====================================================================================

        [Fact]
        public void PushBack_KeepsItsOwnLocationWording_InItsOwnSection()
        {
            // Push Back no usa la fila del diálogo compartido: la familia entera vive en su sección por lado, con su
            // propio vocabulario. Cambiar el de la ventana compartida no puede alcanzarle.
            var options = StaTestRunner.Run(() => PushBackBootSection.ModeLabels.ToArray());

            Assert.Equal(new[] { "Ninguno", "Entrada/Salida", "Posterior", "Ambas" }, options);
        }

        [Fact]
        public void PushBack_DoesNotBuildABootRowInTheSharedDialog()
        {
            var built = StaTestRunner.Run(() => SafetyWindowFor(true, false).BuildsBootRowForTest);

            Assert.False(built); // por eso su vocabulario es intocable desde aquí
        }

        // =====================================================================================
        // Dinámico: caracterización — se conserva EXACTAMENTE lo que hace hoy
        // =====================================================================================

        [Fact]
        public void Dynamic_KeepsTodaysWording_Unchanged()
        {
            // CARACTERIZACIÓN, no decisión. El vocabulario por ubicación llegó al Dinámico como efecto colateral de
            // I-42 (Push Back) y no hay ninguna decisión registrada para este sistema, así que este gate lo deja
            // EXACTAMENTE como está y lo reporta. Si el dueño decide que el Dinámico también va por lado, esta prueba
            // es el sitio donde se ve el cambio.
            // Los mismos argumentos con los que RackDynamicSystemWindow abre la ventana.
            var options = StaTestRunner.Run(() => SafetyWindowFor(false, true, placementNames: true).BootRowOptionsForTest.ToArray());

            Assert.Equal(new[] { "Ninguno", "Entrada/Salida", "Posterior", "Ambas" }, options);
        }
    }
}
