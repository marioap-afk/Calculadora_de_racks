using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.PushBack;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// T5 pb, the review of 24adfee7 (Push Back in the RackCad window): the corrections that live in the draft of Application. «En blanco» goes
    /// by the path of the classic window (point 1) and the «T» of the matrix follows the type «Ninguno» of the tope (point 4). The base of
    /// «Descartar» (point 3), the gate of the BOM (point 2) and PB-S10 in «Nuevo» (point 6) are tested on the window (UI).
    /// </summary>
    public sealed class PushBackReviewCorrectionsTests
    {
        private static PushBackDesign DesignOf(string fixture)
            => new RackProjectStore().Deserialize(new RackEmbedStore().Deserialize(PilotKit.Text(fixture)).Design).PushBackDesign;

        // A rack of two sides with both present in every slot.
        private static PushBackDraftEditor TwoSided(int slots)
        {
            var draft = PushBackDraftEditor.CreateNew(PilotKit.Catalog);
            draft.SetSlotCount(slots);
            draft.SetSideBPresent(true);
            for (var slot = 0; slot < slots; slot++) Assert.Null(draft.SetBlank(PushBackSide.B, slot, false));
            Assert.True(draft.Recompose(), draft.LastError);
            return draft;
        }

        [Fact]
        public void BlankWithBothSidesWritesTheTwoSidesAsTheClassicWindowDoes()
        {
            var draft = TwoSided(2);

            // Before the correction, A went blank first and the rule of B saw A absent and refused the whole gesture.
            Assert.Null(draft.SetBlank(new[] { PushBackSide.A, PushBackSide.B }, 0, true));
            Assert.False(draft.IsPresent(PushBackSide.A, 0));
            Assert.False(draft.IsPresent(PushBackSide.B, 0));
            Assert.True(draft.Recompose(), draft.LastError);

            // And back: both present again.
            Assert.Null(draft.SetBlank(new[] { PushBackSide.A, PushBackSide.B }, 0, false));
            Assert.True(draft.IsPresent(PushBackSide.A, 0));
            Assert.True(draft.IsPresent(PushBackSide.B, 0));
        }

        [Fact]
        public void ASideCanStayEntirelyBlankWhileTheOtherHoldsTheRackAndTheRackKeepsOneFront()
        {
            var draft = TwoSided(2);

            // B entirely blank while A holds the rack (SetSlotPresent with allowAllBlank), as the classic window allows.
            Assert.Null(draft.SetBlank(PushBackSide.B, 0, true));
            Assert.Null(draft.SetBlank(PushBackSide.B, 1, true));
            Assert.False(draft.IsPresent(PushBackSide.B, 0) || draft.IsPresent(PushBackSide.B, 1));

            // A slot that is absent in B can go blank in A while A keeps another front.
            Assert.Null(draft.SetBlank(PushBackSide.A, 0, true));
            Assert.True(draft.Recompose(), draft.LastError);

            // The last front of the rack stays: with both sides, the gesture is refused only when no side took it.
            Assert.Equal("Al menos un frente del rack debe permanecer activo.", draft.SetBlank(new[] { PushBackSide.A, PushBackSide.B }, 1, true));
            Assert.True(draft.IsPresent(PushBackSide.A, 1));

            // Nothing to change is no refusal.
            Assert.Null(draft.SetBlank(new[] { PushBackSide.A, PushBackSide.B }, 0, true));
        }

        [Fact]
        public void TheTopeOfTheMatrixFollowsTheTypeNoneOfItsSide()
        {
            var draft = new PushBackDraftEditor(PilotKit.Catalog);
            Assert.True(draft.LoadFrom(DesignOf("pb-01-base"), "PB 1"), draft.LastError);
            var cell = draft.AllCells().First(c => c.Side == PushBackSide.A);
            Assert.Equal(1, draft.SetTope(new[] { (cell.Front, cell.Level) }, true));
            Assert.True(draft.Recompose(), draft.LastError);
            Assert.True(draft.TopeDrawnAt(PushBackSide.A, cell.Front, cell.Level));

            draft.Side(PushBackSide.A).RearTopePieceId = PushBackDefaults.NonePieceId;
            Assert.True(draft.Recompose(), draft.LastError);
            Assert.False(draft.TopeDrawnAt(PushBackSide.A, cell.Front, cell.Level)); // what «Extremos» counts: none
            Assert.True(draft.TopeIntentAt(PushBackSide.A, cell.Front, cell.Level)); // the intent stays, dormant
            Assert.False(draft.Side(PushBackSide.A).RearTopeConfig().Draws(cell.Front, cell.Level));
        }
    }
}
