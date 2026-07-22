using System.Collections.Generic;
using RackCad.Application.Headers;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>Unit coverage for the pure PARRILLA (deck) placement service (I-22, E6): the deck-instance rule the
    /// frontal and lateral builders now share (bottom-left origin, one span param). The count rule stays in
    /// <see cref="SelectiveFrontalBuilder.ParrillaRow"/>.</summary>
    public class SelectiveParrillaPlacementTests
    {
        [Fact]
        public void Deck_OriginIsBottomLeft_WithTheGivenSpanParam()
        {
            var deck = SelectiveParrillaPlacement.Deck("PARRILLA", "PARRILLA_LATERAL", "LATERAL", footprintLeftX: 5.0, bottomY: 30.0, spanParam: SelectiveSafetyDefaults.ParrillaFondoParam, spanValue: 42.0);

            Assert.Equal(HeaderBlockRole.Safety, deck.Role);
            Assert.Equal("LATERAL", deck.View);
            Assert.Equal(5.0, deck.Insertion.X, 6);   // bottom-left, not centred
            Assert.Equal(30.0, deck.Insertion.Y, 6);
            Assert.False(deck.DynamicParameters.ContainsKey(SelectiveSafetyDefaults.ParrillaFrenteParam));
            Assert.Equal(42.0, deck.DynamicParameters[SelectiveSafetyDefaults.ParrillaFondoParam], 6);
        }

        [Fact]
        public void AppendRow_DistributesDecksAtBottomLeftCorners()
        {
            var target = new List<HeaderBlockInstance>();
            // span 136, 3 decks of 40 -> gap (136-120)/4 = 4; corners at 4, 48, 92.
            SelectiveParrillaPlacement.AppendRow(target, "PARRILLA", "B", "FRONTAL", anchorX: 0.0, span: 136.0, bottomY: 0.0, frente: 40.0, count: 3, frenteParam: SelectiveSafetyDefaults.ParrillaFrenteParam);

            Assert.Equal(3, target.Count);
            Assert.Equal(4.0, target[0].Insertion.X, 6);
            Assert.Equal(48.0, target[1].Insertion.X, 6);
            Assert.Equal(92.0, target[2].Insertion.X, 6);
            Assert.All(target, d => Assert.Equal(40.0, d.DynamicParameters[SelectiveSafetyDefaults.ParrillaFrenteParam], 6));
        }

        [Fact]
        public void AppendRow_ZeroOrNonPositive_DrawsNothing()
        {
            var target = new List<HeaderBlockInstance>();
            SelectiveParrillaPlacement.AppendRow(target, "PARRILLA", "B", "FRONTAL", 0.0, 100.0, 0.0, 40.0, count: 0, frenteParam: SelectiveSafetyDefaults.ParrillaFrenteParam);
            SelectiveParrillaPlacement.AppendRow(target, "PARRILLA", "B", "FRONTAL", 0.0, 100.0, 0.0, 0.0, count: 2, frenteParam: SelectiveSafetyDefaults.ParrillaFrenteParam);
            Assert.Empty(target);
        }
    }
}
