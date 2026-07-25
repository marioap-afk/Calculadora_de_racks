using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// PB-004 (I-32), la mitad que NO se toca — «sin doble elevación de cabecera».
    ///
    /// El dueño reportó que la pendiente excesiva «provoca que la cabecera sea más alta». La tentación evidente es
    /// sumarle a la cabecera la subida de la cama; eso sería exactamente la doble elevación que la decisión prohíbe,
    /// porque la altura ya la incluye UNA vez a través de la elevación de entrada del nivel superior.
    ///
    /// Estas pruebas fijan las dos caras del invariante: la cabecera se calcula desde la envolvente canónica del
    /// nivel más alto (entrada + peralte + un tercio de claro) y NO depende del eje de la cama; y bajar la pendiente
    /// baja la cabecera por esa vía, no por un término añadido.
    /// </summary>
    public class PushBackHeaderHeightTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackDesign Design(int palletsDeep) => new PushBackDesign
        {
            Structure = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = palletsDeep,
                LoadLevels = 3,
                FirstLevelHeight = 6.0,
                BeamDepth = 4.0
            }
        };

        /// <summary>
        /// La altura teórica es EXACTAMENTE la envolvente del nivel superior. Si alguien sumara la subida de la cama,
        /// esta igualdad se rompe: es el centinela de la doble elevación.
        /// </summary>
        [Fact]
        public void HeaderHeight_IsTheCanonicalTopEnvelope_WithNoBedRiseAddedOnTop()
        {
            var catalog = Catalog;
            var system = new PushBackResolver(catalog).Resolve(Design(4));
            var front = system.Structure.Fronts[0];

            var result = DynamicHeaderHeightCalculator.CalculateResolved(front);
            var top = front.LoadBeamLevels.OrderBy(level => level.LevelNumber).Last();
            var topCell = DynamicRackLevelGeometry.At(null, front, top.LevelNumber);
            var expected = top.EntranceElevation
                           + topCell.InOutBeamDepth
                           + (topCell.Pallet.Height + topCell.ClearHeight) * DynamicHeaderHeightCalculator.TopFinishFraction;

            Assert.Equal(expected, result.TheoreticalHeight, 9);
            Assert.Equal(
                DynamicHeaderHeightCalculator.RoundUpToCommercialFoot(expected),
                result.HeaderHeight,
                9);
        }

        /// <summary>
        /// La altura no conoce el eje de la cama. El término de pendiente que reporta es la delta de elevaciones YA
        /// ajustada al troquel, no la subida de la cama: si alguien hiciera que la cabecera consumiera el eje, ambas
        /// coincidirían. Tras PB-004 son distintas a propósito (la cama sube por la razón nominal, las elevaciones
        /// están ajustadas a 2"), así que esta desigualdad es el centinela.
        /// </summary>
        [Fact]
        public void HeaderHeight_ReadsTheSnappedLevelDelta_NotTheBedAxisRise()
        {
            var catalog = Catalog;
            var system = new PushBackResolver(catalog).Resolve(Design(4));
            var front = system.Structure.Fronts[0];

            var result = DynamicHeaderHeightCalculator.CalculateResolved(front);
            var first = front.LoadBeamLevels.OrderBy(level => level.LevelNumber).First();
            var axes = PushBackFlowBedGeometry.Resolve(system, catalog, front);
            Assert.NotEmpty(axes);

            Assert.Equal(first.EntranceElevation - first.ExitElevation, result.Slope, 9);
            Assert.NotEqual(axes[0].Rise, result.Slope, 3);

            // The theoretical height carries the slope exactly ONCE, through the top entrance elevation: adding the
            // bed rise on top is a strictly different number, and that number must never be what the calculator returns.
            Assert.NotEqual(result.TheoreticalHeight + axes[0].Rise, result.TheoreticalHeight, 6);
            Assert.Equal(result.HeaderHeight, DynamicHeaderHeightCalculator.CalculateResolved(front).HeaderHeight, 9);
        }

        /// <summary>
        /// La cabecera SÍ baja al corregir la pendiente, pero por la vía correcta: la elevación de entrada del nivel
        /// superior. Con la pendiente nominal (7/16" por pie) un rack profundo pide menos altura que con la subida
        /// desbocada que medía el dueño.
        /// </summary>
        [Fact]
        public void ADeeperRack_NeedsMoreHeader_ThroughTheTopEntranceElevationOnly()
        {
            var catalog = Catalog;
            var shallow = new PushBackResolver(catalog).Resolve(Design(3)).Structure.Fronts[0];
            var deep = new PushBackResolver(catalog).Resolve(Design(8)).Structure.Fronts[0];

            var shallowTop = shallow.LoadBeamLevels.OrderBy(l => l.LevelNumber).Last();
            var deepTop = deep.LoadBeamLevels.OrderBy(l => l.LevelNumber).Last();

            // The only term that grows with depth is the entrance elevation of the top level.
            Assert.True(deepTop.EntranceElevation > shallowTop.EntranceElevation);
            Assert.Equal(shallowTop.ExitElevation, deepTop.ExitElevation, 9);

            var shallowHeight = DynamicHeaderHeightCalculator.CalculateResolved(shallow).TheoreticalHeight;
            var deepHeight = DynamicHeaderHeightCalculator.CalculateResolved(deep).TheoreticalHeight;
            Assert.Equal(
                deepTop.EntranceElevation - shallowTop.EntranceElevation,
                deepHeight - shallowHeight,
                9);
        }
    }
}
