using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// PB-004 — el OBJETIVO nominal de la pendiente: 7/16" por pie de rack.
    ///
    /// Tras el rechazo de la validación round 1, el Owner precisó que ese número es un objetivo, no la subida final
    /// literal: los dos largueros de extremo tienen que quedar atornillados a un troquel, y la subida resultante es la
    /// que salga de ese ajuste. La regla completa —ancla en el posterior, derivación del bajo y ajuste— se fija en
    /// <see cref="PushBackBedAnchorTests"/>; aquí queda solo la función del objetivo y el aislamiento del Dinámico.
    /// </summary>
    public class PushBackBedSlopeTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        [Fact]
        public void Slope_IsSevenSixteenthsPerCommercialFoot()
        {
            Assert.Equal(7.0 / 16.0, PushBackBedSlope.RisePerFoot, 12);
            Assert.Equal(0.0, PushBackBedSlope.Rise(-10.0), 12);

            // Es una razón pura: la subida escala linealmente con el recorrido, sin números mágicos por rack.
            Assert.Equal(2.0 * PushBackBedSlope.Rise(120.0), PushBackBedSlope.Rise(240.0), 9);
            Assert.Equal(204.0 / 12.0 * (7.0 / 16.0), PushBackBedSlope.Rise(204.0), 9);
        }

        /// <summary>
        /// La cama DIBUJADA persigue ese objetivo pero no lo clava: el ajuste al troquel de los extremos puede
        /// desviarla hasta medio paso. Esta es la garantía que sí se puede dar.
        /// </summary>
        [Fact]
        public void TheDrawnBed_TracksTheNominalTarget_WithinHalfATroquelStep()
        {
            var catalog = Catalog;
            var system = new PushBackResolver(catalog).Resolve(new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 4,
                    LoadLevels = 3,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            });

            var rails = new PushBackSystemLateralBuilder()
                .Build(system, catalog)
                .Flatten()
                .Instances
                .Where(instance => string.Equals(instance.PieceId, FlowBedDefaults.RailId, StringComparison.OrdinalIgnoreCase))
                .ToList();
            Assert.NotEmpty(rails);

            foreach (var rail in rails)
            {
                var longitud = rail.DynamicParameters[SelectiveRackDefaults.LengthParam];
                var drawnRise = longitud * Math.Sin(rail.RotationRadians);
                var nominal = PushBackBedSlope.Rise(longitud);

                Assert.True(drawnRise > 0.0, "la cama tiene que bajar hacia la salida");
                Assert.True(
                    Math.Abs(drawnRise - nominal) <= SelectiveRackDefaults.TroquelPaso / 2.0 + 1e-6,
                    FormattableString.Invariant($"subida dibujada {drawnRise:0.####} contra el objetivo {nominal:0.####}"));
            }
        }

        // ---- Aislamiento: la cama DINÁMICA conserva su propia regla ----

        [Fact]
        public void DynamicBed_KeepsItsOwnSnappedRise_Untouched()
        {
            var catalog = Catalog;
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 3,
                FirstLevelHeight = 6.0,
                BeamDepth = 4.0
            };
            var system = new DynamicRackSystemResolver(catalog).Resolve(design).System;
            var front = system.Fronts[0];
            var levels = DynamicFrontGeometry.LoadBeamLevels(system, front);

            var axes = DynamicFlowBedGeometry.Resolve(system, catalog, front);
            Assert.NotEmpty(axes);
            foreach (var axis in axes)
            {
                var level = levels.First(entry => entry.LevelNumber == axis.LevelNumber);
                Assert.Equal(level.EntranceElevation - level.ExitElevation, axis.Rise, 9);
            }
        }
    }
}
