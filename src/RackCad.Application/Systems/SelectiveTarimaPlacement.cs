using System;
using System.Collections.Generic;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Pure placement of the TARIMA (pallet) VISUAL reference, parameterized by view (I-22, E6). The frontal and the
    /// lateral distribute pallets differently — the frontal spreads a load row's pallets across the frente span; the
    /// lateral draws one edge-on pallet per fondo spanning the depth — but both build the SAME pallet block instance:
    /// the TARIMA block's origin is BOTTOM-CENTRE, its LONGITUD carries the horizontal span (frente in the frontal,
    /// fondo in the lateral) and its ALTURA the pallet alto. That single instance rule now lives here instead of being
    /// copy-pasted in each builder. Never in the BOM (<see cref="HeaderBlockRole.Pallet"/>).
    /// </summary>
    public static class SelectiveTarimaPlacement
    {
        /// <summary>One TARIMA block instance whose footprint starts at <paramref name="footprintLeftX"/> and rests at
        /// <paramref name="bottomY"/>. The block origin is bottom-centre, so it is inserted at footprint-left +
        /// <paramref name="longitud"/>/2; LONGITUD = <paramref name="longitud"/>, ALTURA = <paramref name="alto"/>.</summary>
        public static HeaderBlockInstance Pallet(
            string block, string view, double footprintLeftX, double bottomY, double longitud, double alto)
        {
            var at = new Point2D(footprintLeftX + longitud / 2.0, bottomY);
            var pallet = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Pallet,
                PieceId = SelectiveRackDefaults.PalletPieceId,
                BlockName = block,
                View = view,
                Insertion = at,
                ConnectionAnchor = at
            };
            pallet.DynamicParameters[SelectiveRackDefaults.PalletFrenteParam] = longitud;
            pallet.DynamicParameters[SelectiveRackDefaults.PalletAltoParam] = alto;
            return pallet;
        }

        /// <summary>Distribute <paramref name="count"/> pallets of width <paramref name="frente"/> evenly across
        /// [<paramref name="anchorX"/>, +<paramref name="span"/>] resting at <paramref name="bottomY"/> (the frontal
        /// view). Each pallet is centred in its slot via <see cref="Pallet"/>.</summary>
        public static void AppendRow(
            ICollection<HeaderBlockInstance> target, string block, string view,
            double anchorX, double span, double bottomY, double frente, double alto, int count)
        {
            var gap = Math.Max(0.0, (span - count * frente) / (count + 1));
            for (var k = 0; k < count; k++)
            {
                // Footprint left = anchorX + gap*(k+1) + frente*k; Pallet centres it and keeps its bottom at bottomY.
                target.Add(Pallet(block, view, anchorX + gap * (k + 1) + frente * k, bottomY, frente, alto));
            }
        }
    }
}
