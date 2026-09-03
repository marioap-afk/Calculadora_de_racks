using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;
using RackCad.UI.Systems.Selective;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-43, gate 8.6D (ARQ-43-03), a través de la VENTANA real: al abrir un rack —nuevo o cargado— todo frente
    /// tiene ya su elevación DIRECTA, sin que el usuario pulse nada (INV-12).
    /// <para>
    /// Es donde el defecto se veía: la carga materializaba antes de restaurar la matriz de trabajo y se saltaba el
    /// fondo seleccionado, así que el slot 0 se quedaba con nulos y volvía a la fila viva acto seguido.
    /// </para>
    /// </summary>
    public sealed class SelectiveFloorBeamRiseLoadTests
    {
        private const string PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA";
        private const string BeamId = "LARGUERO_3_REMACHES";

        /// <summary>Un documento anterior a ID14: un valor global y NINGUNA elevación por frente.</summary>
        private static SelectivePalletDesignDocument LegacyDocument(double globalRise, int fondos, int frentes)
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId,
                PostPeralte = 3.0,
                PalletTolerance = 4.0,
                VerticalClearance = 6.0,
                FloorBeamRise = globalRise,
                PalletDepth = 48.0,
                DepthCount = fondos
            };

            SelectiveBayDesign Bay()
            {
                var bay = new SelectiveBayDesign { FloorBeam = true, FloorBeamRiseOverride = null };
                for (var l = 0; l < 2; l++)
                {
                    bay.Levels.Add(new SelectiveCell
                    {
                        Pallet = new Tarima { Frente = 42.0, Alto = 48.0 },
                        PalletCount = 2,
                        BeamId = BeamId,
                        BeamPeralte = 4.0
                    });
                }

                return bay;
            }

            for (var b = 0; b < frentes; b++) design.Bays.Add(Bay());
            for (var k = 1; k < fondos; k++)
            {
                design.ExtraFondoBays.Add(Enumerable.Range(0, frentes).Select(_ => Bay()).ToList());
            }

            var document = SelectivePalletDesignDocument.From(design, "GUID-8-6D", "Legacy");
            foreach (var bay in document.Bays) bay.FloorBeamRiseOverride = null;
            foreach (var bay in document.ExtraFondoBays.SelectMany(f => f)) bay.FloorBeamRiseOverride = null;
            return document;
        }

        private static double?[] RisesOf(RackSelectiveWindow window, int fondo)
        {
            var state = window.EditorState;
            var frentes = fondo == state.SelectedFondo ? state.Bays.Count : state.FondoMatrices[fondo].Bays.Count;
            return Enumerable.Range(0, frentes).Select(f => state.FloorBeamRiseOverrideAt(fondo, f)).ToArray();
        }

        // ---- P0 I: un documento legacy queda materializado en TODOS los fondos ----

        [Fact]
        public void I_ALegacyDocument_MaterializesItsGlobalOnEveryFondoAndFrente()
        {
            var (fondo0, fondo1) = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open();
                window.LoadForNew(LegacyDocument(globalRise: 7.0, fondos: 2, frentes: 2));
                return (RisesOf(window, 0), RisesOf(window, 1));
            });

            Assert.Equal(new double?[] { 7.0, 7.0 }, fondo0); // el slot del fondo SELECCIONADO también
            Assert.Equal(new double?[] { 7.0, 7.0 }, fondo1);
        }

        [Fact]
        public void I_ALegacyDocument_ThenEmitsSevenInBothBaysAndExtraFondoBays()
        {
            var (bays, extra) = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open();
                window.LoadForNew(LegacyDocument(globalRise: 7.0, fondos: 2, frentes: 2));
                var design = window.BuildDesignForTest(out _);
                return (
                    design.Bays.Select(b => b.FloorBeamRiseOverride).ToArray(),
                    design.ExtraFondoBays.SelectMany(f => f).Select(b => b.FloorBeamRiseOverride).ToArray());
            });

            Assert.Equal(new double?[] { 7.0, 7.0 }, bays);
            Assert.Equal(new double?[] { 7.0, 7.0 }, extra);
        }

        [Fact]
        public void I_ALegacyGlobalOfZero_MaterializesZero_NotTheDefault()
        {
            // 0 ES una elevación ("ninguna"); solo un valor negativo cae al default.
            var rises = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open();
                window.LoadForNew(LegacyDocument(globalRise: 0.0, fondos: 2, frentes: 2));
                return RisesOf(window, 0).Concat(RisesOf(window, 1)).ToArray();
            });

            Assert.All(rises, rise => Assert.Equal(0.0, rise));
        }

        // ---- P0 J: una ventana nueva ya nace con el valor directo ----

        [Fact]
        public void J_AFreshWindow_HasTheDefaultOnEveryFrenteWithoutAnyOperation()
        {
            var rises = StaTestRunner.Run(() => RisesOf(SelectiveWindowTestSupport.Open(), 0));

            Assert.NotEmpty(rises);
            Assert.All(rises, rise => Assert.Equal(SelectiveRackDefaults.DefaultFloorBeamRise, rise));
        }

        [Fact]
        public void J_AFreshWindow_EmitsTheDefaultInBuildDesign()
        {
            var emitted = StaTestRunner.Run(() =>
                SelectiveWindowTestSupport.Open().BuildDesignForTest(out _).Bays
                    .Select(b => b.FloorBeamRiseOverride).ToArray());

            Assert.NotEmpty(emitted);
            Assert.All(emitted, rise => Assert.Equal(SelectiveRackDefaults.DefaultFloorBeamRise, rise));
        }

        [Fact]
        public void J_GrowingTheFondosFromAFreshWindow_KeepsEveryFrenteDirect()
        {
            var rises = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(3);
                return Enumerable.Range(0, 3).SelectMany(k => RisesOf(window, k)).ToArray();
            });

            Assert.NotEmpty(rises);
            Assert.DoesNotContain(rises, rise => !rise.HasValue);
        }
    }
}
