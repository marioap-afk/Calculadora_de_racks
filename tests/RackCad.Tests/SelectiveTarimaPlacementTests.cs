using System.Collections.Generic;
using RackCad.Application.Headers;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>Unit coverage for the pure TARIMA placement service (I-22, E6): the single pallet-instance rule both
    /// the frontal and lateral builders now share.</summary>
    public class SelectiveTarimaPlacementTests
    {
        [Fact]
        public void Pallet_OriginIsBottomCentre_WithLongitudAndAlturaParams()
        {
            var pallet = SelectiveTarimaPlacement.Pallet("TARIMA_GENERICA_FRONTAL", "FRONTAL", footprintLeftX: 10.0, bottomY: 20.0, longitud: 40.0, alto: 45.0);

            Assert.Equal(HeaderBlockRole.Pallet, pallet.Role);
            Assert.Equal(SelectiveRackDefaults.PalletPieceId, pallet.PieceId);
            Assert.Equal("TARIMA_GENERICA_FRONTAL", pallet.BlockName);
            Assert.Equal(30.0, pallet.Insertion.X, 6);   // footprint-left 10 + longitud/2 (20)
            Assert.Equal(20.0, pallet.Insertion.Y, 6);   // bottom kept at bottomY
            Assert.Equal(pallet.Insertion.X, pallet.ConnectionAnchor.X, 6);
            Assert.Equal(40.0, pallet.DynamicParameters[SelectiveRackDefaults.PalletFrenteParam], 6);
            Assert.Equal(45.0, pallet.DynamicParameters[SelectiveRackDefaults.PalletAltoParam], 6);
        }

        [Fact]
        public void AppendRow_DistributesCountEvenly_WithGapsAroundEachPallet()
        {
            var target = new List<HeaderBlockInstance>();
            // span 100, 2 pallets of 20 -> total 40, remaining 60 over 3 gaps = 20 each.
            SelectiveTarimaPlacement.AppendRow(target, "B", "FRONTAL", anchorX: 0.0, span: 100.0, bottomY: 0.0, frente: 20.0, alto: 30.0, count: 2);

            Assert.Equal(2, target.Count);
            // pallet 0 footprint-left = 0 + 20*1 + 20*0 = 20 -> centre 30; pallet 1 footprint-left = 0 + 20*2 + 20*1 = 60 -> centre 70.
            Assert.Equal(30.0, target[0].Insertion.X, 6);
            Assert.Equal(70.0, target[1].Insertion.X, 6);
            Assert.All(target, p => Assert.Equal(20.0, p.DynamicParameters[SelectiveRackDefaults.PalletFrenteParam], 6));
        }

        [Fact]
        public void AppendRow_ZeroCount_DrawsNothing()
        {
            var target = new List<HeaderBlockInstance>();
            SelectiveTarimaPlacement.AppendRow(target, "B", "FRONTAL", 0.0, 100.0, 0.0, 20.0, 30.0, count: 0);
            Assert.Empty(target);
        }
    }
}
