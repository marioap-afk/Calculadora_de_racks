using System;
using System.Collections.Generic;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Pure placement of the PARRILLA (deck) block, parameterized by view (I-22, E6). The count rule — how many decks
    /// fit a load row — stays in <see cref="SelectiveFrontalBuilder.ParrillaRow"/> (the single source consumed by the
    /// frontal draw, the lateral gate and the BOM). What was copy-pasted is the deck INSTANCE construction: a
    /// <see cref="HeaderBlockRole.Safety"/> block whose origin is its BOTTOM-LEFT corner, sized by one span parameter
    /// (FRENTE seen edge-on in the frontal, FONDO in the lateral). That rule now lives here.
    /// </summary>
    public static class SelectiveParrillaPlacement
    {
        /// <summary>One deck block whose bottom-left corner is (<paramref name="footprintLeftX"/>,
        /// <paramref name="bottomY"/>); <paramref name="spanParam"/> = <paramref name="spanValue"/> sizes it (FRENTE in
        /// the frontal, FONDO in the lateral).</summary>
        public static HeaderBlockInstance Deck(
            string pieceId, string block, string view, double footprintLeftX, double bottomY, string spanParam, double spanValue)
        {
            var at = new Point2D(footprintLeftX, bottomY);
            var instance = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Safety,
                PieceId = pieceId,
                BlockName = block,
                View = view,
                Insertion = at,
                ConnectionAnchor = at
            };
            instance.DynamicParameters[spanParam] = spanValue;
            return instance;
        }

        /// <summary>Distribute <paramref name="count"/> decks of width <paramref name="frente"/> across
        /// [<paramref name="anchorX"/>, +<paramref name="span"/>] resting at <paramref name="bottomY"/> (the frontal
        /// view): each at its bottom-left corner, evenly spaced. No-op when the row holds no deck.</summary>
        public static void AppendRow(
            ICollection<HeaderBlockInstance> target, string pieceId, string block, string view,
            double anchorX, double span, double bottomY, double frente, int count, string frenteParam)
        {
            if (frente <= 0.0 || count <= 0)
            {
                return;
            }

            var gap = Math.Max(0.0, (span - count * frente) / (count + 1));
            for (var k = 0; k < count; k++)
            {
                target.Add(Deck(pieceId, block, view, anchorX + gap * (k + 1) + frente * k, bottomY, frenteParam, frente));
            }
        }
    }
}
